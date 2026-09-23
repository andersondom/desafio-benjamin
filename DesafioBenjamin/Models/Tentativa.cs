namespace DesafioBenjamin.Models;

public class Tentativa
{
    public int Id { get; set; }

    public DateTime IniciadaEm { get; set; } = DateTime.Now;

    public DateTime? FinalizadaEm { get; set; }

    public int TotalQuestoes { get; set; }

    public int TotalAcertos { get; set; }

    public int TotalErros { get; set; }

    public double PercentualAcertos { get; set; }

    public int AlunoId { get; set; }

    public Aluno Aluno { get; set; } = null!;

    public int QuestionarioId { get; set; }

    public Questionario Questionario { get; set; } = null!;

    public List<Resposta> Respostas { get; set; } = new();

    public List<TentativaQuestao> Questoes { get; set; } = new();
}