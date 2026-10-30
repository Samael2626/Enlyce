. (Join-Path $PSScriptRoot 'Backup-Storage.ps1')

. (Join-Path $PSScriptRoot 'Backup-TestHelpers.ps1')

Describe 'Retencion de backups' {
    $now = [DateTimeOffset]::Parse('2026-10-30T12:00:00Z')

    It 'Conserva todas las copias de los ultimos 30 dias, incluido el limite' {
        $items = @(0..30 | ForEach-Object { New-TestBackup ($now.AddDays(-$_).ToString('o')) })
        $plan = Get-BackupRetentionPlan -Objects $items -Now $now
        $plan.Keep.Count | Should Be 31
        $plan.Delete.Count | Should Be 0
    }

    It 'Conserva la primera copia de cada uno de los ultimos 12 meses calendario' {
        $items = @()
        foreach ($offset in 0..11) {
            $month = [DateTimeOffset]::Parse('2026-10-01T00:00:00Z').AddMonths(-$offset)
            $items += New-TestBackup ($month.ToString('o'))
            $items += New-TestBackup ($month.AddDays(14).ToString('o'))
        }
        $plan = Get-BackupRetentionPlan -Objects $items -Now $now
        foreach ($offset in 0..11) {
            $name = 'enlyce-' + ([DateTimeOffset]::Parse('2026-10-01T00:00:00Z').AddMonths(-$offset).ToString('yyyyMMdd-HHmmss')) + '.dump'
            ($plan.Keep -contains $name) | Should Be $true
        }
        ($plan.Delete -contains 'enlyce-20251115-000000.dump') | Should Be $true
    }

    It 'Conserva al menos siete copias aunque todas sean antiguas' {
        $items = @(1..10 | ForEach-Object { New-TestBackup "2024-01-$($_.ToString('00'))T00:00:00Z" })
        $plan = Get-BackupRetentionPlan -Objects $items -Now $now
        $plan.Keep.Count | Should Be 7
        $plan.Delete.Count | Should Be 3
        ($plan.Keep -contains 'enlyce-20240110-000000.dump') | Should Be $true
    }

    It 'No borra cuando hay menos de siete copias' {
        $items = @(1..3 | ForEach-Object { New-TestBackup "2024-01-0$($_)T00:00:00Z" })
        (Get-BackupRetentionPlan -Objects $items -Now $now).Delete.Count | Should Be 0
    }

    It 'Ignora otros archivos, carpetas, nombres invalidos y metadatos incompletos' {
        $items = @(
            @{ name = '../enlyce-20240101-000000.dump'; id = 'x'; created_at = '2024-01-01'; metadata = @{ size = 10 } },
            @{ name = 'enlyce-20249999-000000.dump'; id = 'x'; created_at = '2024-01-01'; metadata = @{ size = 10 } },
            @{ name = 'otro.dump'; id = 'x'; created_at = '2024-01-01'; metadata = @{ size = 10 } },
            @{ name = 'enlyce-20240101-000000.dump'; id = $null; created_at = '2024-01-01'; metadata = $null }
        )
        $plan = Get-BackupRetentionPlan -Objects $items -Now $now
        $plan.Keep.Count | Should Be 0
        $plan.Delete.Count | Should Be 0
    }

    It 'Usa created_at y no una fecha manipulada del nombre para decidir antiguedad' {
        $item = New-TestBackup '2026-10-29T00:00:00Z'
        $item.name = 'enlyce-20200101-000000.dump'
        (Get-BackupRetentionPlan -Objects @($item) -Now $now).Keep.Count | Should Be 1
    }

    It 'Protege fechas futuras sin descontarlas del minimo de siete copias anteriores' {
        $items = @(1..10 | ForEach-Object { New-TestBackup "2024-01-$($_.ToString('00'))T00:00:00Z" })
        $items += @(1..8 | ForEach-Object { New-TestBackup ($now.AddDays($_).ToString('o')) })
        $plan = Get-BackupRetentionPlan -Objects $items -Now $now
        $plan.Keep.Count | Should Be 15
        $plan.Delete.Count | Should Be 3
    }

    It 'Cancela con nombres duplicados antes de producir un plan' {
        $item = New-TestBackup '2024-01-01T00:00:00Z'
        { Get-BackupRetentionPlan -Objects @($item, $item) -Now $now } | Should Throw
    }
}
