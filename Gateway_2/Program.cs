using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Gateway_2.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gateway_2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cargar Ocelot
            builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

            // 1. Registro de Autenticación con el nombre "GatewayAuth"
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("GatewayAuth", options => // <--- Este nombre debe estar en el JSON
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddOcelot();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 2. Orden del Pipeline (IMPORTANTE)
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            // Primero la autenticación de ASP.NET, luego Middleware 
            app.UseAuthentication();
            app.UseAuthorization();

            // Tu middleware solo una vez
            app.UseMiddleware<GatewayAuthMiddleware>();

            // Ocelot 
            await app.UseOcelot();

            app.Run();
        }
    }
}
