// BitacoraService/Program.cs
using AbstractDataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Services.Implementations;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BitacoraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BitacoraConnection")));

// Cambia la línea por esta:
builder.Services.AddScoped<IBitacoraService, Services.Implementations.BitacoraService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();