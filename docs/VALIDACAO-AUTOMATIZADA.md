# Validacao automatizada — Desafio Benjamin 2.0

## Evidencia confirmada

GitHub Actions, execucao 195 (run ID 39600132693), commit `afd4117`: **success**.
Etapas confirmadas: restore, build Release, 18 testes de modelos/historico/recompensas e verificacao da presenca dos scripts.

## Cobertura e limites

- **GitHub Actions (remoto):** compila o projeto e executa os 18 testes de console a cada push na branch `feat/infra-gratuita-v2` e em pull requests.
- **Windows/Docker (local):** `scripts/Validar-E-Atualizar.ps1` executa 18 testes de console, 11 verificacoes estaticas/HTTP e 37 verificacoes de partida completa em SQLite descartavel.
- O GitHub Actions **nao executa** os 48 testes locais que dependem de Docker e do aplicativo ativo.
- Os testes locais foram aprovados pelo operador (66 verificacoes ao todo); o fluxo remoto foi confirmado no run acima.
- Os testes nao provam seguranca total, ausencia de erros ou integridade de todos os cenarios possiveis.

## Execucao local segura

No PowerShell, a partir de `DesafioBenjamin`:

```powershell
git pull origin feat/infra-gratuita-v2
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Validar-E-Atualizar.ps1
```

Sem `-UpdateWeb`, nao atualiza o aplicativo.

## Regras de operacao

- Nao realizar merge sem aprovacao expressa.
- Nao modificar ou remover volumes, banco real e backups.
- Atualizacao local requer decisao explicita de executar `-UpdateWeb`.
- Em caso de falha, interromper e investigar antes de atualizar.
