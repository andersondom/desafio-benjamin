$ErrorActionPreference = 'Stop'
$name = 'Desafio Benjamin - Monitorar Backups'
$scriptPath = Join-Path $PSScriptRoot 'Verificar-Backups.ps1'
if (Get-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue) { throw 'Tarefa ja existente; nenhuma alteracao realizada.' }
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ('-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $scriptPath + '"')
$trigger = New-ScheduledTaskTrigger -Daily -At '04:00'
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew -RestartCount 2 -RestartInterval (New-TimeSpan -Minutes 20) -ExecutionTimeLimit (New-TimeSpan -Minutes 30)
$principal = New-ScheduledTaskPrincipal -UserId ([Security.Principal.WindowsIdentity]::GetCurrent().Name) -LogonType Interactive -RunLevel Limited
Register-ScheduledTask -TaskName $name -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description 'Verifica backups locais e criptografados do Google Drive; registra falhas sem excluir arquivos.' | Out-Null
Write-Host 'Monitor agendado diariamente as 04:00.'
Get-ScheduledTaskInfo -TaskName $name | Select-Object NextRunTime
