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
// Check real entity relationships without a database or external packages.
var question = new Questao { Id = 11, Enunciado = "Quanto e 2 + 2?", Ordem = 1 };
var correct = new Alternativa { Id = 101, QuestaoId = question.Id, Questao = question, Texto = "4", Correta = true };
var wrong = new Alternativa { Id = 102, QuestaoId = question.Id, Questao = question, Texto = "5", Correta = false };
question.Alternativas.AddRange(new[] { correct, wrong });
Check(question.Alternativas.Count == 2 && question.Alternativas.Count(a => a.Correta) == 1, "Questao com duas alternativas e uma correta");
var attempt = Finished(1, 1);
attempt.Id = 7;
attempt.Questoes.Add(new TentativaQuestao { TentativaId = attempt.Id, Tentativa = attempt, QuestaoId = question.Id, Questao = question, Ordem = 1 });
Check(attempt.Questoes.Single().QuestaoId == question.Id && attempt.Questoes.Single().Ordem == 1, "Ordem e vinculo da questao na tentativa");
attempt.Respostas.Add(new Resposta { TentativaId = attempt.Id, Tentativa = attempt, QuestaoId = question.Id, Questao = question, AlternativaSelecionadaId = correct.Id, AlternativaSelecionada = correct, Acertou = true });
Check(attempt.Respostas.Single().Acertou && attempt.Respostas.Single().AlternativaSelecionada.Correta, "Resposta correta vinculada a alternativa");
attempt.Respostas.Add(new Resposta { TentativaId = attempt.Id, Tentativa = attempt, QuestaoId = question.Id, Questao = question, AlternativaSelecionadaId = wrong.Id, AlternativaSelecionada = wrong, Acertou = false });
Check(!attempt.Respostas.Last().Acertou && !attempt.Respostas.Last().AlternativaSelecionada.Correta, "Resposta incorreta preservada no historico da entidade");
Check(attempt.Respostas.Count == 2 && attempt.Respostas.All(r => r.TentativaId == attempt.Id), "Respostas associadas a tentativa");
var ongoing = new Tentativa { TotalQuestoes = 10, TotalAcertos = 10 };
Check(RecompensasService.Conquistas(new[] { ongoing }).Count == 0, "Partida nao finalizada nao gera conquistas");
Check(RecompensasService.Conquistas(new[] { Finished(9, 10), ongoing }).Count == 2, "Historico ignora partida em andamento");
Check(RecompensasService.Conquistas(new[] { Finished(10, 10), Finished(10, 10) }).Count == 3, "Conquistas nao duplicam");
var quarta = new DateTime(2026, 10, 21, 12, 0, 0);
var inicioSemana = new DateTime(2026, 10, 19);
var partidasSemana = new[]
{
    new Tentativa { FinalizadaEm = inicioSemana, TotalAcertos = 8 },
    new Tentativa { FinalizadaEm = inicioSemana.AddDays(1), TotalAcertos = 7 },
    new Tentativa { FinalizadaEm = quarta, TotalAcertos = 5 },
    new Tentativa { FinalizadaEm = inicioSemana.AddDays(-1), TotalAcertos = 99 },
    new Tentativa { TotalAcertos = 99 }
};
var metas = ProgressoSemanalService.Calcular(partidasSemana, quarta);
Check(metas.Inicio == inicioSemana, "Semana inicia na segunda-feira");
Check(metas.Partidas == 3 && metas.Acertos == 20, "Metas ignoram semana anterior e partida aberta");
Check(metas.MetaPartidasAtingida && metas.MetaAcertosAtingida, "Duas missoes semanais cumpridas");
var metasVazias = ProgressoSemanalService.Calcular(Array.Empty<Tentativa>(), quarta);
Check(metasVazias.Partidas == 0 && !metasVazias.MetaPartidasAtingida && !metasVazias.MetaAcertosAtingida, "Semana sem partidas nao gera conquista");
Console.WriteLine("APROVADO: 22 verificacoes de modelos, historico, recompensas e metas semanais.");
