using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MicroservicioCitas.Models;

namespace MicroservicioCitas.Data
{
    public class CitasContext : DbContext
    {
        public CitasContext(DbContextOptions<CitasContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Cita> Citas { get; set; }
    }
}
