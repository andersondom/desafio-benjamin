param([switch]$UpdateWeb)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repo 'tests\RecompensasSmoke\RecompensasSmoke.csproj'
if (!(Test-Path -LiteralPath $project)) { throw 'Projeto de testes nao encontrado.' }
$docker = Get-Command docker -ErrorAction Stop
Push-Location $repo
try {
    Write-Host 'ETAPA 1/3: verificando Docker...'
    & docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Docker indisponivel.' }
    Write-Host 'ETAPA 2/3: executando testes isolados...'
    $mount = $repo + ':/projeto'
    & docker run --rm --network none -v $mount -w /projeto mcr.microsoft.com/dotnet/sdk:10.0 dotnet run --project tests/RecompensasSmoke/RecompensasSmoke.csproj --ignore-failed-sources
    if ($LASTEXITCODE -ne 0) { throw 'Testes falharam. Atualizacao bloqueada.' }
    Write-Host 'TESTES APROVADOS.'
    if (!$UpdateWeb) {
        Write-Host 'MODO SEGURO: nenhuma atualizacao realizada. Para atualizar, use -UpdateWeb.'
        exit 0
    }
    Write-Host 'ETAPA 3/3: atualizando apenas o servico web, preservando volumes...'
    & docker compose -p desafio-benjamin-v2 -f (Join-Path $repo 'compose.yaml') up -d --build --no-deps web
    if ($LASTEXITCODE -ne 0) { throw 'Atualizacao do web falhou; conferir docker compose logs.' }
    Write-Host 'ATUALIZACAO CONCLUIDA. Verifique http://localhost:8080/'
} finally { Pop-Location }
