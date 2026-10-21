using DesafioBenjamin.Models;

namespace DesafioBenjamin.Services;

/// <summary>Metas de incentivo baseadas apenas em partidas concluidas; sem alterar banco.</summary>
public static class ProgressoSemanalService
{
    public const int MetaPartidas = 3;
    public const int MetaAcertos = 20;

    public static ResumoSemanal Calcular(IEnumerable<Tentativa> historico, DateTime agora)
    {
        var inicio = agora.Date.AddDays(-(((int)agora.DayOfWeek + 6) % 7));
        var fim = inicio.AddDays(7);
        var partidas = historico.Where(t => t.FinalizadaEm >= inicio && t.FinalizadaEm < fim).ToList();
        var acertos = partidas.Sum(t => t.TotalAcertos);
        return new ResumoSemanal(inicio, partidas.Count, acertos,
            partidas.Count >= MetaPartidas, acertos >= MetaAcertos);
    }
}

public sealed record ResumoSemanal(DateTime Inicio, int Partidas, int Acertos,
    bool MetaPartidasAtingida, bool MetaAcertosAtingida);
