using DesafioBenjamin.Data;
using DesafioBenjamin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Controllers;

[Authorize(Roles = "Administrador")]
[Route("painel-pedagogico")]
public sealed class PainelPedagogicoController : Controller
{
    private readonly AppDbContext _context;

    public PainelPedagogicoController(AppDbContext context) => _context = context;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // Consulta somente leitura: nenhuma entidade e atualizada.
        var partidas = await _context.Tentativas.AsNoTracking()
            .Where(t => t.Aluno.Nome == "Benjamin" && t.FinalizadaEm != null)
            .Select(t => new
            {
                t.Id, t.FinalizadaEm, t.TotalQuestoes, t.TotalAcertos,
                Disciplina = t.Questionario.Disciplina.Nome,
                Questionario = t.Questionario.Titulo,
                t.Questionario.Capitulo
            })
            .OrderByDescending(t => t.FinalizadaEm)
            .ToListAsync();

        var totalQuestoes = partidas.Sum(t => t.TotalQuestoes);
        var totalAcertos = partidas.Sum(t => t.TotalAcertos);

        var disciplinas = partidas
            .GroupBy(t => t.Disciplina)
            .Select(g =>
            {
                var questoes = g.Sum(t => t.TotalQuestoes);
                var acertos = g.Sum(t => t.TotalAcertos);
                return new DesempenhoDisciplina(g.Key, g.Count(), acertos, questoes,
                    Percentual(acertos, questoes));
            })
            .OrderBy(d => d.Nome)
            .ToList();

        var respostas = await _context.Respostas.AsNoTracking()
            .Where(r => r.Tentativa.Aluno.Nome == "Benjamin" && r.Tentativa.FinalizadaEm != null)
            .Select(r => new
            {
                r.QuestaoId, r.Acertou,
                Enunciado = r.Questao.Enunciado,
                Capitulo = r.Questao.Questionario.Capitulo,
                Disciplina = r.Questao.Questionario.Disciplina.Nome
            })
            .ToListAsync();

        var dificuldades = respostas
            .GroupBy(r => r.QuestaoId)
            .Select(g =>
            {
                var primeira = g.First();
                var erros = g.Count(r => !r.Acertou);
                return new DificuldadeResumo(primeira.Disciplina, primeira.Capitulo,
                    primeira.Enunciado, erros, g.Count(), Percentual(erros, g.Count()));
            })
            .Where(d => d.Erros > 0)
            .OrderByDescending(d => d.Erros)
            .ThenByDescending(d => d.PercentualErros)
            .Take(10)
            .ToList();

        var model = new PainelPedagogicoViewModel
        {
            TotalPartidas = partidas.Count,
            TotalQuestoes = totalQuestoes,
            TotalAcertos = totalAcertos,
            Aproveitamento = Percentual(totalAcertos, totalQuestoes),
            Disciplinas = disciplinas,
            Historico = partidas.Take(30).Select(t => new PartidaResumo(
                t.Id, t.FinalizadaEm!.Value, t.Disciplina, t.Questionario,
                t.Capitulo, t.TotalAcertos, t.TotalQuestoes,
                Percentual(t.TotalAcertos, t.TotalQuestoes))).ToList(),
            Dificuldades = dificuldades
        };

        Response.Headers.CacheControl = "no-store";
        return View(model);
    }

    private static double Percentual(int parte, int total) =>
        total <= 0 ? 0 : Math.Round(parte * 100.0 / total, 1);
}
