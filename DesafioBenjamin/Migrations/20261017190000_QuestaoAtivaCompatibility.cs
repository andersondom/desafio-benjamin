using DesafioBenjamin.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioBenjamin.Migrations
{
    // Migration de compatibilidade: o esquema ja foi atualizado em instalacoes existentes
    // por DbInitializer. A verificacao idempotente continua atendendo instalacoes novas.
    [DbContext(typeof(AppDbContext))]
    [Migration("20261017190000_QuestaoAtivaCompatibility")]
    public partial class QuestaoAtivaCompatibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Coluna Ativa e garantida de forma idempotente por DbInitializer
            // apos MigrateAsync, sem recriar a tabela nem perder historico.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nao remover a coluna: reversao destrutiva poderia perder estado.
        }
    }
}
