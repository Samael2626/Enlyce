$ErrorActionPreference = 'Stop'

$source = Join-Path $PSScriptRoot '../docs/avance-paridad-wasi-2026-10-23.md'
$destination = 'D:\Brain\10-Proyectos\Enlyce\Avance-Paridad-Wasi-2026-10-23.md'
$moc = 'D:\Brain\10-Proyectos\Enlyce\Enlyce-MOC.md'
if (Test-Path -LiteralPath $destination) {
    throw 'La nota de avance del vault ya existe.'
}
$header = @'
---
title: "Avance paridad Wasi 2026-10-23"
date: 2026-10-23
tags: [enlyce, wasi, crm]
---

'@
$body = [System.IO.File]::ReadAllText($source)
[System.IO.File]::WriteAllText($destination, $header + $body, [System.Text.UTF8Encoding]::new($false))

$mocContent = [System.IO.File]::ReadAllText($moc)
$link = '[[Avance-Paridad-Wasi-2026-10-23]]'
if (-not $mocContent.Contains($link)) {
    $addition = @'

- [[Avance-Paridad-Wasi-2026-10-23]]
'@
    [System.IO.File]::AppendAllText($moc, $addition, [System.Text.UTF8Encoding]::new($false))
}
Write-Output 'Avance publicado en vault y enlazado desde MOC.'
