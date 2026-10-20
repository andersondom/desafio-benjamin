param(
    [string]$BackupRoot = '',
    [string]$CloudFolder = 'G:\Meu Drive\Desafio Benjamin 2.0\Backups Criptografados',
    [int]$MaxAgeHours = 30
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if (!$BackupRoot) { $BackupRoot = Join-Path $repo 'backups\automaticos' }
$staging = Join-Path $repo 'backups\nuvem-criptografados'
$logDir = Join-Path $repo 'backups\monitoramento'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$log = Join-Path $logDir ('verificacao-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.txt')
$issues = New-Object System.Collections.Generic.List[string]
function Check([bool]$condition,[string]$message) {
    if (!$condition) { $script:issues.Add($message) }
}
try {
    Check (Test-Path -LiteralPath $BackupRoot) 'Pasta de backups locais indisponivel'
    $valid = @()
    if (Test-Path -LiteralPath $BackupRoot) {
        $valid = @(Get-ChildItem -LiteralPath $BackupRoot -Directory | Where-Object {
            $_.Name -match '^\d{8}-\d{6}$' -and
            (Test-Path (Join-Path $_.FullName 'resultado.txt')) -and
            (Select-String -LiteralPath (Join-Path $_.FullName 'resultado.txt') -Pattern 'SUCESSO:' -Quiet)
        } | Sort-Object Name -Descending)
    }
    Check ($valid.Count -gt 0) 'Nenhum backup local bem-sucedido encontrado'
    if ($valid.Count -gt 0) {
        $latest = $valid[0]
        $when = [datetime]::ParseExact($latest.Name,'yyyyMMdd-HHmmss',[Globalization.CultureInfo]::InvariantCulture)
        Check (((Get-Date) - $when).TotalHours -le $MaxAgeHours) "Backup local desatualizado: $($latest.Name)"
        foreach ($file in @('desafio-benjamin.db','materiais.tar.gz','SHA256.txt','resultado.txt')) {
            $p = Join-Path $latest.FullName $file
            Check ((Test-Path -LiteralPath $p) -and ((Get-Item -LiteralPath $p -ErrorAction SilentlyContinue).Length -gt 0)) "Arquivo local ausente/vazio: $file"
        }
        $manifest = Join-Path $latest.FullName 'SHA256.txt'
        if (Test-Path -LiteralPath $manifest) {
            foreach ($entry in (Get-Content -LiteralPath $manifest)) {
                if ($entry -match '^([A-Fa-f0-9]{64})\s+(.+)$') {
                    $p = Join-Path $latest.FullName $Matches[2].Trim()
                    if (Test-Path -LiteralPath $p) {
                        Check ((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash -eq $Matches[1]) "Hash local divergente: $p"
                    } else { $issues.Add("Arquivo do manifesto ausente: $p") }
                } else { $issues.Add('Linha invalida no manifesto SHA256') }
            }
        }
        $archive = "benjamin-$($latest.Name).7z"
        $localArchive = Join-Path $staging $archive
        $remoteArchive = Join-Path $CloudFolder $archive
        Check (Test-Path -LiteralPath $localArchive) "Arquivo criptografado local ausente: $archive"
        Check (Test-Path -LiteralPath $remoteArchive) "Arquivo criptografado no Drive ausente: $archive"
        if ((Test-Path -LiteralPath $localArchive) -and (Test-Path -LiteralPath $remoteArchive)) {
            Check ((Get-FileHash -LiteralPath $localArchive -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $remoteArchive -Algorithm SHA256).Hash) "Hash Drive divergente: $archive"
        }
    }
    Check (Test-Path -LiteralPath $CloudFolder) 'Pasta Google Drive indisponivel'
} catch { $issues.Add("Erro na verificacao: $($_.Exception.Message)") }
$state = if ($issues.Count -eq 0) { 'OK' } else { 'FALHA' }
$lines = @("$(Get-Date -Format o) STATUS=$state")
if ($issues.Count) { $lines += $issues.ToArray() }
$lines | Set-Content -LiteralPath $log -Encoding UTF8
$lines | ForEach-Object { Write-Host $_ }
if ($issues.Count) { exit 1 }
exit 0
