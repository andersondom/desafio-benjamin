using DesafioBenjamin.Models;
using DesafioBenjamin.Services;

// Regression checks without external test packages or database changes.
// Run: dotnet run --project tests/RecompensasSmoke/RecompensasSmoke.csproj
static void Check(bool ok, string description)
{
    if (!ok) throw new Exception("FALHA: " + description);
    Console.WriteLine("OK: " + description);
}
static Tentativa Finished(int correct, int total) => new()
{
    TotalAcertos = correct,
    TotalQuestoes = total,
    FinalizadaEm = DateTime.Now
};
Check(RecompensasService.Estrelas(new Tentativa { TotalAcertos = 10, TotalQuestoes = 10 }) == 0, "Partida em andamento nao recebe estrelas");
Check(RecompensasService.Estrelas(Finished(0, 0)) == 0, "Sem questoes nao recebe estrelas");
Check(RecompensasService.Estrelas(Finished(3, 10)) == 0, "30 por cento");
Check(RecompensasService.Estrelas(Finished(4, 10)) == 1, "Limite bronze 40 por cento");
Check(RecompensasService.Estrelas(Finished(7, 10)) == 2, "Limite prata 70 por cento");
Check(RecompensasService.Estrelas(Finished(9, 10)) == 3, "Limite ouro 90 por cento");
Check(RecompensasService.Medalha(Finished(10, 10)).Contains("ouro"), "Medalha ouro");
Check(RecompensasService.Conquistas(new [] { Finished(10, 10) }).Count == 3, "Primeira partida perfeita concede 3 conquistas");
Check(RecompensasService.Conquistas(Enumerable.Range(0, 5).Select(_ => Finished(1, 10))).Count == 2, "Conquista 5 partidas");
Check(RecompensasService.Conquistas(Enumerable.Range(0, 10).Select(_ => Finished(1, 10))).Count == 3, "Conquista 10 partidas");
Console.WriteLine("APROVADO: 10 verificacoes de recompensas.");
