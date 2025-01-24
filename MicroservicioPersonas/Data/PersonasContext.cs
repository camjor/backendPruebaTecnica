using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MicroservicioPersonas.Models;

namespace MicroservicioPersonas.Data;
public class PersonasContext : IdentityDbContext <Persona> {    
 
        public PersonasContext(DbContextOptions<PersonasContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Persona> Personas { get; set; }
    
}

