# Backup automático (Windows)

Script: `scripts/Backup-Automatico.ps1`.

- Cria uma cópia **online** consistente do SQLite por meio da API `sqlite3.Connection.backup` (não interrompe o serviço web).
- Verifica `PRAGMA integrity_check` da cópia.
- Arquiva o volume de materiais em `materiais.tar.gz` e gera SHA-256.
- Armazena cada execução em `backups/automaticos/AAAAMMDD-HHMMSS/` com `resultado.txt`.
- Remove somente pastas de backups automáticos anteriores à retenção, **depois** de um novo backup bem-sucedido; nunca remove volumes Docker, backups manuais ou modelos Ollama.
- Requer Docker Desktop em execução e as imagens `python:3.12-alpine` e `alpine`. Se o Docker estiver desligado, a tarefa falhará com registro de erro; configurar repetição na tarefa do Windows.
- O backup de materiais é um instantâneo de arquivos, não uma transação; caso existam gravações simultâneas, programar horário de baixa atividade.
- A automação local não substitui cópia externa dos backups.

## Primeiro teste manual (obrigatório)

No PowerShell, na pasta `DesafioBenjamin`:

```powershell
git pull origin feat/infra-gratuita-v2
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Backup-Automatico.ps1 -SkipRetention
Get-ChildItem ..\backups\automaticos -Directory | Sort-Object Name -Descending | Select-Object -First 2
```

Confirmar saída `SUCESSO`, existência de `desafio-benjamin.db`, `materiais.tar.gz`, `SHA256.txt` e `resultado.txt`. Se falhar, **não agendar**; enviar o erro para análise.

## Agendamento após teste manual aprovado

Usar o Agendador de Tarefas do Windows:
1. Criar tarefa `Desafio Benjamin - Backup Diario` para executar diariamente às 03:00.
2. Programa: `powershell.exe`. Argumentos: `-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "C:\PROJETOS\GIT\desafio-benjamin\desafio-benjamin\scripts\Backup-Automatico.ps1"`.
3. Em **Configurações**, habilitar **Executar a tarefa o mais cedo possível após uma inicialização agendada ser perdida**.
4. Em **Condições**, avaliar **Ativar o computador para executar esta tarefa** conforme preferência; Docker Desktop deve estar ativo.
5. Em **Configurações**, impedir execução simultânea de instâncias. Considerar repetir após falhas quando Docker não estiver disponível.
6. Executar manualmente a tarefa uma vez e conferir `resultado.txt`.

Não ativar a tarefa antes de verificar o caminho real do script e testar com sucesso. Nunca colocar credenciais administrativas ou `.env` no agendamento.
