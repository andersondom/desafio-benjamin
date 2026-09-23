using DesafioBenjamin.Data;
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

        if (questionario == null)
            return NotFound();

        // Sorteia até 15 questões.
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

        if (aluno == null)
            return NotFound("Aluno Benjamin não encontrado.");

        var questionario = await _context.Questionarios
            .Include(q => q.Questoes)
            .FirstOrDefaultAsync(q =>
                q.Id == questionarioId &&
                q.Ativo);

        if (questionario == null)
            return NotFound("Questionário não encontrado.");

        var idsQuestoes = questionario.Questoes
            .OrderBy(_ => Guid.NewGuid())
            .Take(15)
            .Select(q => q.Id)
            .ToList();

        if (idsQuestoes.Count == 0)
            return BadRequest("O questionário não possui questões.");

        var tentativa = new Tentativa
        {
            AlunoId = aluno.Id,
            QuestionarioId = questionario.Id,
            IniciadaEm = DateTime.Now,
            TotalQuestoes = idsQuestoes.Count,
            TotalAcertos = 0,
            TotalErros = 0,
            PercentualAcertos = 0
        };

        _context.Tentativas.Add(tentativa);

        await _context.SaveChangesAsync();

        // Por enquanto vamos para a primeira pergunta.
        return RedirectToAction(
            nameof(Pergunta),
            new
            {
                tentativaId = tentativa.Id,
                questaoId = idsQuestoes.First()
            });
    }

    public async Task<IActionResult> Pergunta(int tentativaId, int questaoId)
    {
        var tentativa = await _context.Tentativas
            .Include(t => t.Questionario)
            .Include(t => t.Aluno)
            .FirstOrDefaultAsync(t => t.Id == tentativaId);

        if (tentativa == null)
            return NotFound();

        var questao = await _context.Questoes
            .Include(q => q.Alternativas)
            .FirstOrDefaultAsync(q => q.Id == questaoId);

        if (questao == null)
            return NotFound();

        // Embaralha as alternativas.
        questao.Alternativas = questao.Alternativas
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        ViewBag.Tentativa = tentativa;

        return View(questao);
    }

    
}