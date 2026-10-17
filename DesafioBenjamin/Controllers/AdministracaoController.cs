using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DesafioBenjamin.Controllers;

[Route("administracao")]
public sealed class AdministracaoController : Controller
{
    private readonly IConfiguration _configuration;

    public AdministracaoController(IConfiguration configuration) => _configuration = configuration;

    [AllowAnonymous]
    [HttpGet("entrar")]
    public IActionResult Entrar(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/laboratorio-ia";
        ViewBag.Configurado = CredenciaisConfiguradas();
        return View();
    }

    [AllowAnonymous]
    [HttpPost("entrar")]
    [EnableRateLimiting("admin-login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string usuario, string senha, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/laboratorio-ia";
        ViewBag.Configurado = CredenciaisConfiguradas();
        if (!CredenciaisConfiguradas())
        {
            ViewBag.Erro = "Configure ADMIN_USER e ADMIN_PASSWORD no ambiente da aplicacao.";
            return View();
        }

        var usuarioEsperado = _configuration["ADMIN_USER"]!;
        var senhaEsperada = _configuration["ADMIN_PASSWORD"]!;
        if (!Comparar(usuario ?? "", usuarioEsperado) || !Comparar(senha ?? "", senhaEsperada))
        {
            ViewBag.Erro = "Usuario ou senha invalidos.";
            return View();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuarioEsperado),
            new Claim(ClaimTypes.Role, "Administrador")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return LocalRedirect((string)ViewBag.ReturnUrl);
    }

    [Authorize]
    [HttpPost("sair")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Entrar));
    }

    private bool CredenciaisConfiguradas() =>
        !string.IsNullOrWhiteSpace(_configuration["ADMIN_USER"]) &&
        !string.IsNullOrWhiteSpace(_configuration["ADMIN_PASSWORD"]) &&
        _configuration["ADMIN_PASSWORD"]!.Length >= 12;

    private static bool Comparar(string valor, string esperado) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(valor)),
            SHA256.HashData(Encoding.UTF8.GetBytes(esperado)));
}
