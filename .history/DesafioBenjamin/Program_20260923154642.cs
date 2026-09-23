using DesafioBenjamin.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// BANCO DE DADOS
// ======================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ======================================================
// PIPELINE HTTP
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Arquivos estáticos: CSS, JS, imagens etc.
app.MapStaticAssets();

// ======================================================
// ROTAS
// ======================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ======================================================
// INICIALIZAÇÃO
// ======================================================

app.Run();