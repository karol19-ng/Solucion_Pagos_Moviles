using AbstractDataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Implementations;
using Services.Interfaces;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "API Proyecto1", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de un espacio y luego el token.\n\nEjemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// DbContexts
builder.Services.AddDbContext<CoreBancarioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoreBancarioConnection")));
builder.Services.AddDbContext<PagosMovilesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PagosMovilesConnection")));
builder.Services.AddDbContext<BitacoraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BitacoraConnection")));

// HttpClient
builder.Services.AddHttpClient();

// CORS ← NUEVO
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8081",
                "http://localhost:19006",
                "exp://localhost:8081"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Services
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPantallaService, PantallaService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IParametroService, ParametroService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IEntidadService, EntidadService>();
builder.Services.AddScoped<ITransaccionService, TransaccionService>();
builder.Services.AddScoped<IAfiliacionService, AfiliacionService>();
builder.Services.AddScoped<ICoreBancarioService, CoreBancarioService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowMobileApp"); // ← NUEVO - debe ir primero

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
