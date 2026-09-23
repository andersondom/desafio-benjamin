using System.Diagnostics;
using DesafioBenjamin.Data;
using DesafioBenjamin.Models;
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

        ViewBag.TotalDesafios = await _context.Tentativas.CountAsync(t => t.FinalizadaEm != null);
        ViewBag.TotalAcertos = await _context.Tentativas.SumAsync(t => (int?)t.TotalAcertos) ?? 0;
        var totalQuestoes = await _context.Tentativas
            .Where(t => t.FinalizadaEm != null)
            .SumAsync(t => (int?)t.TotalQuestoes) ?? 0;
        ViewBag.Aproveitamento = totalQuestoes == 0 ? 0 :
            Math.Round(ViewBag.TotalAcertos * 100.0 / totalQuestoes, 1);

        return View(questionarios);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
