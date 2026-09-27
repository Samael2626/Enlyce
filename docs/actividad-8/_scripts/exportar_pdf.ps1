param(
    [Parameter(Mandatory = $true)]
    [string]$InputDocx,

    [Parameter(Mandatory = $true)]
    [string]$OutputPdf
)

$word = $null
$document = $null

try {
    $outputDirectory = Split-Path -Parent $OutputPdf
    if (-not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    }

    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $word.DisplayAlerts = 0

    $document = $word.Documents.Open($InputDocx, $false, $true)

    $document.Repaginate()
    $document.ExportAsFixedFormat($OutputPdf, 17)

    Write-Output "PDF=$OutputPdf"
    Write-Output "PAGES=$($document.ComputeStatistics(2))"
}
finally {
    if ($null -ne $document) {
        $document.Close($false)
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($document) | Out-Null
    }

    if ($null -ne $word) {
        $word.Quit()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
    }

    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
