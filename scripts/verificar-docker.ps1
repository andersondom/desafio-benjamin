# Non-destructive verification of Desafio Benjamin Docker services.
# ASCII-only messages avoid Windows PowerShell 5.1 encoding issues.
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

Write-Host '=== Docker containers ==='
docker compose ps
if ($LASTEXITCODE -ne 0) { throw 'Failed to list containers.' }

Write-Host '=== Local HTTP response ==='
$response = Invoke-WebRequest -Uri 'http://localhost:8080/' -UseBasicParsing -TimeoutSec 20
Write-Host "HTTP $($response.StatusCode)"
if ($response.StatusCode -ne 200) { throw "Unexpected HTTP status: $($response.StatusCode)" }

Write-Host '=== OCR languages ==='
docker compose exec -T web tesseract --list-langs
if ($LASTEXITCODE -ne 0) { throw 'OCR verification failed.' }

Write-Host '=== SQLite database ==='
docker compose exec -T web sh -c 'test -s /app/data/desafio-benjamin.db && echo "SQLite database found"'
if ($LASTEXITCODE -ne 0) { throw 'SQLite database missing or empty.' }

Write-Host '=== Ollama models (may be empty) ==='
docker compose exec -T ollama ollama list
if ($LASTEXITCODE -ne 0) { throw 'Ollama verification failed.' }

Write-Host '=== Initial verification complete ==='
Write-Host 'This does not verify persistence after restart or complete gameplay.'
