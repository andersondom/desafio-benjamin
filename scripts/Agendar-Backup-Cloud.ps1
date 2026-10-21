param([switch]$RunNow)
$ErrorActionPreference = 'Stop'
$name = 'Desafio Benjamin - Backup Google Drive'
$scriptPath = Join-Path $PSScriptRoot 'Backup-Cloud-Criptografado.ps1'
if (!(Test-Path -LiteralPath $scriptPath)) { throw 'Script de backup externo nao encontrado.' }
$identity = [Security.Principal.WindowsIdentity]::GetCurrent().Name
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ('-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $scriptPath + '"')
$trigger = New-ScheduledTaskTrigger -Daily -At '03:30'
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Hours 2) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 15)
$principal = New-ScheduledTaskPrincipal -UserId $identity -LogonType Interactive -RunLevel Limited
$existing = Get-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue
if ($existing) {
    throw "A tarefa '$name' ja existe. Nao sera sobrescrita automaticamente."
}
Register-ScheduledTask -TaskName $name -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description 'Copia criptografada dos backups validados do Desafio Benjamin para Google Drive; sem exclusoes.' | Out-Null
Write-Host 'AGENDAMENTO CRIADO: backup externo diario as 03:30.'
if ($RunNow) {
    Start-ScheduledTask -TaskName $name
    Write-Host 'EXECUCAO SOLICITADA. Confira o resultado apos alguns segundos.'
}
Get-ScheduledTaskInfo -TaskName $name | Select-Object LastRunTime,LastTaskResult,NextRunTime
