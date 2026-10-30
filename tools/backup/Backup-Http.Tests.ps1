. (Join-Path $PSScriptRoot 'Backup-Enlyce.ps1')
. (Join-Path $PSScriptRoot 'Backup-TestHelpers.ps1')
Set-Item Function:Invoke-RemoteBackupRetentionOriginal -Value ${function:Invoke-RemoteBackupRetention}
Set-Item Function:Send-BackupSignalOriginal -Value ${function:Send-BackupSignal}

Describe 'Integracion simulada de Storage y healthchecks' {
    BeforeEach {
        $script:requests = @()
        Mock Write-BackupLog { }
        Mock Send-BackupSignal {
            param($PingUrl, $Signal, $RunId)
            Send-BackupSignalOriginal -PingUrl $PingUrl -Signal $Signal -RunId $RunId
        }
        Mock Invoke-WebRequest { throw 'HTTP real prohibido en estas pruebas.' }
        Mock Invoke-RestMethod { throw 'HTTP real prohibido en estas pruebas.' }
    }

    It 'Lista todas las paginas antes de decidir borrados con orden estable' {
        $script:rows = @(1..3 | ForEach-Object { New-TestBackup "2024-01-0$($_)T00:00:00Z" })
        Mock Invoke-RestMethod {
            param($Uri, $Method, $Headers, $Body)
            $data = $Body | ConvertFrom-Json
            $script:requests += @{ Uri = $Uri; Method = $Method; Data = $data }
            @($script:rows | Select-Object -Skip $data.offset -First $data.limit)
        }
        $items = @(Get-RemoteBackupObjects -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{ apikey = 'test' } -PageSize 2)
        $items.Count | Should Be 3
        $script:requests.Count | Should Be 2
        $script:requests[0].Uri | Should Be 'https://storage.example.test/storage/v1/object/list/enlyce-backups'
        $script:requests[0].Data.prefix | Should Be ''
        $script:requests[0].Data.sortBy.column | Should Be 'name'
        $script:requests[1].Data.offset | Should Be 2
        Assert-MockCalled Invoke-RestMethod -Scope It -Times 0 -ParameterFilter { $Method -eq 'Delete' }
    }

    It 'Cancela si una pagina se repite' {
        $script:row = New-TestBackup '2024-01-01T00:00:00Z'
        Mock Invoke-RestMethod { $script:row }
        { Get-RemoteBackupObjects -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{} -PageSize 1 } | Should Throw
    }

    It 'Sube sin sobrescribir, confirma tamano y borra unicamente los nombres exactos del plan' {
        $path = Join-Path $TestDrive 'enlyce-20261030-120000.dump'
        [IO.File]::WriteAllBytes($path, [Text.Encoding]::ASCII.GetBytes('PGDMP'))
        $script:rows = @(1..10 | ForEach-Object { New-TestBackup "2024-01-$($_.ToString('00'))T00:00:00Z" })
        $new = New-TestBackup '2026-10-30T12:00:00Z'
        $new.metadata.size = 5
        $script:rows += $new
        Mock Invoke-WebRequest {
            param($Uri, $Method, $Headers, $InFile)
            $script:requests += @{ Kind = 'upload'; Uri = $Uri; Headers = $Headers; InFile = $InFile }
        }
        Mock Invoke-RestMethod {
            param($Uri, $Method, $Headers, $Body)
            $data = $Body | ConvertFrom-Json
            $script:requests += @{ Kind = $Method; Uri = $Uri; Data = $data; Headers = $Headers }
            if ($Method -eq 'Delete') { @($data.prefixes | ForEach-Object { @{ name = $_ } }) }
            else { $script:rows }
        }
        # La fecha fija se inyecta en la retencion para evitar pruebas dependientes del reloj.
        Mock Invoke-RemoteBackupRetention {
            param($ProjectUrl, $Bucket, $Headers, $UploadedName, $UploadedSize)
            Invoke-RemoteBackupRetentionOriginal -ProjectUrl $ProjectUrl -Bucket $Bucket -Headers $Headers -UploadedName $UploadedName -UploadedSize $UploadedSize -Now ([DateTimeOffset]::Parse('2026-10-30T12:00:00Z'))
        }
        $plan = Publish-BackupArtifact -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -ApiKey 'sb_secret_test-only' -File (Get-Item $path)
        $script:requests[0].Kind | Should Be 'upload'
        $script:requests[0].Headers['x-upsert'] | Should Be 'false'
        $script:requests[0].Headers.ContainsKey('Authorization') | Should Be $false
        $delete = @($script:requests | Where-Object Kind -eq 'Delete')
        $delete.Count | Should Be 1
        $delete[0].Uri | Should Be 'https://storage.example.test/storage/v1/object/enlyce-backups'
        ($delete[0].Data.prefixes -join ',') | Should Be 'enlyce-20240101-000000.dump,enlyce-20240102-000000.dump,enlyce-20240103-000000.dump,enlyce-20240104-000000.dump'
        $plan.Keep.Count | Should Be 7
    }

    It 'No lista ni borra si falla la subida' {
        $path = Join-Path $TestDrive 'enlyce-20261030-120000.dump'
        [IO.File]::WriteAllText($path, 'PGDMP')
        Mock Invoke-WebRequest { throw 'fallo simulado' }
        { Publish-BackupArtifact -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -ApiKey 'test' -File (Get-Item $path) } | Should Throw
        Assert-MockCalled Invoke-RestMethod -Scope It -Times 0
    }

    It 'No borra si no aparece la nueva copia con su tamano exacto' {
        Mock Invoke-RestMethod { @() }
        { Invoke-RemoteBackupRetention -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{} -UploadedName 'enlyce-20261030-120000.dump' -UploadedSize 100 } | Should Throw
        Assert-MockCalled Invoke-RestMethod -Scope It -Times 0 -ParameterFilter { $Method -eq 'Delete' }
    }

    It 'Envia inicio y exito con el mismo identificador sin cuerpo ni claves Supabase' {
        Mock Invoke-WebRequest {
            param($Uri, $Method, $Headers, $Body)
            $script:requests += @{ Uri = $Uri; Headers = $Headers; Body = $Body }
            @{ Content = 'OK' }
        }
        Invoke-MonitoredBackup -PingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111' -Work { }
        $script:requests.Count | Should Be 2
        $script:requests[0].Uri | Should Match '/start\?rid='
        $script:requests[1].Uri | Should Not Match '/start|/fail'
        (($script:requests[0].Uri -split 'rid=')[1]) | Should Be (($script:requests[1].Uri -split 'rid=')[1])
        $script:requests[0].Headers | Should BeNullOrEmpty
        $script:requests[0].Body | Should BeNullOrEmpty
    }

    It 'Envia fallo y conserva el error del trabajo sin enviar su detalle' {
        Mock Invoke-WebRequest {
            param($Uri, $Body)
            $script:requests += @{ Uri = $Uri; Body = $Body }
            @{ Content = 'OK' }
        }
        { Invoke-MonitoredBackup -PingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111' -Work { throw 'error-prueba' } } | Should Throw 'error-prueba'
        $script:requests.Count | Should Be 2
        $script:requests[1].Uri | Should Match '/fail\?rid='
        $script:requests[1].Body | Should BeNullOrEmpty
    }

    It 'Un fallo del aviso de inicio no impide hacer la copia' {
        $script:worked = $false
        Mock Send-BackupSignal { $Signal -ne 'start' }
        Invoke-MonitoredBackup -PingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111' -Work { $script:worked = $true }
        $script:worked | Should Be $true
        Assert-MockCalled Send-BackupSignal -Scope It -Times 1 -ParameterFilter { $Signal -eq 'success' }
    }

    It 'Rechaza el falso exito HTTP con contenido not found' {
        Mock Invoke-WebRequest { @{ Content = 'OK (not found)' } }
        (Send-BackupSignal -PingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111' -Signal success -RunId ([Guid]::NewGuid())) | Should Be $false
    }

    It 'Sin monitoreo configurado hace la copia y avisa en el log' {
        $script:worked = $false
        Invoke-MonitoredBackup -Work { $script:worked = $true }
        $script:worked | Should Be $true
        Assert-MockCalled Write-BackupLog -Scope It -Times 1 -ParameterFilter { $Message -match 'SIN CONFIGURAR' }
        Assert-MockCalled Invoke-WebRequest -Scope It -Times 0
    }

    It 'Rechaza URL de monitoreo con otro host o datos adicionales' {
        (Test-BackupPingUrl 'http://hc-ping.com/11111111-1111-1111-1111-111111111111') | Should Be $false
        (Test-BackupPingUrl 'https://hc-ping.com.ejemplo.test/11111111-1111-1111-1111-111111111111') | Should Be $false
        (Test-BackupPingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111?secreto=x') | Should Be $false
    }

    It 'Borra en lotes de cien y conserva siempre el minimo y la copia nueva' {
        $start = [DateTimeOffset]::Parse('2024-01-01T00:00:00Z')
        $script:rows = @(0..207 | ForEach-Object { New-TestBackup ($start.AddDays($_).ToString('o')) })
        $script:rows += New-TestBackup '2026-10-30T12:00:00Z'
        Mock Invoke-RestMethod {
            param($Method, $Body)
            if ($Method -eq 'Delete') {
                $data = $Body | ConvertFrom-Json
                $script:requests += @($data.prefixes).Count
                @($data.prefixes | ForEach-Object { @{ name = $_ } })
            }
            else { $script:rows }
        }
        $plan = Invoke-RemoteBackupRetentionOriginal -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{} `
            -UploadedName 'enlyce-20261030-120000.dump' -UploadedSize 100 -Now ([DateTimeOffset]::Parse('2026-10-30T12:00:00Z'))
        ($script:requests -join ',') | Should Be '100,100,2'
        $plan.Keep.Count | Should Be 7
        $plan.Delete.Count | Should Be 202
        ($plan.Keep -contains 'enlyce-20261030-120000.dump') | Should Be $true
    }

    It 'Cancela sin borrar cuando el tamano remoto no coincide' {
        $script:new = New-TestBackup '2026-10-30T12:00:00Z'
        Mock Invoke-RestMethod { $script:new }
        { Invoke-RemoteBackupRetentionOriginal -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{} `
            -UploadedName $script:new.name -UploadedSize 200 } | Should Throw
        Assert-MockCalled Invoke-RestMethod -Scope It -Times 0 -ParameterFilter { $Method -eq 'Delete' }
    }

    It 'No declara exito cuando Supabase no confirma todos los borrados' {
        $script:rows = @(1..8 | ForEach-Object { New-TestBackup "2024-01-0$($_)T00:00:00Z" })
        $script:rows += New-TestBackup '2026-10-30T12:00:00Z'
        Mock Invoke-RestMethod {
            param($Method)
            if ($Method -eq 'Delete') { @() } else { $script:rows }
        }
        { Invoke-RemoteBackupRetentionOriginal -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -Headers @{} `
            -UploadedName 'enlyce-20261030-120000.dump' -UploadedSize 100 -Now ([DateTimeOffset]::Parse('2026-10-30T12:00:00Z')) } | Should Throw
        Assert-MockCalled Invoke-RestMethod -Scope It -Times 1 -ParameterFilter { $Method -eq 'Delete' }
    }

    It 'Usa Bearer para claves legacy sin cambiar el nombre del archivo' {
        $path = Join-Path $TestDrive 'enlyce-20261030-120000.dump'
        [IO.File]::WriteAllText($path, 'PGDMP')
        Mock Invoke-WebRequest {
            param($Uri, $Headers)
            $script:requests += @{ Uri = $Uri; Headers = $Headers }
        }
        Mock Invoke-RemoteBackupRetention { @{ Keep = @(); Delete = @() } }
        Publish-BackupArtifact -ProjectUrl 'https://storage.example.test' -Bucket 'enlyce-backups' -ApiKey 'legacy-test-only' -File (Get-Item $path) | Out-Null
        $script:requests[0].Headers.Authorization | Should Be 'Bearer legacy-test-only'
        $script:requests[0].Headers.apikey | Should Be 'legacy-test-only'
        $script:requests[0].Uri | Should Be 'https://storage.example.test/storage/v1/object/enlyce-backups/enlyce-20261030-120000.dump'
    }

    It 'Marca fallo si el respaldo termina pero su aviso final no se confirma' {
        $script:worked = $false
        Mock Send-BackupSignal { $Signal -ne 'success' }
        { Invoke-MonitoredBackup -PingUrl 'https://hc-ping.com/11111111-1111-1111-1111-111111111111' -Work { $script:worked = $true } } | Should Throw
        $script:worked | Should Be $true
        Assert-MockCalled Send-BackupSignal -Scope It -Times 1 -ParameterFilter { $Signal -eq 'fail' }
    }
}
