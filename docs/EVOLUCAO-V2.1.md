# Desafio Benjamin 2.1 — Lote integrado de evolução

**Branch:** `feat/infra-gratuita-v2` (sem merge).  
**Status:** código das três frentes implementado; **aguarda validação integrada** antes de publicar.  
**Referência funcional anterior:** `docs/HOMOLOGACAO-FINAL-V2.md`.

## Entregas deste lote

1. **Aprendizagem adaptativa:** a revisão inteligente prioriza questões com maior proporção de erros e questões cuja resposta mais recente foi incorreta. Continua usando apenas respostas de partidas concluídas e questões ativas; não reescreve o histórico nem utiliza inferências sensíveis.
2. **Gamificação:** duas missões semanais na página inicial — concluir três partidas e somar vinte acertos. A semana começa na segunda-feira. A conquista é calculada a partir do histórico, sem novos dados persistidos.
3. **Painel pedagógico:** exibe progresso das duas missões e evolução por disciplina, comparando janelas consecutivas de até cinco partidas concluídas da mesma disciplina. Sem pelo menos duas partidas na disciplina, indica dados insuficientes.

## Validação integrada única — executar ao final

Na pasta `DesafioBenjamin`:

```powershell
git pull --ff-only origin feat/infra-gratuita-v2
if ($LASTEXITCODE -ne 0) { throw 'Falha ao atualizar codigo.' }
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Validar-E-Atualizar.ps1
if ($LASTEXITCODE -ne 0) { throw 'Lote 2.1 reprovado. Nao publicar.' }
```

A suíte agora inclui **22 verificações de modelos/recompensas/metas**, **13 de contrato HTTP/estático** e **47 de partida completa isolada**, total esperado **82 verificações**. Os números são expectativas, não resultados, até a execução.

A suíte roda sem `-UpdateWeb`: não publica, não altera os volumes reais e descarta o ambiente isolado.

**Somente depois da aprovação**, executar prontidão de backups/volumes e publicar apenas o web:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Verificar-Prontidao-Atualizacao.ps1
if ($LASTEXITCODE -ne 0) { throw 'Prontidao reprovada.' }
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Validar-E-Atualizar.ps1 -UpdateWeb
if ($LASTEXITCODE -ne 0) { throw 'Publicacao bloqueada.' }
```

**Conferência visual pós-publicação:** página inicial mostra “Missões da semana”; painel administrativo mostra “Metas da semana” e “Evolução por disciplina”; revisão inteligente mantém início e conclusão normais.

## Cuidados

- Nunca remover `desafio-benjamin-v2_app-data`, `desafio-benjamin-v2_materials` ou `desafio-benjamin-v2_ollama-data`.
- Não efetuar merge sem aprovação explícita.
- Nenhuma migração de banco ou serviço pago é necessária neste lote.
- Não declarar a versão 2.1 homologada ou publicada antes de confirmar os testes e a atualização local.
