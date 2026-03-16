using API_Gateway.Middleware;
using Gateway_2.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// GTW1: Configurar Ocelot
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// GTW2: Configurar autenticación JWT para validación
builder.Services.AddAuthentication()
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

builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// GTW2: Middleware de validación ANTES de Ocelot
app.UseMiddleware<GatewayAuthenticationMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseMiddleware<GatewayAuthMiddleware>();
// GTW1: Ocelot como middleware final
await app.UseOcelot();

app.Run();