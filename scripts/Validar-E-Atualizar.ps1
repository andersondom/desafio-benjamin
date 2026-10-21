param([switch]$UpdateWeb)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repo 'tests\RecompensasSmoke\RecompensasSmoke.csproj'
if (!(Test-Path -LiteralPath $project)) { throw 'Projeto de testes nao encontrado.' }
$docker = Get-Command docker -ErrorAction Stop
Push-Location $repo
try {
    Write-Host 'ETAPA 1/5: verificando Docker...'
    & docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Docker indisponivel.' }
    Write-Host 'ETAPA 2/5: executando testes isolados...'
    $mount = $repo + ':/projeto'
    & docker run --rm --network none -v $mount -w /projeto mcr.microsoft.com/dotnet/sdk:10.0 dotnet run --project tests/RecompensasSmoke/RecompensasSmoke.csproj --ignore-failed-sources
    if ($LASTEXITCODE -ne 0) { throw 'Testes falharam. Atualizacao bloqueada.' }
    Write-Host 'ETAPA 3/5: verificando contrato e disponibilidade...'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Testar-Fluxo-Jogo.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Verificacao de fluxo falhou. Atualizacao bloqueada.' }
    Write-Host 'ETAPA 4/5: executando partida completa em banco isolado...'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Testar-Partida-Isolada.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Partida isolada falhou. Atualizacao bloqueada.' }
    Write-Host 'TODOS OS TESTES APROVADOS.'
    if (!$UpdateWeb) {
        Write-Host 'MODO SEGURO: nenhuma atualizacao realizada. Para atualizar, use -UpdateWeb.'
        exit 0
    }
    Write-Host 'ETAPA 5/5: atualizando apenas o servico web, preservando volumes...'
    & docker compose -p desafio-benjamin-v2 -f (Join-Path $repo 'compose.yaml') up -d --build --no-deps web
    if ($LASTEXITCODE -ne 0) { throw 'Atualizacao do web falhou; conferir docker compose logs.' }
    Write-Host 'ATUALIZACAO CONCLUIDA. Verifique http://localhost:8080/'
} finally { Pop-Location }
