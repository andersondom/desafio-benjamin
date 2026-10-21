# Desafio Benjamin 2.1

Aplicação educacional ASP.NET Core 10 com SQLite e geração local de questões pelo Ollama.

## Acesso e navegação

- Início e desafios: http://localhost:8080/
- Revisão inteligente: http://localhost:8080/revisao-inteligente
- Laboratório de IA (administrador): http://localhost:8080/laboratorio-ia
- Questões aprovadas (administrador): http://localhost:8080/laboratorio-ia/aprovadas
- Painel pedagógico (administrador): http://localhost:8080/painel-pedagogico

Todas as áreas estão disponíveis no menu principal. A escolha de uma área administrativa protegida redireciona para login e, após autenticação, retorna ao destino solicitado.

## Execução local (Windows / PowerShell)

Na pasta raiz do repositório (onde está `compose.yaml`):

```powershell
git pull --ff-only origin main
docker compose -p desafio-benjamin-v2 config --quiet
docker compose -p desafio-benjamin-v2 up -d --build --no-deps web
docker compose -p desafio-benjamin-v2 ps
```

Crie e mantenha um arquivo `.env` **local e fora do Git** com `ADMIN_USER` e `ADMIN_PASSWORD` (senha de pelo menos 12 caracteres). Não envie credenciais ao repositório. Se o serviço web não existir ainda, inicie a composição completa com `docker compose -p desafio-benjamin-v2 up -d --build`.

O modelo Ollama configurado é `qwen3:4b`. Não é necessário baixar novamente se o volume existente já o contém.

## Preservação dos dados

Os volumes persistentes do projeto Compose `desafio-benjamin-v2` são:

- `desafio-benjamin-v2_app-data`: banco SQLite e histórico;
- `desafio-benjamin-v2_materials`: materiais;
- `desafio-benjamin-v2_ollama-data`: modelos Ollama.

**Nunca execute** `docker compose down -v`, `docker volume prune` ou exclusão manual desses volumes para atualizar o sistema. O comando de atualização acima não remove os volumes.

### Backup antes de mudanças estruturais

Interrompa a escrita do aplicativo antes de copiar o SQLite (ou use a API de backup do SQLite). Exemplo de backup do volume com o aplicativo parado:

```powershell
docker compose -p desafio-benjamin-v2 stop web
docker run --rm -v desafio-benjamin-v2_app-data:/dados:ro -v "${PWD}:/backup" alpine sh -c "cd /dados && tar czf /backup/backup-benjamin-app-data.tar.gz ."
docker compose -p desafio-benjamin-v2 start web
```

Guarde o arquivo de backup em local seguro. Faça backup separado dos materiais quando necessário. Teste a recuperação em ambiente isolado, nunca diretamente sobre o banco ativo.

## Critérios de estabilidade

- Menu acessível sem login; proteção preservada em todas as rotas administrativas.
- Login com retorno ao destino escolhido; logout invalida a sessão.
- Desafios, revisão, pontuação, medalhas e animações permanecem funcionais.
- Histórico de respostas e partidas preservado.
- Sem migrations ou limpeza de volumes nesta etapa.
- Para alterações futuras, trabalhar em uma branch e validar antes de integrar à `main`.

## Estado de entrega

**Versão 2.1 homologada funcionalmente em 21/10/2026 e publicada no Docker local.** Foram aprovadas 82 verificações automatizadas (22 de modelos/recompensas/metas, 13 de contrato/disponibilidade e 47 de partida isolada) e as três frentes de homologação funcional: aprendizagem adaptativa, gamificação semanal e painel pedagógico. Sem observações ou pendências funcionais relatadas.

Documentação de aceite e manutenção:

- [Encerramento da versão 2.1](docs/EVOLUCAO-V2.1.md)
- [Histórico de homologação da versão 2.0](docs/HOMOLOGACAO-FINAL-V2.md)

Este aceite cobre o ambiente local validado pelo responsável, não uma auditoria independente de segurança ou publicação na internet. A branch `main` é a referência para novas intervenções após a integração aprovada pelo responsável.
