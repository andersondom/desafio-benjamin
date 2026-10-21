param([int]$Port = 18081)
$ErrorActionPreference = 'Stop'
$containerName = 'benjamin-teste-isolado-' + [guid]::NewGuid().ToString('N').Substring(0,8)
$temp = Join-Path $env:TEMP ('benjamin-e2e-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp -Force | Out-Null
$base = "http://127.0.0.1:$Port"
$steps = 0
function Assert([bool]$condition, [string]$message) {
    if (!$condition) { throw "FALHA: $message" }
    $script:steps++
    Write-Host "OK: $message"
}
function Field([string]$html,[string]$name) {
    $match = [regex]::Match($html, '<input\b(?=[^>]*\bname="' + [regex]::Escape($name) + '")(?=[^>]*\bvalue="([^"]*)")[^>]*>', 'IgnoreCase')
    if (!$match.Success) { throw "Campo ausente: $name" }
    return [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}
function PostForm([string]$url,[hashtable]$data) {
    return Invoke-WebRequest -Uri ($base + $url) -Method Post -Body $data -WebSession $script:session -UseBasicParsing -MaximumRedirection 10 -TimeoutSec 30
}
try {
    Write-Host 'Iniciando aplicacao em container temporario com SQLite vazio e isolado...'
    $mount = 'type=bind,source=' + $temp + ',target=/app/data'
    $args = @('run','-d','--rm','--name',$containerName,'-p',"127.0.0.1:$($Port):8080",'--mount',$mount,
      '-e','ASPNETCORE_ENVIRONMENT=Production',
      '-e','ConnectionStrings__DefaultConnection=Data Source=/app/data/teste.db',
      '-e','ADMIN_USER=teste-isolado',
      '-e','ADMIN_PASSWORD=SenhaDeTesteSomenteAqui2026!',
      '--network','bridge','desafio-benjamin-v2-web:latest')
    $id = & docker @args
    if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel iniciar o container isolado.' }
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $home = $null
    for ($i=0; $i -lt 45; $i++) {
        try {
            $home = Invoke-WebRequest -Uri ($base + '/') -WebSession $session -UseBasicParsing -TimeoutSec 3
            if ($home.StatusCode -eq 200) { break }
        } catch { Start-Sleep -Seconds 2 }
    }
    Assert ($null -ne $home -and $home.StatusCode -eq 200) 'Aplicacao isolada iniciou e respondeu HTTP 200'
    $link = [regex]::Match($home.Content, 'href="([^"]*/Jogo/Index/\d+[^"]*)"', 'IgnoreCase')
    if (!$link.Success) {
        $link = [regex]::Match($home.Content, 'href="([^"]*/Jogo(?:/Index)?\?id=\d+[^"]*)"', 'IgnoreCase')
    }
    if (!$link.Success) { throw 'Nao foi possivel localizar o link de inicio de desafio na pagina inicial.' }
    $gameUrl = [Net.WebUtility]::HtmlDecode($link.Groups[1].Value)
    $game = Invoke-WebRequest -Uri ([uri]::new([uri]$base,$gameUrl)) -WebSession $session -UseBasicParsing
    Assert ($game.StatusCode -eq 200) 'Pagina do desafio acessivel'
    $questionnaireId = Field $game.Content 'questionarioId'
    $token = Field $game.Content '__RequestVerificationToken'
    $start = PostForm '/Jogo/Iniciar' @{questionarioId=$questionnaireId;__RequestVerificationToken=$token}
    $idMatch = [regex]::Match($start.BaseResponse.ResponseUri.AbsoluteUri, 'tentativaId=(\d+)', 'IgnoreCase')
    if (!$idMatch.Success) { throw 'Nao foi possivel localizar a tentativa iniciada.' }
    $attemptId = $idMatch.Groups[1].Value
    Assert ($start.Content -match 'Pergunta') 'Tentativa iniciada e primeira pergunta exibida'
    for ($order=1; $order -le 15; $order++) {
        $page = Invoke-WebRequest -Uri ($base + "/Jogo/Pergunta?tentativaId=$attemptId&ordem=$order") -WebSession $session -UseBasicParsing
        $answer = [regex]::Match($page.Content, '<button\b(?=[^>]*\bname="alternativaId")(?=[^>]*\bvalue="(\d+)")[^>]*>', 'IgnoreCase')
        if (!$answer.Success) { throw "Alternativa nao localizada na pergunta $order" }
        $token = Field $page.Content '__RequestVerificationToken'
        $answered = PostForm '/Jogo/Responder' @{tentativaId=$attemptId;ordem=$order;alternativaId=$answer.Groups[1].Value;__RequestVerificationToken=$token}
        Assert ($answered.Content -match 'Sua resposta:') "Resposta $order registrada com feedback"
        $token = Field $answered.Content '__RequestVerificationToken'
        $next = PostForm '/Jogo/Proxima' @{tentativaId=$attemptId;ordem=$order;__RequestVerificationToken=$token}
        if ($order -lt 15) {
            Assert ($next.Content -match ('Pergunta\s+' + ($order+1) + '\s+de')) "Avanco para pergunta $($order+1)"
        } else {
            Assert ($next.Content -match 'Desafio conclu.do!') 'Partida finalizada e resultado exibido'
        }
    }
    Write-Host "APROVADO: $steps verificacoes HTTP em partida completa isolada."
} catch {
    Write-Host ('ERRO: ' + $_.Exception.Message)
    throw
} finally {
    & docker rm -f $containerName 2>$null | Out-Null
    Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Ambiente temporario removido; dados reais preservados.'
}
