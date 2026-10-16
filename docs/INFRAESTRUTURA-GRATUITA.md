# Infraestrutura gratuita — Desafio Benjamin 2.0

Esta branch parte de `chatgpt/gameplay` e **não altera a branch principal**.

## Pré-requisitos

- Docker Engine com plugin Compose ou Docker Desktop elegível para uso gratuito.
- Memória suficiente para a aplicação e o modelo local (32 GB RAM é uma boa base).
- Espaço em disco para imagens Docker, arquivos e modelos.
- Para acelerar Ollama por GPU, verificar o modelo de placa, VRAM e suporte do host.

## Iniciar a aplicação

Na raiz do repositório:

```bash
docker compose up --build -d
docker compose ps
docker compose logs --tail=100 web
```

Abrir `http://localhost:8080` no computador hospedeiro.

A porta 8080 é vinculada apenas a `127.0.0.1` por segurança. Não expor a aplicação diretamente à internet: a autenticação individual ainda será implementada.

## Modelo de IA local

Ollama roda em serviço separado, sem publicar sua porta no host. Baixar o modelo apenas após verificar GPU/VRAM:

```bash
docker compose exec ollama ollama pull qwen3:4b
docker compose exec ollama ollama list
```

A presença do modelo não significa que a aplicação já está integrada ao Ollama. Integração e revisão humana serão desenvolvidas em outra etapa. O contêiner Ollama começa em CPU por padrão; GPU depende de configuração específica do host.

## OCR

O contêiner `web` contém Tesseract e idiomas português e inglês. Verificar instalação:

```bash
docker compose exec web tesseract --list-langs
```

A interface de upload e o fluxo OCR ainda não foram implementados.

## Persistência

- `app-data`: SQLite em `/app/data/desafio-benjamin.db`
- `materials`: futuros arquivos em `/app/materials`
- `ollama-data`: modelos Ollama em `/root/.ollama`

`docker compose down` preserva volumes. **Não usar `docker compose down -v`**: isso remove os volumes e seus dados.

Antes de implantar, definir rotina de backup consistente para SQLite e arquivos. Não copiar o banco aberto sem mecanismo apropriado de backup.

## Verificações antes de integrar à main

1. Compilar e iniciar em Docker.
2. Conferir migrações e dados iniciais do SQLite.
3. Executar uma partida completa e conferir respostas e resultado.
4. Reiniciar os contêineres e conferir persistência.
5. Confirmar OCR `por` e `eng`.
6. Testar modelo local com recursos reais do host.
7. Corrigir questões de segurança e adicionar testes automatizados.

Nenhuma dessas verificações é considerada aprovada até ser executada no ambiente correspondente.

## Custos e limites

Não são utilizados serviços pagos. A operação ainda consome energia, espaço em disco e recursos do computador. A disponibilidade depende de ele permanecer ligado.
