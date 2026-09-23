namespace DesafioBenjamin.Models;

public class Aluno
{

}namespace DesafioBenjamin.Models;

public class Aluno
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public bool Ativo { get; set; } = true;

    public List<Tentativa> Tentativas { get; set; } = new();
}