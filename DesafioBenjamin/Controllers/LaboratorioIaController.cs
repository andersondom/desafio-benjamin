using DesafioBenjamin.Services.IA;
using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DesafioBenjamin.Controllers;

// Local development preview only. No database writes or publishing.
[Route("laboratorio-ia")]
public sealed class LaboratorioIaController : Controller
{
    private readonly OllamaQuestionService _ollama;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _db;

    public LaboratorioIaController(OllamaQuestionService ollama, IWebHostEnvironment environment, AppDbContext db)
    {
        _ollama = ollama;
        _environment = environment;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!IsLocalRequest()) return NotFound();
        ViewBag.Questionarios = await _db.Questionarios.AsNoTracking().OrderBy(q => q.Titulo).ToListAsync();
        return View();
    }

    [HttpPost("gerar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gerar(string textoDidatico, CancellationToken cancellationToken)
    {
        if (!IsLocalRequest()) return NotFound();
        if (string.IsNullOrWhiteSpace(textoDidatico) || textoDidatico.Length > 12000)
        {
            ViewBag.Erro = "Informe um texto didatico de ate 12.000 caracteres.";
            return await RevisaoAsync();
        }

        ViewBag.TextoDidatico = textoDidatico;
        try
        {
            ViewBag.QuestaoGerada = await _ollama.GerarRascunhoAsync(textoDidatico, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            ViewBag.Erro = "Nao foi possivel gerar uma questao valida. O material nao foi salvo. Detalhe: " + ex.Message;
        }
        return await RevisaoAsync();
    }

    private async Task<IActionResult> RevisaoAsync()
    {
        ViewBag.Questionarios = await _db.Questionarios.AsNoTracking().OrderBy(q => q.Titulo).ToListAsync();
        return View("Index");
    }

    [HttpPost("aprovar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int questionarioId, string enunciado, string[] alternativas, int indiceCorreto, string explicacao, bool revisado)
    {
        if (!IsLocalRequest()) return NotFound();
        if (!revisado || string.IsNullOrWhiteSpace(enunciado) || !enunciado.TrimEnd().EndsWith('?') ||
            alternativas is not { Length: 4 } || alternativas.Any(string.IsNullOrWhiteSpace) ||
            alternativas.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4 ||
            indiceCorreto is < 0 or > 3 || string.IsNullOrWhiteSpace(explicacao))
        {
            ViewBag.Erro = "Revise os campos, confirme a conferencia e escolha uma unica resposta correta.";
            return await RevisaoAsync();
        }
        var questionario = await _db.Questionarios.FirstOrDefaultAsync(q => q.Id == questionarioId && q.Ativo);
        if (questionario is null)
        {
            ViewBag.Erro = "Questionario nao encontrado ou inativo.";
            return await RevisaoAsync();
        }
        var questao = new Questao
        {
            QuestionarioId = questionario.Id,
            Enunciado = enunciado.Trim(),
            Explicacao = explicacao.Trim(),
            Ordem = await _db.Questoes.CountAsync(q => q.QuestionarioId == questionario.Id) + 1,
            Alternativas = alternativas.Select((texto, i) => new Alternativa { Texto = texto.Trim(), Correta = i == indiceCorreto }).ToList()
        };
        _db.Questoes.Add(questao);
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Questao revisada adicionada ao questionario. Ela ja pode aparecer em novos jogos.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsLocalRequest()
    {
        // The container is reachable only through Docker's localhost-bound port.
        // Do not deploy this unauthenticated laboratory to a public environment.
        return _environment.IsDevelopment() ||
            string.Equals(Environment.GetEnvironmentVariable("LABORATORIO_IA_LOCAL"), "true", StringComparison.OrdinalIgnoreCase);
    }
}
