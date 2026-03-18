open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Extensions
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open NexusPay.Web.Admin.Services

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

// 1. Configurar Autenticación (SA4: 5 minutos)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(fun options ->
        options.LoginPath <- PathString("/Auth/Login")
        options.LogoutPath <- PathString("/Auth/Logout")
        options.AccessDeniedPath <- PathString("/Auth/AccessDenied")
        options.ExpireTimeSpan <- TimeSpan.FromMinutes(5.0)
        options.SlidingExpiration <- false
        options.Cookie.Name <- "NexusPay.Admin.Session"
        options.Cookie.HttpOnly <- true
        options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest
        options.Cookie.SameSite <- SameSiteMode.Lax
    ) |> ignore

builder.Services.AddControllersWithViews() |> ignore

// 2. HttpClient para Auth (Apunta al Ocelot 2 - Puerto 5001)
builder.Services.AddHttpClient<IAuthService, AuthService>(fun client ->
    let authUrl = builder.Configuration.["GatewayUrls:Auth"]
    client.BaseAddress <- Uri(authUrl)
    client.Timeout <- TimeSpan.FromSeconds(30.0)
) |> ignore

// 3. HttpClient para Admin/Core (Apunta al Ocelot 1 - Puerto 5000)
builder.Services.AddHttpClient<IAdminService, AdminService>(fun client ->
    let coreUrl = builder.Configuration.["GatewayUrls:Core"]
    client.BaseAddress <- Uri(coreUrl)
    client.DefaultRequestHeaders.Add("Accept", "application/json")
) |> ignore

var app = builder.Build()

if not (app.Environment.IsDevelopment()) then
    app.UseExceptionHandler("/Home/Error") |> ignore
    app.UseHsts() |> ignore

// Si el HTTPS te da problemas de certificado, puedes comentar esta línea temporalmente
app.UseHttpsRedirection() |> ignore
app.UseStaticFiles() |> ignore

app.UseRouting() |> ignore

app.UseAuthentication() |> ignore
app.UseAuthorization() |> ignore

app.MapControllerRoute(
    name = "default",
    pattern = "{controller=Auth}/{action=Login}/{id?}") |> ignore

app.Run()
