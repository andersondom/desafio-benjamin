using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Controllers;

/// <summary>Revisao baseada em erros historicos, usando o fluxo de jogo existente.</summary>
[Route("revisao-inteligente")]
public sealed class RevisaoInteligenteController : Controller
{
    private readonly AppDbContext _context;
    public RevisaoInteligenteController(AppDbContext context) => _context = context;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var dados = await _context.Respostas.AsNoTracking()
            .Where(r => r.Tentativa.Aluno.Nome == "Benjamin"
                && r.Tentativa.FinalizadaEm != null
                && r.Questao.Ativa
                && r.Questao.Questionario.Ativo
                && r.Questao.Questionario.Disciplina.Ativa)
            .Select(r => new
            {
                r.QuestaoId, r.Acertou, r.Tentativa.FinalizadaEm,
                r.Questao.QuestionarioId,
                Questionario = r.Questao.Questionario.Titulo,
                Disciplina = r.Questao.Questionario.Disciplina.Nome,
                r.Questao.Questionario.Capitulo
            }).ToListAsync();

        var opcoes = dados.GroupBy(r => r.QuestionarioId)
            .Select(g => new RevisaoOpcao(
                g.Key, g.First().Questionario, g.First().Disciplina,
                g.First().Capitulo,
                g.GroupBy(r => r.QuestaoId).Count(q => q.Any(r => !r.Acertou))))
            .Where(o => o.QuestoesComErros > 0)
            .OrderBy(o => o.Disciplina).ThenBy(o => o.Questionario)
            .ToList();

        return View(opcoes);
    }

    [HttpPost("iniciar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iniciar(int questionarioId)
    {
        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Nome == "Benjamin" && a.Ativo);
        if (aluno is null) return NotFound("Aluno Benjamin não encontrado.");

        var valido = await _context.Questionarios.AnyAsync(q => q.Id == questionarioId
            && q.Ativo && q.Disciplina.Ativa);
        if (!valido) return NotFound("Questionário indisponível.");

        // Reutiliza apenas questoes com erros em partidas concluidas. Erros recorrentes
        // recebem prioridade, sem sobrescrever respostas ou tentativas anteriores.
        var historico = await _context.Respostas.AsNoTracking()
            .Where(r => r.Tentativa.AlunoId == aluno.Id
                && r.Tentativa.FinalizadaEm != null
                && r.Questao.QuestionarioId == questionarioId
                && r.Questao.Ativa)
            .Select(r => new { r.QuestaoId, r.Acertou, r.Tentativa.FinalizadaEm })
            .ToListAsync();

        var ids = historico.GroupBy(r => r.QuestaoId)
            .Select(g => new
            {
                Id = g.Key,
                Erros = g.Count(r => !r.Acertou),
                Total = g.Count(),
                UltimaResposta = g.OrderByDescending(r => r.FinalizadaEm).First()
            })
            .Where(x => x.Erros > 0)
            // Prioriza dificuldades persistentes e o ultimo erro; acerto recente reduz prioridade.
            .OrderByDescending(x => x.Erros * 1.0 / x.Total + (x.UltimaResposta.Acertou ? 0 : 1))
            .ThenByDescending(x => x.Erros)
            .ThenBy(x => x.Id)
            .Take(10)
            .Select(x => x.Id)
            .ToList();

        if (ids.Count == 0)
        {
            TempData["RevisaoAviso"] = "Ainda não há questões com erros disponíveis para revisão neste questionário.";
            return RedirectToAction(nameof(Index));
        }

        var tentativa = new Tentativa
        {
            AlunoId = aluno.Id,
            QuestionarioId = questionarioId,
            IniciadaEm = DateTime.Now,
            TotalQuestoes = ids.Count
        };
        for (var i = 0; i < ids.Count; i++)
            tentativa.Questoes.Add(new TentativaQuestao { QuestaoId = ids[i], Ordem = i + 1 });

        _context.Tentativas.Add(tentativa);
        await _context.SaveChangesAsync();

        return RedirectToAction("Pergunta", "Jogo", new { tentativaId = tentativa.Id, ordem = 1 });
    }

    public sealed record RevisaoOpcao(int QuestionarioId, string Questionario,
        string Disciplina, string? Capitulo, int QuestoesComErros);
}
