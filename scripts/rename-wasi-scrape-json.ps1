$ErrorActionPreference = 'Stop'

$inventoryPath = Join-Path $PSScriptRoot '../docs/inventario-json-scrape-wasi.md'
$sourceRoot = 'C:\Users\USUARIO\Downloads\Wasi\Inventarios'
$copyRoot = 'D:\Brain\10-Proyectos\Enlyce\evidencia\wasi-scrape'
$rows = foreach ($line in Get-Content -LiteralPath $inventoryPath -Encoding UTF8) {
    if ($line -match '^\| (\d{13}) \| `([^`]+\.json)` \|') {
        [pscustomobject]@{
            Old = "wasi-ui-wasi.co-$($Matches[1]).json"
            New = $Matches[2]
        }
    }
}

if ($rows.Count -ne 28 -or @($rows.New | Sort-Object -Unique).Count -ne 28) {
    throw 'El inventario debe contener 28 nombres nuevos unicos.'
}

foreach ($row in $rows) {
    $source = Join-Path $sourceRoot $row.Old
    $copy = Join-Path $copyRoot $row.Old
    if (-not (Test-Path -LiteralPath $source -PathType Leaf) -or
        -not (Test-Path -LiteralPath $copy -PathType Leaf)) {
        throw "Falta un archivo de $($row.Old)."
    }
    if ((Test-Path -LiteralPath (Join-Path $sourceRoot $row.New)) -or
        (Test-Path -LiteralPath (Join-Path $copyRoot $row.New))) {
        throw "Nombre destino ocupado: $($row.New)."
    }
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash) {
        throw "Las copias difieren: $($row.Old)."
    }
}

foreach ($row in $rows) {
    Rename-Item -LiteralPath (Join-Path $sourceRoot $row.Old) -NewName $row.New
    Rename-Item -LiteralPath (Join-Path $copyRoot $row.Old) -NewName $row.New
}

Write-Output "Renombrados $($rows.Count) pares identicos."
