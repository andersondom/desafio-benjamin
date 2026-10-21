param([string]$BaseUrl = 'http://localhost:8080')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$controller = Join-Path $repo 'DesafioBenjamin\Controllers\JogoController.cs'
if (!(Test-Path -LiteralPath $controller)) { throw 'JogoController.cs nao encontrado.' }
$source = [IO.File]::ReadAllText($controller)
$checks = [ordered]@{
    'Inicio usa POST com antiforgery' = $source -match '(?s)\[HttpPost\]\s*\[ValidateAntiForgeryToken\]\s*public async Task<IActionResult> Iniciar'
    'Inicio rejeita questionario sem questoes' = $source.Contains('questoesSorteadas.Count == 0')
    'Inicio salva tentativa e questoes' = $source.Contains('_context.Tentativas.Add(tentativa)') -and $source.Contains('tentativa.Questoes.Add(')
    'Responder exige POST com antiforgery' = $source -match '(?s)\[HttpPost\]\s*\[ValidateAntiForgeryToken\]\s*public async Task<IActionResult> Responder'
    'Responder bloqueia partida concluida' = $source.Contains('tentativa.FinalizadaEm != null) return NotFound()')
    'Responder verifica alternativa da questao' = $source.Contains('a.Id == alternativaId && a.QuestaoId == tentativaQuestao.QuestaoId')
    'Responder impede pontuacao duplicada' = $source.Contains('if (!jaRespondida)')
    'Proxima exige resposta anterior' = $source.Contains('if (!respondeu)')
    'Proxima registra conclusao' = $source.Contains('tentativa.FinalizadaEm = DateTime.Now')
    'Resultado bloqueia tentativa incompleta' = $source.Contains('if (tentativa.FinalizadaEm == null)')
}
$failed = 0
foreach ($entry in $checks.GetEnumerator()) {
    if ($entry.Value) { Write-Host ('OK: ' + $entry.Key) }
    else { Write-Host ('FALHA: ' + $entry.Key); $failed++ }
}
try {
    $response = Invoke-WebRequest -Uri ($BaseUrl.TrimEnd('/') + '/') -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -eq 200) { Write-Host 'OK: pagina inicial HTTP 200' }
    else { Write-Host ('FALHA: HTTP ' + $response.StatusCode); $failed++ }
} catch {
    Write-Host ('FALHA: pagina inicial indisponivel - ' + $_.Exception.Message)
    $failed++
}
if ($failed) { throw "$failed verificacao(oes) falharam." }
Write-Host 'APROVADO: 11 verificacoes de contrato do fluxo e disponibilidade.'
Write-Host 'NOTA: verificacoes estaticas + HTTP GET; nao simulam partidas reais nem gravam dados.'
