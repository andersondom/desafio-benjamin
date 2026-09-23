using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioBenjamin.Migrations
{
    /// <inheritdoc />
    public partial class AddTentativaQuestoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TentativaQuestoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    TentativaId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TentativaQuestoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TentativaQuestoes_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TentativaQuestoes_Tentativas_TentativaId",
                        column: x => x.TentativaId,
                        principalTable: "Tentativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TentativaQuestoes_QuestaoId",
                table: "TentativaQuestoes",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TentativaQuestoes_TentativaId",
                table: "TentativaQuestoes",
                column: "TentativaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TentativaQuestoes");
        }
    }
}
