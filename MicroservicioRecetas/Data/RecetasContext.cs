using Microsoft.EntityFrameworkCore;
using MicroservicioRecetas.Models;

namespace MicroservicioRecetas.Data
{
    public class RecetasContext : DbContext
    {
        public RecetasContext(DbContextOptions<RecetasContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Receta> Recetas { get; set; }
    }
}
