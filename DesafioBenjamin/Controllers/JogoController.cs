using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Controllers;

public class JogoController : Controller
{
    private readonly AppDbContext _context;

    public JogoController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int id)
    {
        var questionario = await _context.Questionarios
            .Include(q => q.Disciplina)
            .Include(q => q.Questoes)
                .ThenInclude(q => q.Alternativas)
            .FirstOrDefaultAsync(q => q.Id == id && q.Ativo);

        if (questionario == null) return NotFound();

        var questoes = questionario.Questoes
            .OrderBy(_ => Guid.NewGuid())
            .Take(15)
            .ToList();

        ViewBag.Questionario = questionario;
        return View(questoes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iniciar(int questionarioId)
    {
        var aluno = await _context.Alunos
            .FirstOrDefaultAsync(a => a.Nome == "Benjamin" && a.Ativo);

        if (aluno == null) return NotFound("Aluno Benjamin não encontrado.");

        var questionario = await _context.Questionarios
            .Include(q => q.Questoes)
            .FirstOrDefaultAsync(q => q.Id == questionarioId && q.Ativo);

        if (questionario == null) return NotFound("Questionário não encontrado.");

        var questoesSorteadas = questionario.Questoes
            .OrderBy(_ => Guid.NewGuid())
            .Take(15)
            .ToList();

        if (questoesSorteadas.Count == 0)
            return BadRequest("O questionário não possui questões.");

        var tentativa = new Tentativa
        {
            AlunoId = aluno.Id,
            QuestionarioId = questionario.Id,
            IniciadaEm = DateTime.Now,
            TotalQuestoes = questoesSorteadas.Count
        };

        for (var i = 0; i < questoesSorteadas.Count; i++)
        {
            tentativa.Questoes.Add(new TentativaQuestao
            {
                QuestaoId = questoesSorteadas[i].Id,
                Ordem = i + 1
            });
        }

        _context.Tentativas.Add(tentativa);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Pergunta), new { tentativaId = tentativa.Id, ordem = 1 });
    }

    public async Task<IActionResult> Pergunta(int tentativaId, int ordem = 1)
    {
        var tentativa = await _context.Tentativas
            .Include(t => t.Questionario)
            .Include(t => t.Aluno)
            .FirstOrDefaultAsync(t => t.Id == tentativaId);

        if (tentativa == null) return NotFound();

        if (tentativa.FinalizadaEm != null)
            return RedirectToAction(nameof(Resultado), new { tentativaId });

        var tentativaQuestao = await _context.TentativaQuestoes
            .Include(tq => tq.Questao)
                .ThenInclude(q => q.Alternativas)
            .FirstOrDefaultAsync(tq => tq.TentativaId == tentativaId && tq.Ordem == ordem);

        if (tentativaQuestao == null)
            return RedirectToAction(nameof(Resultado), new { tentativaId });

        var respostaExistente = await _context.Respostas
            .Include(r => r.AlternativaSelecionada)
            .FirstOrDefaultAsync(r => r.TentativaId == tentativaId && r.QuestaoId == tentativaQuestao.QuestaoId);

        var questao = tentativaQuestao.Questao;
        questao.Alternativas = questao.Alternativas.OrderBy(a => a.Id).ToList();

        ViewBag.Tentativa = tentativa;
        ViewBag.Ordem = ordem;
        ViewBag.Resposta = respostaExistente;
        ViewBag.AlternativaCorretaId = questao.Alternativas.First(a => a.Correta).Id;

        return View(questao);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(int tentativaId, int ordem, int alternativaId)
    {
        var tentativa = await _context.Tentativas
            .FirstOrDefaultAsync(t => t.Id == tentativaId);

        if (tentativa == null || tentativa.FinalizadaEm != null) return NotFound();

        var tentativaQuestao = await _context.TentativaQuestoes
            .FirstOrDefaultAsync(tq => tq.TentativaId == tentativaId && tq.Ordem == ordem);

        if (tentativaQuestao == null) return NotFound();

        var jaRespondida = await _context.Respostas
            .AnyAsync(r => r.TentativaId == tentativaId && r.QuestaoId == tentativaQuestao.QuestaoId);

        if (!jaRespondida)
        {
            var alternativa = await _context.Alternativas
                .FirstOrDefaultAsync(a => a.Id == alternativaId && a.QuestaoId == tentativaQuestao.QuestaoId);

            if (alternativa == null) return BadRequest("Alternativa inválida.");

            _context.Respostas.Add(new Resposta
            {
                TentativaId = tentativaId,
                QuestaoId = tentativaQuestao.QuestaoId,
                AlternativaSelecionadaId = alternativa.Id,
                Acertou = alternativa.Correta,
                RespondidaEm = DateTime.Now
            });

            if (alternativa.Correta) tentativa.TotalAcertos++;
            else tentativa.TotalErros++;

            tentativa.PercentualAcertos =
                tentativa.TotalQuestoes == 0 ? 0 :
                Math.Round(tentativa.TotalAcertos * 100.0 / tentativa.TotalQuestoes, 1);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Pergunta), new { tentativaId, ordem });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Proxima(int tentativaId, int ordem)
    {
        var tentativa = await _context.Tentativas.FindAsync(tentativaId);
        if (tentativa == null) return NotFound();

        var respondeu = await _context.Respostas.AnyAsync(r =>
            r.TentativaId == tentativaId &&
            _context.TentativaQuestoes.Any(tq =>
                tq.TentativaId == tentativaId &&
                tq.Ordem == ordem &&
                tq.QuestaoId == r.QuestaoId));

        if (!respondeu)
            return RedirectToAction(nameof(Pergunta), new { tentativaId, ordem });

        if (ordem >= tentativa.TotalQuestoes)
        {
            tentativa.FinalizadaEm = DateTime.Now;
            tentativa.PercentualAcertos =
                tentativa.TotalQuestoes == 0 ? 0 :
                Math.Round(tentativa.TotalAcertos * 100.0 / tentativa.TotalQuestoes, 1);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Resultado), new { tentativaId });
        }

        return RedirectToAction(nameof(Pergunta), new { tentativaId, ordem = ordem + 1 });
    }

    public async Task<IActionResult> Resultado(int tentativaId)
    {
        var tentativa = await _context.Tentativas
            .Include(t => t.Aluno)
            .Include(t => t.Questionario)
                .ThenInclude(q => q.Disciplina)
            .FirstOrDefaultAsync(t => t.Id == tentativaId);

        if (tentativa == null) return NotFound();

        return View(tentativa);
    }
}
