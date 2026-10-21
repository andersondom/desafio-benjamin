$ErrorActionPreference = 'Stop'
$name = 'Desafio Benjamin - Monitorar Backups'
$task = Get-ScheduledTask -TaskName $name -ErrorAction Stop
$scriptPath = Join-Path $PSScriptRoot 'Verificar-Backups.ps1'
if (!(Test-Path -LiteralPath $scriptPath)) { throw 'Monitor nao encontrado.' }
$argument = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $scriptPath + '" -NotifyOnFailure'
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument $argument
Set-ScheduledTask -TaskName $name -Action $action | Out-Null
Write-Host 'ALERTA ATIVADO: tarefa existente preservada; notificacao somente em falhas.'
Get-ScheduledTask -TaskName $name | Select-Object TaskName,State
Get-ScheduledTaskInfo -TaskName $name | Select-Object NextRunTime
