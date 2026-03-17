using Microsoft.AspNetCore.Authentication.Cookies;
using Pegasos.WEB.Portal.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar autenticación con cookies (5 minutos = PTL4)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
        options.Cookie.Name = "Pegasos.Portal.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddControllersWithViews();

// HttpClient para llamar al Gateway (IGUAL QUE EL ADMIN)
builder.Services.AddHttpClient<IAuthPortalService, AuthPortalService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IPagosService, PagosService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IConsultasService, ConsultasService>(client =>
{
    var baseUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5200/";
    client.BaseAddress = new Uri(baseUrl);
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();