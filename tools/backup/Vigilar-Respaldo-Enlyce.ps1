param(
    [ValidateSet('Install', 'Check', 'SelfTest', 'TestEmail', 'TestAlerts')]
    [string] $Mode = 'Check'
)

$ErrorActionPreference = 'Stop'
$taskName = 'Enlyce-Postgres-Backup'
$monitorTaskName = 'Enlyce-Backup-Monitor'
$root = Join-Path $env:LOCALAPPDATA 'Enlyce\Backups'
$configPath = Join-Path $root 'alert-config.json'
$sentPath = Join-Path $root 'last-alert.txt'
$logPath = Join-Path $root 'backup.log'

function Get-BackupHealth {
    param(
        [datetime] $ExpectedAt,
        [datetime] $LastRunAt,
        [int] $LastTaskResult,
        [Nullable[datetime]] $LastSuccessAt
    )

    if ($LastRunAt -lt $ExpectedAt) { return 'Missed' }
    if ($LastTaskResult -ne 0) { return 'Failed' }
    if ($null -eq $LastSuccessAt -or $LastSuccessAt -lt $ExpectedAt) { return 'Unconfirmed' }
    return 'Ok'
}

function Get-LastSuccessAt {
    if (-not (Test-Path -LiteralPath $logPath)) { return $null }
    $lines = Get-Content -LiteralPath $logPath -Tail 200
    for ($index = $lines.Count - 1; $index -ge 0; $index--) {
        if ($lines[$index] -notmatch '^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) OK enlyce-\d{8}-\d{6}\.dump \d+ bytes uploaded$') {
            continue
        }
        return [datetime]::ParseExact($Matches[1], 'yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture)
    }
    return $null
}

function Send-BackupAlert {
    param([string] $Subject, [string] $Body)

    if (-not (Test-Path -LiteralPath $configPath)) {
        throw 'Falta configurar el correo de alertas. Ejecuta el modo Install.'
    }
    $config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
    $securePassword = ConvertTo-SecureString $config.EncryptedPassword
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    try { $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }

    $mailPath = Join-Path $env:TEMP ([IO.Path]::GetRandomFileName())
    try {
        $plainPassword = $plainPassword -replace ' ', ''
        if ($plainPassword -notmatch '^[A-Za-z0-9]{16}$') {
            throw "La contraseña de aplicación debe tener 16 caracteres (recibidos: $($plainPassword.Length))."
        }
        $mail = "From: $($config.Sender)`r`nTo: $($config.Recipient)`r`nSubject: $Subject`r`nContent-Type: text/plain; charset=utf-8`r`n`r`n$Body`r`n"
        [IO.File]::WriteAllText($mailPath, $mail, [Text.UTF8Encoding]::new($false))
        $curlConfig = "user = `"$($config.Sender):$plainPassword`""
        $curlConfig | & curl.exe --config - --silent --show-error --fail --connect-timeout 10 --max-time 30 `
            --ssl-reqd --url 'smtp://smtp.gmail.com:587' --mail-from $config.Sender `
            --mail-rcpt $config.Recipient --upload-file $mailPath
        if ($LASTEXITCODE -ne 0) { throw "Gmail SMTP devolvió código $LASTEXITCODE." }
    }
    finally {
        $plainPassword = $null
        if (Test-Path -LiteralPath $mailPath) { Remove-Item -LiteralPath $mailPath -Force }
    }
}

function Protect-GmailPassword {
    param([securestring] $Password)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    try {
        $normalized = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) -replace ' ', ''
        if ($normalized -notmatch '^[A-Za-z0-9]{16}$') {
            throw 'La contraseña de aplicación debe tener 16 caracteres; se aceptan tres espacios separadores.'
        }
        return (ConvertTo-SecureString -String $normalized -AsPlainText -Force | ConvertFrom-SecureString)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
        $normalized = $null
    }
}

function Install-Monitor {
    if (-not (Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue)) {
        throw "No existe la tarea $taskName. Instala primero el respaldo."
    }
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    $sender = Read-Host 'Correo Gmail remitente'
    $recipient = Read-Host 'Correo destinatario de alertas (Enter usa remitente)'
    if ([string]::IsNullOrWhiteSpace($recipient)) { $recipient = $sender }
    [void][Net.Mail.MailAddress]::new($sender)
    [void][Net.Mail.MailAddress]::new($recipient)
    $password = Read-Host 'Contraseña de aplicación de Gmail (no se mostrará)' -AsSecureString
    $encryptedPassword = Protect-GmailPassword -Password $password
    @{ Sender = $sender; Recipient = $recipient; EncryptedPassword = $encryptedPassword } |
        ConvertTo-Json | Set-Content -LiteralPath $configPath -Encoding UTF8

    $scriptPath = Join-Path $PSScriptRoot 'Vigilar-Respaldo-Enlyce.ps1'
    $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`" -Mode Check"
    $trigger = New-ScheduledTaskTrigger -Daily -At '4:00 AM'
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 5) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
    Register-ScheduledTask -TaskName $monitorTaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
    Disable-ScheduledTask -TaskName $monitorTaskName | Out-Null
    Write-Host "Monitor configurado: $monitorTaskName, 4:00 a. m. Queda desactivado hasta probar correo y recepcion."
}

function Invoke-Monitor {
    $now = Get-Date
    $expectedDate = if ($now.Hour -lt 4) { $now.Date.AddDays(-1) } else { $now.Date }
    $expectedAt = $expectedDate.AddHours(2)
    $taskInfo = Get-ScheduledTaskInfo -TaskName $taskName
    $lastSuccessAt = Get-LastSuccessAt
    $health = Get-BackupHealth -ExpectedAt $expectedAt -LastRunAt $taskInfo.LastRunTime -LastTaskResult $taskInfo.LastTaskResult -LastSuccessAt $lastSuccessAt
    if ($health -eq 'Ok') {
        Write-Host "Respaldo confirmado para $($expectedDate.ToString('yyyy-MM-dd'))."
        return
    }

    $alertKey = "$($expectedDate.ToString('yyyy-MM-dd')):$health"
    if ((Test-Path -LiteralPath $sentPath) -and (Get-Content -LiteralPath $sentPath -Raw).Trim() -eq $alertKey) {
        Write-Host "Alerta ya enviada: $alertKey"
        return
    }
    $subject = "Enlyce: respaldo $health $($expectedDate.ToString('yyyy-MM-dd'))"
    $body = "Estado: $health`r`nEjecución esperada: $expectedAt`r`nÚltima ejecución: $($taskInfo.LastRunTime)`r`nCódigo de tarea: $($taskInfo.LastTaskResult)`r`nÚltimo respaldo confirmado: $lastSuccessAt"
    Send-BackupAlert -Subject $subject -Body $body
    Set-Content -LiteralPath $sentPath -Value $alertKey -Encoding ASCII
    Write-Host "Alerta enviada: $alertKey"
}

function Test-Monitor {
    $expected = [datetime]'2026-10-24T02:00:00'
    $cases = @(
        @{ LastRun = $expected.AddDays(-1); Result = 0; LastOk = $expected.AddDays(-1); Want = 'Missed' },
        @{ LastRun = $expected.AddMinutes(1); Result = 1; LastOk = $expected.AddDays(-1); Want = 'Failed' },
        @{ LastRun = $expected.AddMinutes(1); Result = 0; LastOk = $expected.AddDays(-1); Want = 'Unconfirmed' },
        @{ LastRun = $expected.AddMinutes(1); Result = 0; LastOk = $expected.AddMinutes(2); Want = 'Ok' }
    )
    foreach ($case in $cases) {
        $actual = Get-BackupHealth -ExpectedAt $expected -LastRunAt $case.LastRun -LastTaskResult $case.Result -LastSuccessAt $case.LastOk
        if ($actual -ne $case.Want) { throw "Esperado $($case.Want), recibido $actual" }
    }
    $sample = ConvertTo-SecureString -String 'abcd efgh ijkl mnop' -AsPlainText -Force
    $encrypted = Protect-GmailPassword -Password $sample
    if ((ConvertTo-SecureString $encrypted).Length -ne 16) {
        throw 'La contraseña de prueba no se normalizo a 16 caracteres.'
    }
    $invalidRejected = $false
    try { Protect-GmailPassword -Password (ConvertTo-SecureString -String 'abcdefghij' -AsPlainText -Force) | Out-Null }
    catch { $invalidRejected = $true }
    if (-not $invalidRejected) { throw 'Se acepto una contraseña de prueba corta.' }
    if (-not (Get-Command curl.exe -ErrorAction SilentlyContinue)) { throw 'Falta curl.exe para SMTP.' }
    Write-Host '4 casos de monitor, validacion de contraseña y curl aprobados.'
}

switch ($Mode) {
    'Install' { Install-Monitor }
    'Check' { Invoke-Monitor }
    'SelfTest' { Test-Monitor }
    'TestEmail' {
        Send-BackupAlert -Subject 'Enlyce: prueba del monitor de respaldos' -Body 'Prueba de alerta. No se ha detectado un fallo de respaldo.'
        Write-Host 'Correo de prueba aceptado por SMTP.'
    }
    'TestAlerts' {
        Send-BackupAlert -Subject 'Enlyce: PRUEBA respaldo Failed' -Body 'Prueba sintetica de fallo. El respaldo real no se ha modificado.'
        Send-BackupAlert -Subject 'Enlyce: PRUEBA respaldo Missed' -Body 'Prueba sintetica de ejecucion omitida. El respaldo real no se ha modificado.'
        Write-Host 'Dos alertas de prueba aceptadas por SMTP.'
    }
}
