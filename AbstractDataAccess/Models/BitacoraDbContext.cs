using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AbstractDataAccess.Models
{
    public class BitacoraDbContext : DbContext
    {
        public BitacoraDbContext(DbContextOptions<BitacoraDbContext> options) : base(options) { }

        public DbSet<Bitacora> Bitacoras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bitacora>().ToTable("Bitacora");
        }
    }
}
