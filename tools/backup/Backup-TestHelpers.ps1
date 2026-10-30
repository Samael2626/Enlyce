function New-TestBackup([string] $Date) {
    $created = [DateTimeOffset]::Parse($Date, [Globalization.CultureInfo]::InvariantCulture)
    [PSCustomObject] @{
        name = "enlyce-$($created.UtcDateTime.ToString('yyyyMMdd-HHmmss')).dump"
        id = [Guid]::NewGuid().ToString()
        created_at = $created.ToString('o')
        metadata = @{ size = 100 }
    }
}
