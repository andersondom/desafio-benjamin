namespace DesafioBenjamin.Models;

public class Questionario
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Capitulo { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public bool Ativo { get; set; } = true;

    public int DisciplinaId { get; set; }

    public Disciplina Disciplina { get; set; } = null!;

    public List<Questao> Questoes { get; set; } = new();

    public List<Tentativa> Tentativas { get; set; } = new();
}