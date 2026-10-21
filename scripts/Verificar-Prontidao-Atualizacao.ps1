param([switch]$RunTests)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$failed = $false
function Step([string]$name,[scriptblock]$action) {
    Write-Host ("VERIFICANDO: " + $name)
    try {
        & $action
        Write-Host ("OK: " + $name)
    } catch {
        $script:failed = $true
        Write-Host ("FALHA: " + $name + " - " + $_.Exception.Message)
    }
}
Step 'Git branch e alteracoes locais' {
    Push-Location $repo
    try {
        $branch = (& git branch --show-current).Trim()
        if ($LASTEXITCODE -ne 0 -or $branch -ne 'feat/infra-gratuita-v2') { throw "Branch inesperada: $branch" }
        $dirty = & git status --porcelain --untracked-files=no
        if ($LASTEXITCODE -ne 0 -or $dirty) { throw 'Arquivos versionados alterados localmente.' }
        & git fetch --quiet origin feat/infra-gratuita-v2
        if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel consultar a branch remota.' }
        $local = (& git rev-parse HEAD).Trim()
        $remote = (& git rev-parse origin/feat/infra-gratuita-v2).Trim()
        if ($local -ne $remote) { throw 'Branch local difere da remota. Sincronize antes de prosseguir.' }
    } finally { Pop-Location }
}
Step 'Docker e volumes existentes' {
    & docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Docker indisponivel.' }
    foreach ($v in @('desafio-benjamin-v2_app-data','desafio-benjamin-v2_materials','desafio-benjamin-v2_ollama-data')) {
        & docker volume inspect $v --format '{{.Name}}' | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Volume ausente: $v" }
    }
}
Step 'Aplicativo principal acessivel' {
    $r = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/' -UseBasicParsing -TimeoutSec 15
    if ($r.StatusCode -ne 200) { throw "HTTP $($r.StatusCode)" }
}
Step 'Backups locais e copia criptografada no Drive' {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Verificar-Backups.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Monitor de backup retornou falha.' }
}
if ($RunTests) {
    Step 'Testes completos em modo seguro' {
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Validar-E-Atualizar.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Suite de testes falhou.' }
    }
}
if ($failed) { Write-Host 'RESULTADO: NAO APTO. Nenhuma atualizacao realizada.'; exit 1 }
Write-Host 'RESULTADO: VERIFICACOES LOCAIS APROVADAS. Nenhuma atualizacao realizada.'
exit 0
