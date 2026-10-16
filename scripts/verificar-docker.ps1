# Verificação não destrutiva do Desafio Benjamin no Docker
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

Write-Host '=== Estado dos contêineres ==='
docker compose ps
if ($LASTEXITCODE -ne 0) { throw 'Falha ao consultar contêineres.' }

Write-Host '=== Resposta HTTP local ==='
try {
    $response = Invoke-WebRequest -Uri 'http://localhost:8080/' -UseBasicParsing -TimeoutSec 20
    Write-Host "HTTP $($response.StatusCode)"
    if ($response.StatusCode -ne 200) { throw "HTTP inesperado: $($response.StatusCode)" }
} catch { Write-Host "Falha HTTP: $_"; throw }

Write-Host '=== OCR instalado ==='
docker compose exec -T web tesseract --list-langs
if ($LASTEXITCODE -ne 0) { throw 'Falha ao verificar OCR.' }

Write-Host '=== Banco SQLite presente ==='
docker compose exec -T web sh -c 'test -s /app/data/desafio-benjamin.db && echo "SQLite encontrado"'
if ($LASTEXITCODE -ne 0) { throw 'Banco SQLite não encontrado ou vazio.' }

Write-Host '=== Modelo Ollama (pode estar vazio) ==='
docker compose exec -T ollama ollama list
if ($LASTEXITCODE -ne 0) { throw 'Falha ao consultar Ollama.' }

Write-Host '=== Verificação inicial concluída ==='
Write-Host 'ATENÇÃO: este script não comprova persistência após reinício nem uma partida completa.'
