$ErrorActionPreference = 'Stop'

$inventoryPath = Join-Path $PSScriptRoot '../docs/inventario-json-scrape-wasi.md'
$paths = @(
    (Join-Path $PSScriptRoot '../docs/evidencia-scrape-wasi.md'),
    'D:\Brain\10-Proyectos\Enlyce\Ingenieria-Inversa-Wasi.md'
)
$rows = foreach ($line in Get-Content -LiteralPath $inventoryPath -Encoding UTF8) {
    if ($line -match '^\| (\d{13}) \| `([^`]+\.json)` \|') {
        [pscustomobject]@{
            Old = "wasi-ui-wasi.co-$($Matches[1]).json"
            New = $Matches[2]
        }
    }
}

foreach ($path in $paths) {
    $content = [System.IO.File]::ReadAllText($path)
    foreach ($row in $rows) {
        $content = $content.Replace($row.Old, $row.New)
    }
    [System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($false))
}

Write-Output 'Referencias actualizadas en repo y vault.'
