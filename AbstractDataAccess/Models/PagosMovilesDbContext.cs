using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AbstractDataAccess.Models
{
    public class PagosMovilesDbContext : DbContext
    {
        public PagosMovilesDbContext(DbContextOptions<PagosMovilesDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<TablaPantalla> TablaPantallas { get; set; }
        public DbSet<RolPorPantalla> RolPorPantallas { get; set; }
        public DbSet<Entidad> Entidades { get; set; }
        public DbSet<Parametro> Parametros { get; set; }
        public DbSet<InicioSesion> InicioSesiones { get; set; }
        public DbSet<TransaccionEnvio> TransaccionEnvios { get; set; }
        public DbSet<Afiliacion> Afiliacion { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Rol>().ToTable("Roles");
            modelBuilder.Entity<TablaPantalla>().ToTable("Tabla_Pantallas");
            modelBuilder.Entity<RolPorPantalla>().ToTable("Rol_Por_Pantalla");
            modelBuilder.Entity<Entidad>().ToTable("Entidades");
            modelBuilder.Entity<Parametro>().ToTable("Parametros");
            modelBuilder.Entity<InicioSesion>().ToTable("Inicio_Sesion");
            modelBuilder.Entity<TransaccionEnvio>().ToTable("Transaccion_Envio");
            modelBuilder.Entity<Afiliacion>().ToTable("Afiliacion");
            modelBuilder.Entity<Estado>().ToTable("Estados");
            modelBuilder.Entity<TipoIdentificacion>().ToTable("Tipos_Identificacion");
        }
    }
}
