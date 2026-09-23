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
}