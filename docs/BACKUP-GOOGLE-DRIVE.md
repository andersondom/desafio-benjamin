# Backup criptografado para Google Drive (Windows)

Destino confirmado: `G:\Meu Drive\Desafio Benjamin 2.0\Backups Criptografados` (conta selecionada pelo operador). O script `scripts/Backup-Cloud-Criptografado.ps1` não altera o backup local das 03h nem os volumes Docker.

## Pré-requisitos e riscos

- 7-Zip instalado em `C:\Program Files\7-Zip\7z.exe`.
- Google Drive para computador ativo na sessão Windows que executará a tarefa, unidade `G:\Meu Drive` acessível.
- Arquivos `.7z` usam AES-256 e cabeçalhos criptografados (`-mhe=on`). A senha é protegida localmente via DPAPI (`ConvertFrom-SecureString`) e só pode ser recuperada no mesmo contexto de usuário/Windows. **Guardar a senha original em local externo e seguro** para recuperação após perda do computador.
- **Limitação do 7-Zip CLI:** durante a execução, a senha é passada como argumento ao processo e pode ficar visível para software com privilégios adequados. Evitar executar em máquina compartilhada ou comprometida.
- A verificação SHA-256 da cópia em `G:\` comprova igualdade local, **não** que o Google Drive já sincronizou. Conferir no navegador a presença e disponibilidade do arquivo.
- Os arquivos locais e remotos não são excluídos pelo script. Retenção de backups na nuvem precisa ser planejada separadamente, após validar recuperação e capacidade de armazenamento.

## Primeira configuração

No PowerShell, na pasta `DesafioBenjamin`:

```powershell
git pull origin feat/infra-gratuita-v2
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Backup-Cloud-Criptografado.ps1 -InitializePassword
```

Digitar a senha duas vezes sem mostrá-la no chat. Mínimo de 16 caracteres; usar senha longa, única e guardá-la fora do computador. A senha **não** deve ser gravada no repositório ou nos argumentos do Agendador.

## Homologação, sem copiar para a nuvem

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Backup-Cloud-Criptografado.ps1 -TestOnly
Get-ChildItem ..\backups\nuvem-criptografados -Filter *.7z | Select-Object Name,Length
```

O teste criptografa localmente e valida a abertura de cada arquivo. **Não ativa agendamento e não envia dados para a nuvem.** Validar restauração em diretório isolado antes de ativar a automação externa.

## Envio após homologação

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Backup-Cloud-Criptografado.ps1
```

O script envia somente backups locais que possuem `SUCESSO:` e os quatro arquivos exigidos. Arquivos já presentes no Drive não são sobrescritos nem apagados. Em caso de divergência, a execução falha para inspeção manual. **Atenção:** o arquivo `.partial` no Drive não é removido automaticamente se houver interrupção; investigar antes de tentar novamente.

Após conferir arquivo no Google Drive pelo navegador e realizar um teste de recuperação, agendar tarefa separada após o backup local, com usuário Windows interativo e Google Drive ativo. Não agendar antes dessa validação.


## Homologação da automação externa — 20/10/2026

- Google Drive para computador: unidade `G:\Meu Drive`, com dois arquivos `.7z` copiados, SHA-256 local conferido e presença na nuvem confirmada pelo operador via navegador.
- Extração com senha de recuperação: arquivos `desafio-benjamin.db`, `materiais.tar.gz`, `resultado.txt` e `SHA256.txt` presentes no diretório isolado. Não houve nova execução do `PRAGMA integrity_check` sobre o banco extraído nessa etapa.
- Script `scripts/Agendar-Backup-Cloud.ps1` criou a tarefa `Desafio Benjamin - Backup Google Drive` para 03:30 diariamente, com usuário Windows interativo.
- Execução manual agendada em 20/10/2026 às 08:54:38: resultado inicial `267009` (em execução), resultado final `0` (sucesso). Próxima execução indicada: 21/10/2026 às 03:30:00.
- O resultado da tarefa não equivale à confirmação da sincronização de backups futuros: conferir periodicamente a pasta no navegador e testar recuperação.
- Backups locais, volumes Docker, modelo Ollama e arquivos já existentes na nuvem não foram excluídos pela rotina.
