param(
    [string]$ProjectName = 'desafio-benjamin-v2',
    [int]$RetentionDays = 30,
    [string]$BackupDirectory = '',
    [switch]$SkipRetention
)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
    $BackupDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'backups\automaticos'
}
New-Item -ItemType Directory -Force -Path $BackupDirectory | Out-Null
$BackupDirectory = (Resolve-Path $BackupDirectory).Path
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $BackupDirectory $stamp
$lock = Join-Path $BackupDirectory 'backup.lock'
$lockStream = $null
try {
    # Exclusive lock; fail safely when a previous backup is running.
    $lockStream = [System.IO.File]::Open($lock, [System.IO.FileMode]::OpenOrCreate, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
    New-Item -ItemType Directory -Path $runDir | Out-Null
    $log = Join-Path $runDir 'resultado.txt'
    function Log([string]$message) {
        $line = "$(Get-Date -Format o) $message"
        Add-Content -LiteralPath $log -Value $line -Encoding UTF8
        Write-Host $line
    }
    function DockerRun([string[]]$argsList) {
        & docker @argsList
        if ($LASTEXITCODE -ne 0) { throw "Docker retornou codigo $LASTEXITCODE." }
    }
    Log 'INICIO'
    DockerRun @('info','--format','{{.ServerVersion}}')
    $appVolume = "${ProjectName}_app-data"
    $materialsVolume = "${ProjectName}_materials"
    foreach ($v in @($appVolume,$materialsVolume)) {
        DockerRun @('volume','inspect',$v,'--format','{{.Name}}')
    }
    # SQLite online backup API: consistent snapshot while the app keeps running.
    # Source is mounted read-write to permit SQLite WAL/shared-memory coordination.
    # No data-changing SQL is issued. Output is written to the backup directory.
    $python = 'import sqlite3,os; s=sqlite3.connect("file:/source/desafio-benjamin.db?mode=ro",uri=True,timeout=30); d=sqlite3.connect("/output/desafio-benjamin.db"); s.backup(d,pages=100,sleep=0.1); print("integrity_check:",d.execute("PRAGMA integrity_check").fetchone()[0]); d.close(); s.close()'
    DockerRun @('run','--rm','--mount',"type=volume,source=$appVolume,target=/source",'--mount',"type=bind,source=$runDir,target=/output",'python:3.12-alpine','python','-c',$python)
    $dbFile = Join-Path $runDir 'desafio-benjamin.db'
    if (!(Test-Path -LiteralPath $dbFile) -or (Get-Item $dbFile).Length -le 0) { throw 'Copia SQLite ausente ou vazia.' }
    $verify = 'import sqlite3; c=sqlite3.connect("file:/output/desafio-benjamin.db?mode=ro&immutable=1",uri=True); v=c.execute("PRAGMA integrity_check").fetchone()[0]; print(v); c.close(); assert v=="ok"'
    DockerRun @('run','--rm','--mount',"type=bind,source=$runDir,target=/output,readonly",'python:3.12-alpine','python','-c',$verify)
    # Tar materials into the same host directory. Does not touch source volume.
    DockerRun @('run','--rm','--mount',"type=volume,source=$materialsVolume,target=/materials,readonly",'--mount',"type=bind,source=$runDir,target=/output",'alpine','sh','-c','cd /materials && tar -czf /output/materiais.tar.gz . && tar -tzf /output/materiais.tar.gz >/dev/null')
    $hashes = @('desafio-benjamin.db','materiais.tar.gz') | ForEach-Object { Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $runDir $_) }
    $hashes | ForEach-Object { "$($_.Hash)  $([System.IO.Path]::GetFileName($_.Path))" } | Set-Content -LiteralPath (Join-Path $runDir 'SHA256.txt') -Encoding ASCII
    Log 'SUCESSO: banco verificado, materiais arquivados, hashes gerados.'
    # Never remove previous backups until the current run has succeeded.
    if (!$SkipRetention -and $RetentionDays -gt 0) {
        $cutoff = (Get-Date).AddDays(-$RetentionDays)
        Get-ChildItem -LiteralPath $BackupDirectory -Directory |
          Where-Object { $_.Name -match '^\d{8}-\d{6}$' -and $_.Name -ne $stamp -and $_.CreationTime -lt $cutoff -and (Test-Path (Join-Path $_.FullName 'SHA256.txt')) -and (Select-String -LiteralPath (Join-Path $_.FullName 'resultado.txt') -Pattern 'SUCESSO:' -Quiet -ErrorAction SilentlyContinue) } |
          ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force; Log "RETENCAO: removido $($_.Name)" }
    }
} catch {
    if (Test-Path variable:log) { Log "FALHA: $($_.Exception.Message)" }
    Write-Error $_
    exit 1
} finally {
    if ($null -ne $lockStream) { $lockStream.Dispose() }
}
