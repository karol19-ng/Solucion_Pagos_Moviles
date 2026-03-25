using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AbstractDataAccess.Models
{
    public class CoreBancarioDbContext : DbContext
    {
        public CoreBancarioDbContext(DbContextOptions<CoreBancarioDbContext> options) : base(options) { }

        public DbSet<ClienteBanco> ClientesBanco { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<MovimientoCuenta> MovimientosCuenta { get; set; }
        public DbSet<EstadoCore> EstadosCore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClienteBanco>().ToTable("Cliente_Banco");
            modelBuilder.Entity<Cuenta>().ToTable("Cuentas");
            modelBuilder.Entity<MovimientoCuenta>().ToTable("Movimiento_Cuenta");
            modelBuilder.Entity<EstadoCore>().ToTable("Estados");
        }
    }
}
