using DesafioBenjamin.Services.IA;
using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DesafioBenjamin.Controllers;

// Laboratorio de uso local. Exige autenticacao antes de qualquer implantacao publica.
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

    [HttpGet("aprovadas")]
    public async Task<IActionResult> Aprovadas(int? questionarioId)
    {
        if (!IsLocalRequest()) return NotFound();
        var questionarios = await _db.Questionarios.AsNoTracking().OrderBy(q => q.Titulo).ToListAsync();
        ViewBag.Questionarios = questionarios;
        ViewBag.QuestionarioId = questionarioId;
        var query = _db.Questoes.AsNoTracking()
            .Include(q => q.Alternativas)
            .Include(q => q.Questionario)
            .AsQueryable();
        if (questionarioId.HasValue)
            query = query.Where(q => q.QuestionarioId == questionarioId.Value);
        var questoes = await query.OrderByDescending(q => q.Id).Take(100).ToListAsync();
        return View(questoes);
    }


    private static string Normalizar(string texto) =>
        string.Join(" ", texto.Trim().ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [HttpGet("editar/{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        if (!IsLocalRequest()) return NotFound();
        var questao = await _db.Questoes.AsNoTracking().Include(q => q.Alternativas)
            .Include(q => q.Questionario).FirstOrDefaultAsync(q => q.Id == id);
        if (questao is null) return NotFound();
        ViewBag.Utilizada = await _db.TentativaQuestoes.AnyAsync(t => t.QuestaoId == id);
        return View(questao);
    }

    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarEdicao(int id, string enunciado, string[] alternativas, int indiceCorreto, string explicacao)
    {
        if (!IsLocalRequest()) return NotFound();
        var questao = await _db.Questoes.Include(q => q.Alternativas).FirstOrDefaultAsync(q => q.Id == id);
        if (questao is null) return NotFound();
        if (await _db.TentativaQuestoes.AnyAsync(t => t.QuestaoId == id))
        {
            TempData["Erro"] = "Questao utilizada em partida: edicao bloqueada para preservar o historico. Desative-a e cadastre uma versao corrigida.";
            return RedirectToAction(nameof(Editar), new { id });
        }
        if (string.IsNullOrWhiteSpace(enunciado) || !enunciado.TrimEnd().EndsWith('?') ||
            alternativas is not { Length: 4 } || alternativas.Any(string.IsNullOrWhiteSpace) ||
            alternativas.Select(Normalizar).Distinct().Count() != 4 ||
            indiceCorreto is < 0 or > 3 || string.IsNullOrWhiteSpace(explicacao) ||
            questao.Alternativas.Count != 4)
        {
            TempData["Erro"] = "Preencha uma pergunta, quatro alternativas distintas e a explicacao.";
            return RedirectToAction(nameof(Editar), new { id });
        }
        var enunciadosExistentes = await _db.Questoes.AsNoTracking()
            .Where(q => q.Id != id && q.QuestionarioId == questao.QuestionarioId)
            .Select(q => q.Enunciado).ToListAsync();
        var duplicada = enunciadosExistentes.Any(e => Normalizar(e) == Normalizar(enunciado));
        if (duplicada)
        {
            TempData["Erro"] = "Ja existe uma questao com este enunciado no questionario.";
            return RedirectToAction(nameof(Editar), new { id });
        }
        questao.Enunciado = enunciado.Trim();
        questao.Explicacao = explicacao.Trim();
        var existentes = questao.Alternativas.OrderBy(a => a.Id).ToArray();
        for (var i = 0; i < 4; i++)
        {
            existentes[i].Texto = alternativas[i].Trim();
            existentes[i].Correta = i == indiceCorreto;
        }
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Questao atualizada com sucesso.";
        return RedirectToAction(nameof(Aprovadas), new { questionarioId = questao.QuestionarioId });
    }

    [HttpPost("alternar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(int id)
    {
        if (!IsLocalRequest()) return NotFound();
        var questao = await _db.Questoes.FindAsync(id);
        if (questao is null) return NotFound();
        questao.Ativa = !questao.Ativa;
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = questao.Ativa ? "Questao reativada." : "Questao desativada para novas partidas; historico preservado.";
        return RedirectToAction(nameof(Aprovadas), new { questionarioId = questao.QuestionarioId });
    }

    [HttpPost("aprovar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int questionarioId, string enunciado, string[] alternativas, int indiceCorreto, string explicacao, bool revisado)
    {
        if (!IsLocalRequest()) return NotFound();
        // Preserve os campos editados em qualquer falha de validacao.
        ViewBag.QuestaoGerada = new QuestaoGerada
        {
            Enunciado = enunciado ?? "",
            Alternativas = alternativas ?? Array.Empty<string>(),
            IndiceCorreto = indiceCorreto,
            Explicacao = explicacao ?? ""
        };
        ViewBag.QuestionarioSelecionado = questionarioId;
        if (!revisado || string.IsNullOrWhiteSpace(enunciado) || !enunciado.TrimEnd().EndsWith('?') ||
            alternativas is not { Length: 4 } || alternativas.Any(string.IsNullOrWhiteSpace) ||
            alternativas.Select(Normalizar).Distinct().Count() != 4 ||
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
        var enunciadosExistentes = await _db.Questoes.AsNoTracking()
            .Where(q => q.QuestionarioId == questionario.Id)
            .Select(q => q.Enunciado).ToListAsync();
        var jaExiste = enunciadosExistentes.Any(e => Normalizar(e) == Normalizar(enunciado));
        if (jaExiste)
        {
            ViewBag.Erro = "Ja existe uma questao com este enunciado no questionario. Nenhuma duplicata foi salva.";
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
