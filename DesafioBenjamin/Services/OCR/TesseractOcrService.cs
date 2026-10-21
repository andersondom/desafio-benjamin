using System.Diagnostics;

namespace DesafioBenjamin.Services.OCR;

public sealed class TesseractOcrService : IOcrService
{
    private static readonly HashSet<string> IdiomasPermitidos = new(StringComparer.Ordinal)
    {
        "por", "eng", "por+eng", "eng+por"
    };

    public async Task<string> ExtrairTextoAsync(string caminhoImagem, string idioma = "por+eng", CancellationToken cancellationToken = default)
    {
        if (!IdiomasPermitidos.Contains(idioma))
            throw new ArgumentException("Idioma OCR nao permitido.", nameof(idioma));

        if (string.IsNullOrWhiteSpace(caminhoImagem) || !File.Exists(caminhoImagem))
            throw new FileNotFoundException("Imagem para OCR nao encontrada.", caminhoImagem);

        var startInfo = new ProcessStartInfo("tesseract")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(caminhoImagem);
        startInfo.ArgumentList.Add("stdout");
        startInfo.ArgumentList.Add("-l");
        startInfo.ArgumentList.Add(idioma);

        using var processo = new Process { StartInfo = startInfo };
        if (!processo.Start()) throw new InvalidOperationException("Nao foi possivel iniciar o Tesseract.");

        var saidaTask = processo.StandardOutput.ReadToEndAsync(cancellationToken);
        var erroTask = processo.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await processo.WaitForExitAsync(cancellationToken);
            var saida = await saidaTask;
            var erro = await erroTask;
            if (processo.ExitCode != 0)
                throw new InvalidOperationException($"Falha no OCR (codigo {processo.ExitCode}): {erro}");
            return saida.Trim();
        }
        catch (OperationCanceledException)
        {
            if (!processo.HasExited) processo.Kill(entireProcessTree: true);
            throw;
        }
    }
}
