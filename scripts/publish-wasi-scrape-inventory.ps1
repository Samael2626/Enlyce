$ErrorActionPreference = 'Stop'

$source = Join-Path $PSScriptRoot '../docs/inventario-json-scrape-wasi.md'
$destination = 'D:\Brain\10-Proyectos\Enlyce\Inventario-JSON-Scrape-Wasi-2026-10-23.md'
$moc = 'D:\Brain\10-Proyectos\Enlyce\Enlyce-MOC.md'
if (Test-Path -LiteralPath $destination) {
    throw 'El inventario del vault ya existe.'
}
$header = @'
---
title: "Inventario JSON scrape Wasi 2026-10-23"
date: 2026-10-23
tags: [enlyce, wasi, evidencia]
---

'@
$body = [System.IO.File]::ReadAllText($source)
[System.IO.File]::WriteAllText($destination, $header + $body, [System.Text.UTF8Encoding]::new($false))

$mocContent = [System.IO.File]::ReadAllText($moc)
$link = '[[Inventario-JSON-Scrape-Wasi-2026-10-23]]'
if (-not $mocContent.Contains($link)) {
    $addition = @'

## Evidencia de paridad Wasi

- [[Inventario-JSON-Scrape-Wasi-2026-10-23]]
'@
    [System.IO.File]::AppendAllText($moc, $addition, [System.Text.UTF8Encoding]::new($false))
}
Write-Output 'Inventario publicado en vault y enlazado desde MOC.'
