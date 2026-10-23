$ErrorActionPreference = 'Stop'

$source = Join-Path $PSScriptRoot '../docs/avance-paridad-wasi-2026-10-23.md'
$destination = 'D:\Brain\10-Proyectos\Enlyce\Avance-Paridad-Wasi-2026-10-23.md'
$header = @'
---
title: "Avance paridad Wasi 2026-10-23"
date: 2026-10-23
tags: [enlyce, wasi, crm]
---

'@
$body = [System.IO.File]::ReadAllText($source)
[System.IO.File]::WriteAllText($destination, $header + $body, [System.Text.UTF8Encoding]::new($false))
Write-Output 'Avance del vault sincronizado.'
