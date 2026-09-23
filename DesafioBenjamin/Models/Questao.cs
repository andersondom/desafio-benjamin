namespace DesafioBenjamin.Models;

public class Questao
{
    public int Id { get; set; }

    public string Enunciado { get; set; } = string.Empty;

    public string? Explicacao { get; set; }

    public int Ordem { get; set; }

    public int QuestionarioId { get; set; }

    public Questionario Questionario { get; set; } = null!;

    public List<Alternativa> Alternativas { get; set; } = new();
}