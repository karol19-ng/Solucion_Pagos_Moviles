using API_Gateway.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// GTW1: Cargar Ocelot desde la carpeta Configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// GTW2: Configurar autenticación JWT
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

builder.Services.AddOcelot(builder.Configuration); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();


app.UseMiddleware<GatewayAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// 3. Ocelot siempre al final
await app.UseOcelot();

app.Run();