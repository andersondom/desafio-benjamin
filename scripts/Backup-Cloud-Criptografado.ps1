param(
    [string]$DriveFolder = 'G:\Meu Drive\Desafio Benjamin 2.0\Backups Criptografados',
    [string]$BackupRoot = '',
    [switch]$InitializePassword,
    [switch]$TestOnly
)
$ErrorActionPreference = 'Stop'
$sevenZip = 'C:\Program Files\7-Zip\7z.exe'
if (!(Test-Path -LiteralPath $sevenZip)) { throw '7-Zip nao encontrado.' }
$repo = Split-Path $PSScriptRoot -Parent
if (!$BackupRoot) { $BackupRoot = Join-Path $repo 'backups\automaticos' }
$credentialDir = Join-Path $env:LOCALAPPDATA 'DesafioBenjamin\BackupCloud'
$credentialFile = Join-Path $credentialDir 'segredo.dpapi'
$staging = Join-Path $repo 'backups\nuvem-criptografados'
New-Item -ItemType Directory -Force -Path $credentialDir,$staging | Out-Null
if ($InitializePassword) {
    if (Test-Path -LiteralPath $credentialFile) { throw 'Senha ja configurada. Nao sobrescreva sem recuperar e validar backups anteriores.' }
    $first = Read-Host 'Crie uma senha forte para os backups (nao sera exibida)' -AsSecureString
    $second = Read-Host 'Repita a mesma senha' -AsSecureString
    $b1 = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($first)
    $b2 = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($second)
    try {
        $p1 = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b1)
        $p2 = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b2)
        if ($p1 -cne $p2 -or $p1.Length -lt 16) { throw 'As senhas diferem ou possuem menos de 16 caracteres.' }
        $first | ConvertFrom-SecureString | Set-Content -LiteralPath $credentialFile -Encoding ASCII
        Write-Host 'Senha protegida com DPAPI para o usuario Windows atual. Guarde a senha original fora deste computador.'
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b1)
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b2)
        $p1 = $null; $p2 = $null
    }
    exit 0
}
if (!(Test-Path -LiteralPath $credentialFile)) { throw 'Configure a senha primeiro usando -InitializePassword.' }
if (!(Test-Path -LiteralPath $BackupRoot)) { throw 'Pasta local de backups nao encontrada.' }
if (!(Test-Path -LiteralPath 'G:\Meu Drive')) { throw 'Google Drive G:\Meu Drive indisponivel. Nenhum arquivo sera removido.' }
# Set-Content adds a newline; ConvertTo-SecureString requires only the DPAPI hex string.
# Read one line rather than -Raw, which includes CR/LF and causes FormatException.
$protectedText = Get-Content -LiteralPath $credentialFile -TotalCount 1 -ErrorAction Stop
if ([string]::IsNullOrWhiteSpace($protectedText)) { throw 'Arquivo DPAPI vazio. Nao envie seu conteudo; solicite procedimento seguro de recuperacao.' }
try {
    $secure = ConvertTo-SecureString -String $protectedText.Trim() -ErrorAction Stop
} catch {
    throw 'Nao foi possivel abrir a credencial DPAPI. Confirme o mesmo usuario Windows; nao exclua a credencial nem backups existentes.'
}
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    $folders = @(Get-ChildItem -LiteralPath $BackupRoot -Directory | Where-Object { $_.Name -match '^\d{8}-\d{6}$' } | Sort-Object Name)
    $processed = 0
    foreach ($folder in $folders) {
        $report = Join-Path $folder.FullName 'resultado.txt'
        $expected = @('desafio-benjamin.db','materiais.tar.gz','SHA256.txt','resultado.txt')
        if (!(Test-Path -LiteralPath $report) -or !(Select-String -LiteralPath $report -Pattern 'SUCESSO:' -Quiet)) { continue }
        if (@($expected | Where-Object { !(Test-Path -LiteralPath (Join-Path $folder.FullName $_)) }).Count -gt 0) { continue }
        $name = "benjamin-$($folder.Name).7z"
        $localArchive = Join-Path $staging $name
        $remoteArchive = Join-Path $DriveFolder $name
        if (Test-Path -LiteralPath $remoteArchive) {
            if (!(Test-Path -LiteralPath $localArchive)) { throw "Arquivo remoto ja existe sem copia local verificavel: $name" }
            if ((Get-FileHash -LiteralPath $remoteArchive -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $localArchive -Algorithm SHA256).Hash) { throw "Divergencia no arquivo remoto: $name" }
            continue
        }
        if (!(Test-Path -LiteralPath $localArchive)) {
            $tmp = "$localArchive.partial"
            if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Force }
            Push-Location $BackupRoot
            try {
                # 7z CLI requires the password in the process command line temporarily.
                & $sevenZip a -t7z -m0=lzma2 -mhe=on "-p$password" $tmp $folder.Name | Out-Null
                if ($LASTEXITCODE -ne 0) { throw "Falha ao criptografar $name" }
            } finally { Pop-Location }
            & $sevenZip t "-p$password" $tmp | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Falha no teste de descriptografia $name" }
            Move-Item -LiteralPath $tmp -Destination $localArchive
        } else {
            & $sevenZip t "-p$password" $localArchive | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Arquivo local invalido: $name" }
        }
        if (!$TestOnly) {
            New-Item -ItemType Directory -Force -Path $DriveFolder | Out-Null
            $remoteTmp = "$remoteArchive.partial"
            if (Test-Path -LiteralPath $remoteTmp) { throw "Arquivo parcial remoto ja existe: $remoteTmp. Verifique manualmente." }
            Copy-Item -LiteralPath $localArchive -Destination $remoteTmp -ErrorAction Stop
            if ((Get-FileHash -LiteralPath $localArchive -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $remoteTmp -Algorithm SHA256).Hash) { throw "Falha na verificacao SHA256: $name" }
            Move-Item -LiteralPath $remoteTmp -Destination $remoteArchive
            Write-Host "COPIADO E VERIFICADO LOCALMENTE: $remoteArchive"
        } else {
            Write-Host "TESTE DE CRIPTOGRAFIA APROVADO: $localArchive"
        }
        $processed++
    }
    Write-Host "CONCLUIDO: $processed novo(s) arquivo(s). Sincronizacao na nuvem deve ser conferida separadamente."
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    $password = $null
}
