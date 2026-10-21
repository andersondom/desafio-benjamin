# Backup e recuperação — Desafio Benjamin 2.0

Registro de validação: 19/10/2026. Homologação funcional 8/8 aprovada. Não realizar merge sem autorização.

## Evidências da cópia local

- Arquivo `backups/backup-benjamin-dados.tar.gz`: 17.353 bytes, contém `./desafio-benjamin.db` (118.784 bytes extraídos).
- Arquivo `backups/backup-benjamin-materiais.tar.gz`: 87 bytes, listagem `./` (volume vazio naquele momento).
- SHA-256 do arquivo de dados: `CC7E9DFD16DF18F45370C4739015E20DB736956646BC15C3D51F6939E247D83D`.
- SHA-256 do arquivo de materiais: `716FF45D3726EB11E1C766E4C6F331AED1380228DC73256DD70A1EA8EBC08F53`.
- `PRAGMA integrity_check` do banco **extraído da cópia**: `ok`.
- Serviços web e ollama em execução na última checagem; contêiner temporário de verificação removido.
- `PRAGMA table_list` foi executado com `r[0]`, mostrando nomes de esquemas (`main`/`temp`), **não** nomes das tabelas. A restauração funcional isolada foi homologada em 19/10/2026 (5/5 testes aprovados).

Os arquivos de backup estão apenas na máquina local do operador e **não são versionados neste repositório**. Não subir backups, banco ou segredos ao GitHub.

## Conferir os nomes das tabelas da cópia, se necessário

Na pasta `DesafioBenjamin` do PowerShell:

```powershell
docker create --name benjamin-verifica-backup python:3.12-alpine sleep 300
docker cp ..\backups\verificacao\desafio-benjamin.db benjamin-verifica-backup:/tmp/desafio-benjamin.db
docker start benjamin-verifica-backup
docker exec benjamin-verifica-backup python -c "import sqlite3; c=sqlite3.connect('/tmp/desafio-benjamin.db'); print([(r[0],r[1]) for r in c.execute('PRAGMA table_list')]); c.close()"
docker rm -f benjamin-verifica-backup
```

## Procedimento de recuperação — somente em incidente real, após confirmação

1. Preservar os arquivos de backup originais e seus hashes, copiar para mídia externa e identificar data/versão.
2. Confirmar que o projeto Compose é `desafio-benjamin-v2` e os volumes corretos são `desafio-benjamin-v2_app-data` e `desafio-benjamin-v2_materials`. **Nunca usar** `docker compose down -v` ou `docker volume prune`.
3. Parar **somente** o serviço web para impedir gravações, antes de qualquer intervenção no SQLite.
4. Fazer uma **segunda cópia de segurança do volume atual** antes de considerar substituição.
5. Validar o arquivo restaurável em volume ou contêiner de teste **isolado**, executando `PRAGMA integrity_check` e um teste funcional da aplicação apontada para esse volume de teste. Não reutilizar o volume de produção durante o ensaio.
6. Somente depois de aprovação explícita, executar restauração real, preservando permissões e eventual estrutura de arquivos SQLite (incluindo WAL/SHM quando existentes), iniciar o serviço e verificar os dados.
7. Documentar data, responsável, resultados e hashes.

**Não há autorização para restaurar ou substituir dados de produção.** Este documento é um plano de recuperação, não uma execução de restauração.

## Recomendações de proteção

- Manter ao menos uma cópia fora do computador (disco externo ou armazenamento seguro), com acesso restrito.
- Usar nomes com data e hora nos backups futuros para não sobrescrever cópias anteriores.
- Não incluir o arquivo `.env` em repositórios ou registros de execução.
- A verificação de integridade do SQLite não substitui ensaio de restauração e teste funcional.


## Homologação da restauração isolada — 19/10/2026

**Resultado: APROVADO — 5/5, sem observações.**

1. O arquivo `backup-benjamin-dados.tar.gz` foi extraído para o volume independente `benjamin-teste-restauracao`. O SQLite retornou `PRAGMA integrity_check: ok` usando `mode=ro&immutable=1`.
2. A estrutura foi identificada com `PRAGMA table_list`, incluindo `Alunos`, `Disciplinas`, `Questionarios`, `Questoes`, `Alternativas`, `Tentativas`, `TentativaQuestoes`, `Respostas`, além das tabelas de controle e internas.
3. O contêiner de teste `benjamin-teste-web` foi iniciado com a imagem `desafio-benjamin-v2-web`, banco restaurado em `/app/data`, e porta local `127.0.0.1:8081`. Logs mostraram consultas EF Core executadas e aplicação iniciada.
4. O operador confirmou: (i) página inicial acessível; (ii) menu correto; (iii) revisão inteligente acessível com informações; (iv) desafios e questionários anteriores disponíveis; (v) instância original funcionando em `127.0.0.1:8080`. Todos **funcionaram**.
5. O encerramento seguro do teste foi orientado com `docker stop benjamin-teste-web` e `docker rm benjamin-teste-web`; **a execução desses dois comandos ainda depende de confirmação do operador**. O volume `benjamin-teste-restauracao` deve permanecer preservado até decisão posterior.

Este ensaio confirma recuperação funcional **da cópia testada**. Não houve restauração nem substituição do banco original, merge, ou teste de recuperação de uma máquina completamente perdida.

### Comandos de encerramento seguro

```powershell
docker stop benjamin-teste-web
docker rm benjamin-teste-web
docker ps --filter "name=benjamin-teste-web"
docker compose -f ..\compose.yaml -p desafio-benjamin-v2 ps
```

Não remover volumes de produção nem executar `docker compose down -v`.
