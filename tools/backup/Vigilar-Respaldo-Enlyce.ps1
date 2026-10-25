param(
    [ValidateSet('Install', 'Check', 'SelfTest', 'TestEmail')]
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

    try {
        $message = [Net.Mail.MailMessage]::new($config.Sender, $config.Recipient, $Subject, $Body)
        $client = [Net.Mail.SmtpClient]::new('smtp.gmail.com', 587)
        try {
            $client.EnableSsl = $true
            $client.UseDefaultCredentials = $false
            $client.Credentials = [Net.NetworkCredential]::new($config.Sender, $plainPassword)
            $client.Timeout = 30000
            $client.Send($message)
        }
        finally {
            $client.Dispose()
            $message.Dispose()
        }
    }
    finally { $plainPassword = $null }
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
    @{ Sender = $sender; Recipient = $recipient; EncryptedPassword = ($password | ConvertFrom-SecureString) } |
        ConvertTo-Json | Set-Content -LiteralPath $configPath -Encoding UTF8

    $scriptPath = Join-Path $PSScriptRoot 'Vigilar-Respaldo-Enlyce.ps1'
    $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`" -Mode Check"
    $trigger = New-ScheduledTaskTrigger -Daily -At '4:00 AM'
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 5) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
    Register-ScheduledTask -TaskName $monitorTaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
    Write-Host "Monitor instalado: $monitorTaskName, 4:00 a. m."
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
    Write-Host '4 casos de monitor aprobados.'
}

switch ($Mode) {
    'Install' { Install-Monitor }
    'Check' { Invoke-Monitor }
    'SelfTest' { Test-Monitor }
    'TestEmail' {
        Send-BackupAlert -Subject 'Enlyce: prueba del monitor de respaldos' -Body 'Prueba de alerta. No se ha detectado un fallo de respaldo.'
        Write-Host 'Correo de prueba aceptado por SMTP.'
    }
}
