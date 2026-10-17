# Local Ollama diagnostic. No downloads or database changes.
$ErrorActionPreference = 'Stop'
$uri = 'http://127.0.0.1:11434/api/chat'
$payload = @{
    model = 'qwen3:4b'
    messages = @(
        @{ role = 'system'; content = 'You are a concise assistant. Output the final answer only.' },
        @{ role = 'user'; content = 'Reply with exactly OK. /no_think' }
    )
    stream = $false
    think = $false
    keep_alive = '30s'
    options = @{ num_predict = 48; num_ctx = 1024; temperature = 0 }
} | ConvertTo-Json -Depth 8
Write-Host 'Testing Ollama API (maximum 45 seconds)...'
try {
    $result = Invoke-RestMethod -Uri $uri -Method Post -ContentType 'application/json' -Body $payload -TimeoutSec 45
    $answer = [string]$result.message.content
    Write-Host ('Answer: ' + $answer)
    Write-Host ('Seconds: ' + [math]::Round($result.total_duration / 1e9, 2))
    Write-Host ('Generated tokens: ' + $result.eval_count)
    if ($answer.Trim() -ceq 'OK') {
        Write-Host 'RESULT: PASS'
    } else {
        Write-Host 'RESULT: MODEL_OUTPUT_NOT_COMPLIANT'
        Write-Host 'The model is operational, but its response does not follow the instruction.'
    }
} catch {
    Write-Host ('RESULT: ERROR_OR_TIMEOUT - ' + $_.Exception.Message)
    exit 1
}
