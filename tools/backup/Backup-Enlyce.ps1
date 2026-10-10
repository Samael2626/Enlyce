param(
    [ValidateSet('Install', 'Run')]
    [string] $Mode = 'Install'
)

$ErrorActionPreference = 'Stop'
$projectUrl = 'https://yxpsnkvnvaxobqfogfte.supabase.co'
$bucket = 'enlyce-backups'
$root = Join-Path $env:LOCALAPPDATA 'Enlyce\Backups'
$secretPath = Join-Path $root 'supabase-key.dpapi'
$taskName = 'Enlyce-Postgres-Backup'
$identityPath = Join-Path $HOME '.ssh\id_ed25519_enlyce'

function Write-BackupLog([string] $Message) {
    $path = Join-Path $root 'backup.log'
    Add-Content -LiteralPath $path -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $Message" -Encoding UTF8
}

function Get-SupabaseKey {
    if (-not (Test-Path -LiteralPath $secretPath)) {
        throw 'Falta la clave cifrada. Ejecuta el modo Install y configura una clave sb_secret_.'
    }

    $secureKey = Get-Content -LiteralPath $secretPath -Raw | ConvertTo-SecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureKey)
    try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
}

function Test-LegacyServiceRoleKey([string] $Key) {
    $parts = $Key.Split('.')
    if ($parts.Count -ne 3) { return $false }

    try {
        $payload = $parts[1].Replace('-', '+').Replace('_', '/')
        $payload += '=' * ((4 - ($payload.Length % 4)) % 4)
        $claims = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload)) | ConvertFrom-Json
        return $claims.role -eq 'service_role'
    }
    catch { return $false }
}

function Install-Backup {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    if (-not (Test-Path -LiteralPath $identityPath)) { throw "No existe la clave Railway: $identityPath" }
    if (-not (Get-Command railway -ErrorAction SilentlyContinue)) { throw 'Railway CLI no esta instalado o no esta en PATH.' }

    $secureKey = Read-Host 'Pega la clave secreta sb_secret_ de Supabase (no se mostrara)' -AsSecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureKey)
    try { $plainKey = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
    if (-not $plainKey.StartsWith('sb_secret_') -and -not (Test-LegacyServiceRoleKey $plainKey)) {
        throw 'Clave invalida: usa sb_secret_ o la clave legacy service_role del proyecto Enlyce-backups. No uses publishable/anon ni la clave de Postgres.'
    }
    $secureKey | ConvertFrom-SecureString | Set-Content -LiteralPath $secretPath -Encoding ASCII
    $plainKey = $null
    $secureKey = $null

    $scriptPath = Join-Path $PSScriptRoot 'Backup-Enlyce.ps1'
    $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`" -Mode Run"
    $trigger = New-ScheduledTaskTrigger -Daily -At '2:00 AM'
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 1) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
    Write-Host "Tarea diaria instalada: $taskName, 2:00 AM (hora local)."
    Write-Host 'Windows debe estar encendido y tu sesion abierta para ejecutar el respaldo.'
}

function Invoke-Backup {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $dumpPath = Join-Path $root "enlyce-$stamp.dump"
    $errorPath = Join-Path $root "enlyce-$stamp.stderr.log"
    $remoteKeyAdded = $false
    $railway = $null
    $fingerprint = $null

    try {
        if (-not (Test-Path -LiteralPath $identityPath)) { throw 'Falta la clave SSH de Railway.' }
        $railway = (Get-Command railway -ErrorAction Stop).Source
        $keyInfo = & ssh-keygen.exe -lf "$identityPath.pub"
        if ($LASTEXITCODE -ne 0 -or $keyInfo -notmatch '(SHA256:\S+)') { throw 'No se pudo leer la huella de la clave SSH.' }
        $fingerprint = $Matches[1]
        $repository = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
        Push-Location $repository
        try {
            $registeredKeys = & $railway ssh keys list
            if ($LASTEXITCODE -ne 0) { throw 'No se pudo consultar las claves SSH registradas en Railway.' }
            $registeredText = ($registeredKeys -join "`n").Split('Local Keys (not registered):')[0]
            if ($registeredText.Contains($fingerprint)) {
                & $railway ssh keys remove $fingerprint | Out-Null
                if ($LASTEXITCODE -ne 0) { throw 'No se pudo retirar la clave SSH temporal anterior.' }
            }
            & $railway ssh keys add --key $identityPath --name 'EnlyceBackupAuto'
            if ($LASTEXITCODE -ne 0) { throw 'Railway rechazo el registro temporal de la clave SSH.' }
            $remoteKeyAdded = $true

            $command = '"{0}" ssh --service Postgres --environment production -i "{1}" -- pg_dump --format=custom --no-owner --dbname=$DATABASE_URL > "{2}" 2> "{3}"' -f $railway, $identityPath, $dumpPath, $errorPath
            & $env:ComSpec /d /s /c $command
            if ($LASTEXITCODE -ne 0) {
                $detail = if (Test-Path -LiteralPath $errorPath) { (Get-Content -LiteralPath $errorPath -Raw).Trim() } else { 'sin detalle' }
                throw "Railway pg_dump fallo: $detail"
            }
        }
        finally {
            if ($remoteKeyAdded) {
                try {
                    & $railway ssh keys remove $fingerprint | Out-Null
                    if ($LASTEXITCODE -ne 0) { Write-BackupLog 'WARN Railway no confirmo la retirada de la clave SSH temporal' }
                }
                catch { Write-BackupLog 'WARN Fallo al retirar la clave SSH temporal' }
            }
            Pop-Location
        }

        $file = Get-Item -LiteralPath $dumpPath
        if ($file.Length -lt 5 -or $file.Length -ge 49000000) { throw "Tamano de dump fuera del limite seguro para el bucket: $($file.Length) bytes." }
        $stream = [IO.File]::OpenRead($dumpPath)
        try {
            $magic = New-Object byte[] 5
            [void] $stream.Read($magic, 0, 5)
            if ([Text.Encoding]::ASCII.GetString($magic) -ne 'PGDMP') { throw 'El archivo no tiene encabezado de dump PostgreSQL custom.' }
        }
        finally { $stream.Dispose() }

        $apiKey = Get-SupabaseKey
        $headers = @{ apikey = $apiKey; 'Content-Type' = 'application/octet-stream'; 'x-upsert' = 'false' }
        if (-not $apiKey.StartsWith('sb_secret_')) { $headers.Authorization = "Bearer $apiKey" }
        $objectName = [Uri]::EscapeDataString($file.Name)
        $uploadUrl = "$projectUrl/storage/v1/object/$bucket/$objectName"
        Invoke-WebRequest -Uri $uploadUrl -Method Post -Headers $headers -InFile $dumpPath -UseBasicParsing | Out-Null
        $apiKey = $null
        Write-BackupLog "OK $($file.Name) $($file.Length) bytes uploaded"

        Get-ChildItem -LiteralPath $root -Filter 'enlyce-*.dump' -File |
            Where-Object LastWriteTime -lt (Get-Date).AddDays(-7) |
            Remove-Item -Force
        Remove-Item -LiteralPath $errorPath -Force -ErrorAction SilentlyContinue
    }
    catch {
        Write-BackupLog "ERROR $($_.Exception.Message)"
        throw
    }
    finally {
        if (Test-Path -LiteralPath $errorPath) { Remove-Item -LiteralPath $errorPath -Force -ErrorAction SilentlyContinue }
    }
}

if ($Mode -eq 'Install') { Install-Backup }
else { Invoke-Backup }
