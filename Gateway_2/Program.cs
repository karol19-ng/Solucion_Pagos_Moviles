using Gateway_2.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar Ocelot
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// Configurar autenticación JWT
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

// IMPORTANTE: Registrar HttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// Middleware de autenticación personalizado
app.UseMiddleware<GatewayAuthMiddleware>();

await app.UseOcelot();

app.Run();