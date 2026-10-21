# Desafio Benjamin 2.1 — Lote integrado de evolução

**Branch:** `feat/infra-gratuita-v2` (sem merge).  
**Status:** **HOMOLOGAÇÃO FUNCIONAL APROVADA E ENCERRADA EM 21/10/2026**, após publicação local e aceite do responsável.  
**Referência funcional anterior:** `docs/HOMOLOGACAO-FINAL-V2.md`.

## Entregas deste lote

1. **Aprendizagem adaptativa:** a revisão inteligente prioriza questões com maior proporção de erros e questões cuja resposta mais recente foi incorreta. Continua usando apenas respostas de partidas concluídas e questões ativas; não reescreve o histórico nem utiliza inferências sensíveis.
2. **Gamificação:** duas missões semanais na página inicial — concluir três partidas e somar vinte acertos. A semana começa na segunda-feira. A conquista é calculada a partir do histórico, sem novos dados persistidos.
3. **Painel pedagógico:** exibe progresso das duas missões e evolução por disciplina, comparando janelas consecutivas de até cinco partidas concluídas da mesma disciplina. Sem pelo menos duas partidas na disciplina, indica dados insuficientes.

## Procedimento de validação e atualização — referência para manutenções futuras

Na pasta `DesafioBenjamin`:

```powershell
git pull --ff-only origin feat/infra-gratuita-v2
if ($LASTEXITCODE -ne 0) { throw 'Falha ao atualizar codigo.' }
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ..\scripts\Validar-E-Atualizar.ps1
if ($LASTEXITCODE -ne 0) { throw 'Lote 2.1 reprovado. Nao publicar.' }
```

A suíte inclui **22 verificações de modelos/recompensas/metas**, **13 de contrato HTTP/estático** e **47 de partida completa isolada**. **As 82 verificações foram aprovadas**, conforme execução e resultado comunicados pelo responsável.

A suíte roda sem `-UpdateWeb`: não publica, não altera os volumes reais e descarta o ambiente isolado.

**Para atualizações futuras**, após aprovação dos testes, executar prontidão de backups/volumes e publicar apenas o web:

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
- A versão 2.1 foi homologada e publicada localmente, conforme evidências comunicadas pelo responsável; não implica publicação pública nem merge.

## Encerramento formal da versão 2.1 — 21/10/2026

**Decisão:** APROVADA, PUBLICADA LOCALMENTE E ENCERRADA FUNCIONALMENTE.

### Evidências comunicadas pelo responsável

| Verificação | Resultado |
| --- | --- |
| Modelos, histórico, recompensas e metas semanais | 22/22 aprovadas |
| Contratos e disponibilidade | 13/13 aprovadas |
| Partida completa isolada em banco temporário | 47/47 aprovadas |
| **Total da suíte integrada** | **82/82 aprovadas** |
| Construção da imagem Docker do aplicativo principal | Concluída |
| Inicialização do contêiner `desafio-benjamin-v2-web-1` | Concluída |
| Aprendizagem adaptativa após publicação | Aprovada pelo responsável |
| Gamificação semanal após publicação | Aprovada pelo responsável |
| Painel pedagógico após publicação | Aprovado pelo responsável |
| Observações da homologação funcional | Nenhuma |

**Fluxo executado:** testes em modo seguro, verificação de prontidão, atualização do serviço web com `-UpdateWeb` e conferência funcional das três frentes. O responsável informou “ATUALIZACAO CONCLUIDA. Verifique http://localhost:8080/” e, em seguida, aprovou as três funcionalidades sem observações.

**Preservação:** a suíte informou remoção do ambiente temporário e preservação dos dados reais. A publicação atualizou o serviço web, sem indicação de exclusão dos volumes persistentes. O histórico da versão 2.0 permanece documentado em `docs/HOMOLOGACAO-FINAL-V2.md`.

**Escopo do aceite:** homologação funcional do ambiente local, com evidências reportadas pelo responsável. Não constitui auditoria independente, teste de carga, certificação de segurança, assinatura digital ou implantação pública.

**Situação do GitHub:** registro efetuado na branch `feat/infra-gratuita-v2`; nenhum merge autorizado ou executado como parte deste encerramento.

**Pendências funcionais informadas para encerrar o ciclo 2.1:** nenhuma.
