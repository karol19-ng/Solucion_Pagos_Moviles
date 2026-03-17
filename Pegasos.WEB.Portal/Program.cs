using Microsoft.AspNetCore.Authentication.Cookies;
using Pegasos.WEB.Portal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure HttpClient for Gateway
builder.Services.AddHttpClient<IAuthPortalService, AuthPortalService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]
        ?? "http://localhost:5000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IPagosService, PagosService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]
        ?? "http://localhost:5000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IConsultasService, ConsultasService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayUrl"]
        ?? "http://localhost:5000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Cookie Authentication (PTL1, PTL3)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // PTL4: 5 minutos de inactividad
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();