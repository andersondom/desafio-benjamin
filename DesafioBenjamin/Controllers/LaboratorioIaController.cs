using DesafioBenjamin.Services.IA;
using Microsoft.AspNetCore.Mvc;

namespace DesafioBenjamin.Controllers;

// Local development preview only. No database writes or publishing.
[Route("laboratorio-ia")]
public sealed class LaboratorioIaController : Controller
{
    private readonly OllamaQuestionService _ollama;
    private readonly IWebHostEnvironment _environment;

    public LaboratorioIaController(OllamaQuestionService ollama, IWebHostEnvironment environment)
    {
        _ollama = ollama;
        _environment = environment;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        if (!IsLocalRequest()) return NotFound();
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
            return View("Index");
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
        return View("Index");
    }

    private bool IsLocalRequest()
    {
        // The container is reachable only through Docker's localhost-bound port.
        // Do not deploy this unauthenticated laboratory to a public environment.
        return _environment.IsDevelopment() ||
            string.Equals(Environment.GetEnvironmentVariable("LABORATORIO_IA_LOCAL"), "true", StringComparison.OrdinalIgnoreCase);
    }
}
