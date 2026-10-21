using DesafioBenjamin.Data;
using DesafioBenjamin.Services;
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

        // Compara janelas de mesmo tamanho, sem confundir variacao com diagnostico.
        // Exige ao menos duas partidas concluidas para uma comparacao justa.
        var janela = Math.Min(5, partidas.Count / 2);
        double? evolucaoRecente = null;
        if (janela > 0)
        {
            var recentes = partidas.Take(janela).ToList();
            var anteriores = partidas.Skip(janela).Take(janela).ToList();
            var totalRecente = recentes.Sum(p => p.TotalQuestoes);
            var totalAnterior = anteriores.Sum(p => p.TotalQuestoes);
            if (totalRecente > 0 && totalAnterior > 0)
                evolucaoRecente = Math.Round(
                    Percentual(recentes.Sum(p => p.TotalAcertos), totalRecente)
                    - Percentual(anteriores.Sum(p => p.TotalAcertos), totalAnterior), 1);
        }

        var evolucaoPorDisciplina = partidas.GroupBy(p => p.Disciplina)
            .Select(g =>
            {
                var lista = g.ToList();
                var n = Math.Min(5, lista.Count / 2);
                if (n == 0) return new EvolucaoDisciplina(g.Key, null, 0);
                var recentes = lista.Take(n).ToList();
                var anteriores = lista.Skip(n).Take(n).ToList();
                var qr = recentes.Sum(p => p.TotalQuestoes);
                var qa = anteriores.Sum(p => p.TotalQuestoes);
                double? delta = qr > 0 && qa > 0
                    ? Math.Round(Percentual(recentes.Sum(p => p.TotalAcertos), qr)
                        - Percentual(anteriores.Sum(p => p.TotalAcertos), qa), 1)
                    : null;
                return new EvolucaoDisciplina(g.Key, delta, n);
            }).OrderBy(x => x.Nome).ToList();

        var semana = ProgressoSemanalService.Calcular(
            partidas.Select(t => new DesafioBenjamin.Models.Tentativa
            {
                FinalizadaEm = t.FinalizadaEm,
                TotalAcertos = t.TotalAcertos
            }), DateTime.Now);

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

        // Recomendacoes explicaveis baseadas exclusivamente em erros observados.
        // Acoes sugeridas nao classificam capacidade ou diagnostico do aluno.
        var orientacoes = respostas
            .Where(r => !r.Acertou)
            .GroupBy(r => new { r.Disciplina, r.Capitulo })
            .Select(g =>
            {
                var total = respostas.Count(r => r.Disciplina == g.Key.Disciplina && r.Capitulo == g.Key.Capitulo);
                var erros = g.Count();
                var prioridade = erros >= 3 && erros * 2 >= total ? "Priorizar" : "Revisar";
                var sugestao = prioridade == "Priorizar"
                    ? "Retomar o conteúdo com exemplos e depois praticar na revisão inteligente."
                    : "Refazer algumas questões e acompanhar os próximos resultados.";
                return new OrientacaoEstudo(g.Key.Disciplina, g.Key.Capitulo, erros, total, prioridade, sugestao);
            })
            .OrderByDescending(o => o.Prioridade == "Priorizar")
            .ThenByDescending(o => o.Erros)
            .ThenBy(o => o.Disciplina)
            .Take(5)
            .ToList();

        var model = new PainelPedagogicoViewModel
        {
            Semana = semana,
            EvolucaoPorDisciplina = evolucaoPorDisciplina,
            TotalPartidas = partidas.Count,
            EvolucaoRecente = evolucaoRecente,
            PartidasComparadas = janela,
            TotalQuestoes = totalQuestoes,
            TotalAcertos = totalAcertos,
            Aproveitamento = Percentual(totalAcertos, totalQuestoes),
            Disciplinas = disciplinas,
            Historico = partidas.Take(30).Select(t => new PartidaResumo(
                t.Id, t.FinalizadaEm!.Value, t.Disciplina, t.Questionario,
                t.Capitulo, t.TotalAcertos, t.TotalQuestoes,
                Percentual(t.TotalAcertos, t.TotalQuestoes))).ToList(),
            Dificuldades = dificuldades,
            Orientacoes = orientacoes
        };

        Response.Headers.CacheControl = "no-store";
        return View(model);
    }

    private static double Percentual(int parte, int total) =>
        total <= 0 ? 0 : Math.Round(parte * 100.0 / total, 1);
}
