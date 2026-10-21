namespace DesafioBenjamin.ViewModels;

public sealed class PainelPedagogicoViewModel
{
    public DesafioBenjamin.Services.ResumoSemanal Semana { get; init; } = new(DateTime.MinValue, 0, 0, false, false);
    public List<EvolucaoDisciplina> EvolucaoPorDisciplina { get; init; } = new();
    public int TotalPartidas { get; init; }
    public int TotalQuestoes { get; init; }
    public int TotalAcertos { get; init; }
    public double Aproveitamento { get; init; }
    public double? EvolucaoRecente { get; init; }
    public int PartidasComparadas { get; init; }
    public List<DesempenhoDisciplina> Disciplinas { get; init; } = new();
    public List<PartidaResumo> Historico { get; init; } = new();
    public List<OrientacaoEstudo> Orientacoes { get; init; } = new();
    public List<DificuldadeResumo> Dificuldades { get; init; } = new();
}

public sealed record DesempenhoDisciplina(string Nome, int Partidas, int Acertos, int Questoes, double Percentual);
public sealed record PartidaResumo(int Id, DateTime Data, string Disciplina, string Questionario, string? Capitulo, int Acertos, int Questoes, double Percentual);
public sealed record DificuldadeResumo(string Disciplina, string? Capitulo, string Questao, int Erros, int Respostas, double PercentualErros);

public sealed record OrientacaoEstudo(string Disciplina, string? Capitulo, int Erros, int Respostas, string Prioridade, string Sugestao);

public sealed record EvolucaoDisciplina(string Nome, double? Diferenca, int PartidasComparadas);
