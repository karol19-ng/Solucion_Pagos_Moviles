using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Pegasos.Web.Administrador.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Configurar autenticaci�n con cookies (5 minutos = SA4)
// Configurar autenticación con cookies (5 minutos = SA4)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // SA4: 5 minutos exactos
        options.SlidingExpiration = false; // Forzar re-login después de 5 min de inactividad
        options.Cookie.Name = "NexusPay.Admin.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// 🔴 NUEVO: Configurar sesión para guardar el token JWT
builder.Services.AddDistributedMemoryCache(); // Almacenamiento en memoria para la sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // La sesión expira a los 5 minutos también
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "NexusPay.Admin.SessionState";
});

builder.Services.AddControllersWithViews();

// IMPORTANTE: Agregar IHttpContextAccessor para que funcione ScreenService
builder.Services.AddHttpContextAccessor();

// HttpClient para llamar al Gateway (GTW1) - AuthService
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar IScreenService con su propio HttpClient
builder.Services.AddHttpClient<IPantallaService, PantallaService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar ClienteCoreService
builder.Services.AddHttpClient<IClienteCoreService, ClienteCoreService>(client =>
{
    // Esta URL debe ser la del GATEWAY 1 (puerto 7096)
    client.BaseAddress = new Uri("https://localhost:7096/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// REGISTRAR CuentaCoreService - NUEVO
builder.Services.AddHttpClient<ICuentaCoreService, CuentaCoreService>(client =>
{
    // La misma URL del GATEWAY 1 (puerto 7096)
    client.BaseAddress = new Uri("https://localhost:7096/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar ParametroService
builder.Services.AddHttpClient<IParametroService, ParametroService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7096/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar EntidadService
builder.Services.AddHttpClient<IEntidadService, EntidadService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7096/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
// Después de los otros AddHttpClient
builder.Services.AddHttpClient<IPantallaService, PantallaService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IRolService, RolService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7096/"); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔴 NUEVO: Importante - Session debe ir antes de Authentication
app.UseSession(); // HABILITAR SESIÓN

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();