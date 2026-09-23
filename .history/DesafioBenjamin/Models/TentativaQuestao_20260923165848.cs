namespace DesafioBenjamin.Models;

public class TentativaQuestao
{
    public int Id { get; set; }

    public int Ordem { get; set; }

    public int TentativaId { get; set; }
    public Tentativa Tentativa { get; set; } = null!;

    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;
}