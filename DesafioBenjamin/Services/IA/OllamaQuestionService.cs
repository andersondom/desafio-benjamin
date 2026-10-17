using System.Net.Http.Json;
using System.Text.Json;

namespace DesafioBenjamin.Services.IA;

public sealed class OllamaQuestionService
{
    private readonly HttpClient _client;
    private readonly string _model;

    public OllamaQuestionService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"] ?? "http://ollama:11434");
        _client.Timeout = TimeSpan.FromSeconds(90);
        _model = configuration["Ollama:Model"] ?? "qwen3:4b";
    }

    public async Task<QuestaoGerada> GerarRascunhoAsync(string textoDidatico, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(textoDidatico))
            throw new ArgumentException("Texto didatico obrigatorio.", nameof(textoDidatico));

        if (textoDidatico.Length > 12000)
            throw new ArgumentException("Texto didatico excede o limite desta versao.", nameof(textoDidatico));

        var payload = new
        {
            model = _model,
            stream = false,
            think = false,
            format = "json",
            messages = new object[]
            {
                new { role = "system", content = "Voce cria apenas rascunhos de questoes educativas em portugues. Retorne somente JSON valido com propriedades enunciado, alternativas (array de 4 textos), indiceCorreto (0 a 3) e explicacao. Nao inclua raciocinio ou comentarios." },
                new { role = "user", content = $"Crie uma questao de multipla escolha, fiel exclusivamente ao material a seguir. /no_think\n\n{textoDidatico}" }
            },
            options = new { num_predict = 400, num_ctx = 2048, temperature = 0.2 }
        };

        using var response = await _client.PostAsJsonAsync("/api/chat", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var raw = json.RootElement.GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("IA nao retornou conteudo.");

        QuestaoGerada? questao;
        try { questao = JsonSerializer.Deserialize<QuestaoGerada>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch (JsonException ex) { throw new InvalidOperationException("Resposta da IA nao e um JSON valido.", ex); }

        if (questao is null || string.IsNullOrWhiteSpace(questao.Enunciado) ||
            questao.Alternativas is not { Length: 4 } ||
            questao.Alternativas.Any(string.IsNullOrWhiteSpace) ||
            questao.Alternativas.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4 ||
            questao.IndiceCorreto is < 0 or > 3 ||
            string.IsNullOrWhiteSpace(questao.Explicacao))
            throw new InvalidOperationException("Resposta da IA nao atende aos criterios minimos de uma questao.");

        return questao; // Draft only. No automatic persistence or publication.
    }
}

public sealed class QuestaoGerada
{
    public string Enunciado { get; set; } = "";
    public string[] Alternativas { get; set; } = Array.Empty<string>();
    public int IndiceCorreto { get; set; }
    public string Explicacao { get; set; } = "";
}
