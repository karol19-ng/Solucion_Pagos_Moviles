using API_Gateway.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// GTW1: Configurar Ocelot
builder.Configuration.AddJsonFile("Configuration/Ocelot .json", optional: false, reloadOnChange: true);
// GTW2: Configurar autenticación JWT para validación
builder.Services.AddAuthentication("GatewayAuth")  // ← Especificar esquema por defecto
    .AddJwtBearer("GatewayAuth", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,    
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });




// IMPORTANTE: Registrar HttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("GlobalDownstreamHandler")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddOcelot();

//Implemento del corsn 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.WithOrigins("http://localhost:8081", "http://localhost:19006", "exp://localhost:8081")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowMobileApp");
// IMPORTANTE: Agregar UseAuthentication y UseAuthorization ANTES del middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();  // ← NUEVO
app.UseAuthorization();   // ← NUEVO

// GTW2: Middleware de validación ANTES de Ocelot
app.UseMiddleware<GatewayAuthenticationMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();



// GTW1: Ocelot como middleware final
await app.UseOcelot();

app.Run();