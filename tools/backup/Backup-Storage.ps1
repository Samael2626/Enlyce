function Get-BackupRetentionPlan {
    param(
        [object[]] $Objects,
        [DateTimeOffset] $Now = [DateTimeOffset]::UtcNow
    )

    $Now = $Now.ToUniversalTime()
    $valid = @($Objects | ForEach-Object {
        $stamp = [DateTime]::MinValue
        $created = [DateTimeOffset]::MinValue
        if ($_.name -cmatch '^enlyce-(\d{8}-\d{6})\.dump$' -and
            [DateTime]::TryParseExact($Matches[1], 'yyyyMMdd-HHmmss', [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::None, [ref] $stamp) -and
            $_.id -and $_.metadata -and $_.metadata.size -gt 0 -and
            [DateTimeOffset]::TryParse([string] $_.created_at, [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::AssumeUniversal, [ref] $created)) {
            [PSCustomObject] @{ Name = [string] $_.name; CreatedAt = $created.ToUniversalTime() }
        }
    })
    if (@($valid | Group-Object Name | Where-Object Count -gt 1).Count -gt 0) {
        throw 'El listado contiene nombres duplicados; retencion cancelada.'
    }

    $keep = @{}
    foreach ($item in @($valid | Where-Object { $_.CreatedAt -le $Now } |
        Sort-Object CreatedAt, Name -Descending | Select-Object -First 7)) {
        $keep[$item.Name] = $true
    }
    foreach ($item in $valid) {
        if ($item.CreatedAt -ge $Now.AddDays(-30)) { $keep[$item.Name] = $true }
    }
    $monthStart = [DateTimeOffset]::new($Now.Year, $Now.Month, 1, 0, 0, 0, [TimeSpan]::Zero).AddMonths(-11)
    $monthly = @($valid | Where-Object { $_.CreatedAt -ge $monthStart -and $_.CreatedAt -le $Now } |
        Sort-Object CreatedAt, Name | Group-Object { $_.CreatedAt.ToString('yyyy-MM') })
    foreach ($month in $monthly) { $keep[$month.Group[0].Name] = $true }

    [PSCustomObject] @{
        Keep = @($valid | Where-Object { $keep.ContainsKey($_.Name) } | ForEach-Object Name)
        Delete = @($valid | Where-Object { -not $keep.ContainsKey($_.Name) } | ForEach-Object Name)
    }
}

function Get-RemoteBackupObjects {
    param([string] $ProjectUrl, [string] $Bucket, [hashtable] $Headers, [int] $PageSize = 1000)
    $objects = New-Object 'System.Collections.Generic.List[object]'
    $seen = @{}
    $offset = 0
    do {
        $body = @{ prefix = ''; limit = $PageSize; offset = $offset; sortBy = @{ column = 'name'; order = 'asc' } } | ConvertTo-Json -Depth 4
        $page = @(Invoke-RestMethod -Uri "$ProjectUrl/storage/v1/object/list/$Bucket" -Method Post -Headers $Headers `
            -ContentType 'application/json' -Body $body -TimeoutSec 30 -MaximumRedirection 0)
        foreach ($item in $page) {
            if (-not $item -or -not $item.name -or $seen.ContainsKey([string] $item.name)) {
                throw 'Listado remoto incompleto o repetido; no se borrara ningun objeto.'
            }
            $seen[[string] $item.name] = $true
            $objects.Add($item)
        }
        $offset += $page.Count
        if ($offset -gt 100000) { throw 'Listado remoto fuera del limite de seguridad.' }
    } while ($page.Count -eq $PageSize)
    return $objects.ToArray()
}

function Invoke-RemoteBackupRetention {
    param([string] $ProjectUrl, [string] $Bucket, [hashtable] $Headers,
        [string] $UploadedName, [long] $UploadedSize, [DateTimeOffset] $Now = [DateTimeOffset]::UtcNow)

    $objects = @(Get-RemoteBackupObjects -ProjectUrl $ProjectUrl -Bucket $Bucket -Headers $Headers)
    $uploaded = @($objects | Where-Object { $_.name -ceq $UploadedName -and $_.id -and $_.metadata.size -eq $UploadedSize })
    if ($UploadedSize -le 0 -or $uploaded.Count -ne 1) {
        throw 'No se pudo confirmar la nueva copia remota con su tamano; retencion cancelada.'
    }
    $plan = Get-BackupRetentionPlan -Objects $objects -Now $Now
    if ($plan.Keep -cnotcontains $UploadedName) { throw 'La nueva copia no esta protegida; retencion cancelada.' }
    for ($offset = 0; $offset -lt $plan.Delete.Count; $offset += 100) {
        $names = @($plan.Delete | Select-Object -Skip $offset -First 100)
        $body = @{ prefixes = $names } | ConvertTo-Json -Depth 3 -Compress
        $deleted = @(Invoke-RestMethod -Uri "$ProjectUrl/storage/v1/object/$Bucket" -Method Delete -Headers $Headers `
            -ContentType 'application/json' -Body $body -TimeoutSec 30 -MaximumRedirection 0)
        $actual = @($deleted | ForEach-Object name | Sort-Object)
        if (($actual -join "`n") -cne (($names | Sort-Object) -join "`n")) {
            throw 'Supabase no confirmo todos los borrados solicitados.'
        }
    }
    return $plan
}

function Publish-BackupArtifact {
    param([string] $ProjectUrl, [string] $Bucket, [string] $ApiKey, [IO.FileInfo] $File)
    $headers = @{ apikey = $ApiKey }
    if (-not $ApiKey.StartsWith('sb_secret_')) { $headers.Authorization = "Bearer $ApiKey" }
    $uploadHeaders = $headers.Clone()
    $uploadHeaders['x-upsert'] = 'false'
    $objectName = [Uri]::EscapeDataString($File.Name)
    Invoke-WebRequest -Uri "$ProjectUrl/storage/v1/object/$Bucket/$objectName" -Method Post `
        -Headers $uploadHeaders -ContentType 'application/octet-stream' -InFile $File.FullName `
        -UseBasicParsing -TimeoutSec 120 -MaximumRedirection 0 | Out-Null
    Invoke-RemoteBackupRetention -ProjectUrl $ProjectUrl -Bucket $Bucket -Headers $headers `
        -UploadedName $File.Name -UploadedSize $File.Length
}

function Test-BackupPingUrl([string] $PingUrl) {
    $uri = $null
    return [Uri]::TryCreate($PingUrl, [UriKind]::Absolute, [ref] $uri) -and
        $uri.Scheme -eq 'https' -and $uri.Host -eq 'hc-ping.com' -and $uri.Port -eq 443 -and
        -not $uri.UserInfo -and -not $uri.Query -and -not $uri.Fragment -and
        $uri.AbsolutePath -match '^/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$'
}

function Send-BackupSignal {
    param([string] $PingUrl, [ValidateSet('start', 'success', 'fail')] [string] $Signal, [Guid] $RunId)
    if (-not (Test-BackupPingUrl $PingUrl)) { throw 'URL de monitoreo invalida.' }
    $suffix = if ($Signal -eq 'success') { '' } else { "/$Signal" }
    try {
        $response = Invoke-WebRequest -Uri "$PingUrl$suffix`?rid=$RunId" -Method Get -UseBasicParsing `
            -TimeoutSec 10 -MaximumRedirection 0
        return ([string] $response.Content).Trim() -ceq 'OK'
    }
    catch { return $false }
}

function Invoke-MonitoredBackup {
    param([string] $PingUrl, [scriptblock] $Work)
    $runId = [Guid]::NewGuid()
    if ($PingUrl) {
        if (-not (Test-BackupPingUrl $PingUrl)) { throw 'URL de monitoreo invalida.' }
        if (-not (Send-BackupSignal -PingUrl $PingUrl -Signal start -RunId $runId)) {
            Write-BackupLog 'WARN No se confirmo el aviso de inicio; se intentara el respaldo.'
        }
    }
    else { Write-BackupLog 'WARN Monitoreo externo SIN CONFIGURAR' }
    try {
        & $Work
        if ($PingUrl -and -not (Send-BackupSignal -PingUrl $PingUrl -Signal success -RunId $runId)) {
            throw 'Respaldo completado, pero no se confirmo el aviso de exito.'
        }
    }
    catch {
        Write-BackupLog 'ERROR El respaldo o su confirmacion de monitoreo no se completo'
        if ($PingUrl -and -not (Send-BackupSignal -PingUrl $PingUrl -Signal fail -RunId $runId)) {
            Write-BackupLog 'WARN No se confirmo el aviso de fallo; el servicio detectara la ausencia.'
        }
        throw
    }
}
