namespace DesafioBenjamin.Models;

public class Alternativa
{
    public int Id { get; set; }

    public string Texto { get; set; } = string.Empty;

    public bool Correta { get; set; }

    public int QuestaoId { get; set; }

    public Questao Questao { get; set; } = null!;
}