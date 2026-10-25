param(
    [ValidateSet('Preview', 'SelfTest')]
    [string] $Mode = 'Preview'
)

$ErrorActionPreference = 'Stop'
$projectUrl = 'https://yxpsnkvnvaxobqfogtfe.supabase.co'
$bucket = 'enlyce-backups'
$root = Join-Path $env:LOCALAPPDATA 'Enlyce\Backups'
$secretPath = Join-Path $root 'supabase-key.dpapi'

function Get-SupabaseKey {
    $encrypted = (Get-Content -LiteralPath $secretPath -Raw).Trim()
    $secure = ConvertTo-SecureString $encrypted
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
}

function Get-RemoteObjects {
    $key = Get-SupabaseKey
    try {
        $headers = @{ apikey = $key; 'Content-Type' = 'application/json' }
        if (-not $key.StartsWith('sb_secret_')) { $headers.Authorization = "Bearer $key" }
        $items = [Collections.Generic.List[object]]::new()
        for ($offset = 0; $offset -lt 5000; $offset += 100) {
            $body = @{ prefix = ''; limit = 100; offset = $offset; sortBy = @{ column = 'name'; order = 'asc' } } |
                ConvertTo-Json -Depth 3 -Compress
            $page = @(Invoke-RestMethod -Uri "$projectUrl/storage/v1/object/list/$bucket" -Method Post -Headers $headers -Body $body)
            foreach ($item in $page) { $items.Add($item) }
            if ($page.Count -lt 100) { return $items.ToArray() }
        }
        throw 'La lista remota supera 5000 objetos. No se calcula retencion parcial.'
    }
    finally { $key = $null }
}

function Get-RetentionPlan {
    param([object[]] $Objects, [datetime] $Now)

    $recentStart = $Now.Date.AddDays(-6)
    $dailyStart = $Now.Date.AddDays(-29)
    $currentMonth = [datetime]::new($Now.Year, $Now.Month, 1)
    $monthlyStart = $currentMonth.AddMonths(-12)
    $dated = [Collections.Generic.List[object]]::new()
    $unknown = [Collections.Generic.List[object]]::new()
    foreach ($object in $Objects) {
        $match = [regex]::Match($object.name, '^enlyce-(\d{8})-(\d{6})\.dump$')
        if (-not $match.Success) { $unknown.Add($object); continue }
        $timestamp = [datetime]::MinValue
        $validDate = [datetime]::TryParseExact($match.Groups[1].Value + $match.Groups[2].Value,
            'yyyyMMddHHmmss', [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::None, [ref]$timestamp)
        if (-not $validDate) { $unknown.Add($object); continue }
        $dated.Add([pscustomobject]@{ Name = $object.name; Timestamp = $timestamp; Size = [long]$object.metadata.size })
    }

    $keep = [Collections.Generic.List[object]]::new()
    $candidates = [Collections.Generic.List[object]]::new()
    $days = [Collections.Generic.HashSet[string]]::new()
    $months = [Collections.Generic.HashSet[string]]::new()
    $ordered = @($dated | Sort-Object Timestamp -Descending)
    $latestName = if ($ordered.Count -gt 0) { $ordered[0].Name } else { $null }
    foreach ($item in $ordered) {
        $day = $item.Timestamp.ToString('yyyy-MM-dd')
        $month = $item.Timestamp.ToString('yyyy-MM')
        if ($item.Name -eq $latestName -or
            $item.Timestamp.Date -ge $recentStart -or
            ($item.Timestamp.Date -ge $dailyStart -and $days.Add($day)) -or
            ($item.Timestamp -ge $monthlyStart -and $item.Timestamp -lt $currentMonth -and $months.Add($month))) {
            $keep.Add($item)
        }
        else { $candidates.Add($item) }
    }
    [pscustomobject]@{ Keep = $keep.ToArray(); Candidates = $candidates.ToArray(); Unknown = $unknown.ToArray() }
}

function Test-RetentionPlan {
    $now = [datetime]'2026-10-25T09:00:00'
    $names = @(
        'enlyce-20261025-020000.dump', 'enlyce-20261020-020000.dump', 'enlyce-20261020-100000.dump',
        'enlyce-20261010-020000.dump', 'enlyce-20261010-100000.dump',
        'enlyce-20260915-020000.dump', 'enlyce-20260915-100000.dump',
        'enlyce-20240901-020000.dump', 'no-borrar.txt', 'enlyce-20261399-020000.dump'
    )
    $objects = @($names | ForEach-Object { [pscustomobject]@{ name = $_; metadata = @{ size = 100 } } })
    $plan = Get-RetentionPlan -Objects $objects -Now $now
    if ($plan.Keep.Count -ne 5 -or $plan.Candidates.Count -ne 3 -or $plan.Unknown.Count -ne 2) {
        throw "Plan inesperado: $($plan.Keep.Count)/$($plan.Candidates.Count)/$($plan.Unknown.Count)"
    }
    $stale = @('enlyce-20240101-020000.dump', 'enlyce-20230101-020000.dump') |
        ForEach-Object { [pscustomobject]@{ name = $_; metadata = @{ size = 100 } } }
    $stalePlan = Get-RetentionPlan -Objects $stale -Now $now
    if ($stalePlan.Keep.Count -ne 1 -or $stalePlan.Keep[0].Name -ne $stale[0].name -or $stalePlan.Candidates.Count -ne 1) {
        throw 'El plan debe conservar el respaldo mas reciente aunque todos sean antiguos.'
    }
    Write-Host 'Retencion sintetica aprobada: 5 conservar, 3 candidatos, 2 desconocidos protegidos; ultimo respaldo siempre protegido.'
}

if ($Mode -eq 'SelfTest') { Test-RetentionPlan; return }

$objects = Get-RemoteObjects
$plan = Get-RetentionPlan -Objects $objects -Now (Get-Date)
$totalBytes = [long]0
foreach ($item in $objects) { $totalBytes += [long]$item.metadata.size }
Write-Host "Objetos: $($objects.Count); bytes: $totalBytes; conservar: $($plan.Keep.Count); candidatos: $($plan.Candidates.Count); desconocidos protegidos: $($plan.Unknown.Count)"
foreach ($item in $plan.Candidates) { Write-Host "CANDIDATO $($item.Name) $($item.Size) bytes" }
Write-Host 'Solo vista previa. Este script no borra objetos.'
