namespace DesafioBenjamin.Services.OCR;

public interface IOcrService
{
    Task<string> ExtrairTextoAsync(string caminhoImagem, string idioma = "por+eng", CancellationToken cancellationToken = default);
}
