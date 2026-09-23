namespace DesafioBenjamin.Models;

public class Resposta
{
    public int Id { get; set; }

    public DateTime RespondidaEm { get; set; } = DateTime.Now;

    public bool Acertou { get; set; }

    public int TentativaId { get; set; }

    public Tentativa Tentativa { get; set; } = null!;

    public int QuestaoId { get; set; }

    public Questao Questao { get; set; } = null!;

    public int AlternativaSelecionadaId { get; set; }

    public Alternativa AlternativaSelecionada { get; set; } = null!;
}