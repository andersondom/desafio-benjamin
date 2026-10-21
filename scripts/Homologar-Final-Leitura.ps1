param([string]$BaseUrl = 'http://127.0.0.1:8080')
$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')
$passed = 0
function Check([bool]$ok, [string]$label) {
    if (-not $ok) { throw "FALHA: $label" }
    $script:passed++
    Write-Host "OK: $label"
}
function GetPage([string]$path) {
    return Invoke-WebRequest -Uri ($base + $path) -UseBasicParsing -MaximumRedirection 10 -TimeoutSec 20
}
Write-Host 'Homologacao final: somente leitura, sem alterar dados nem publicar.'
$home = GetPage '/'
Check ($home.StatusCode -eq 200) 'Pagina inicial responde HTTP 200'
Check ($home.Content -match 'Desafio Benjamin') 'Pagina inicial identifica o aplicativo'
$review = GetPage '/revisao-inteligente'
Check ($review.StatusCode -eq 200) 'Revisao inteligente acessivel'
$login = GetPage '/administracao/entrar'
Check ($login.StatusCode -eq 200) 'Login administrativo acessivel'
foreach ($path in @('/painel-pedagogico','/laboratorio-ia','/laboratorio-ia/aprovadas')) {
    $page = GetPage $path
    $finalPath = $page.BaseResponse.ResponseUri.AbsolutePath
    Check ($finalPath -eq '/administracao/entrar') ("Acesso anonimo bloqueado em " + $path)
}
Write-Host ("APROVADO: " + $passed + " verificacoes HTTP somente leitura.")
Write-Host 'NOTA: estes testes nao substituem o fluxo autenticado nem a geracao real por IA.'
