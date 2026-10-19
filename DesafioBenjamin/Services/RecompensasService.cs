using DesafioBenjamin.Models;

namespace DesafioBenjamin.Services;

/// <summary>Recompensas calculadas a partir de partidas concluidas; nao altera o historico.</summary>
public static class RecompensasService
{
    public static int Estrelas(Tentativa tentativa)
    {
        if (tentativa.FinalizadaEm is null || tentativa.TotalQuestoes <= 0) return 0;
        var percentual = tentativa.TotalAcertos * 100.0 / tentativa.TotalQuestoes;
        return percentual >= 90 ? 3 : percentual >= 70 ? 2 : percentual >= 40 ? 1 : 0;
    }

    public static string Medalha(Tentativa tentativa) => Estrelas(tentativa) switch
    {
        3 => "🥇 Medalha de ouro",
        2 => "🥈 Medalha de prata",
        1 => "🥉 Medalha de bronze",
        _ => "🌱 Medalha de perseverança"
    };

    public static IReadOnlyList<string> Conquistas(IEnumerable<Tentativa> historico)
    {
        var partidas = historico.Where(t => t.FinalizadaEm is not null).ToList();
        var conquistas = new List<string>();
        if (partidas.Count >= 1) conquistas.Add("🚀 Primeira missão concluída");
        if (partidas.Count >= 5) conquistas.Add("🎮 Explorador: 5 desafios");
        if (partidas.Count >= 10) conquistas.Add("🏆 Veterano: 10 desafios");
        if (partidas.Any(t => Estrelas(t) == 3)) conquistas.Add("🌟 Desafio brilhante: 3 estrelas");
        if (partidas.Any(t => t.TotalQuestoes > 0 && t.TotalAcertos == t.TotalQuestoes))
            conquistas.Add("🎯 Precisão total: todas as respostas corretas");
        return conquistas;
    }
}
