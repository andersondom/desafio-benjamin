namespace DesafioBenjamin.Models;

public class Disciplina
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Icone { get; set; }

    public bool Ativa { get; set; } = true;

    public List<Questionario> Questionarios { get; set; } = new();
}