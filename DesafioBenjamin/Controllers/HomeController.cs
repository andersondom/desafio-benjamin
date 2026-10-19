using System.Diagnostics;
using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
using DesafioBenjamin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var questionarios = await _context.Questionarios
            .Include(q => q.Disciplina)
            .Where(q => q.Ativo && q.Disciplina.Ativa)
            .OrderBy(q => q.Disciplina.Nome)
            .ToListAsync();

        var historico = await _context.Tentativas.AsNoTracking()
            .Where(t => t.FinalizadaEm != null && t.Aluno.Nome == "Benjamin")
            .ToListAsync();
        ViewBag.TotalDesafios = historico.Count;
        ViewBag.Estrelas = historico.Sum(RecompensasService.Estrelas);
        ViewBag.Conquistas = RecompensasService.Conquistas(historico);
        ViewBag.TotalAcertos = historico.Sum(t => t.TotalAcertos);
        var totalQuestoes = historico.Sum(t => t.TotalQuestoes);
        ViewBag.Aproveitamento = totalQuestoes == 0 ? 0 :
            Math.Round(ViewBag.TotalAcertos * 100.0 / totalQuestoes, 1);

        return View(questionarios);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
