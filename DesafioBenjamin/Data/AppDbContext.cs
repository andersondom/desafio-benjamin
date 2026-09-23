using DesafioBenjamin.Models;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Disciplina> Disciplinas => Set<Disciplina>();
    public DbSet<Questionario> Questionarios => Set<Questionario>();
    public DbSet<Questao> Questoes => Set<Questao>();
    public DbSet<Alternativa> Alternativas => Set<Alternativa>();
    public DbSet<Tentativa> Tentativas => Set<Tentativa>();
    public DbSet<Resposta> Respostas => Set<Resposta>();
    public DbSet<TentativaQuestao> TentativaQuestoes
    => Set<TentativaQuestao>();
}