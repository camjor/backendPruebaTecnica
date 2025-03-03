using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace MicroservicioPersonas.Models
{
    public class Persona : IdentityUser
    {
        //[Key]
        //[Required]
        //public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Identificacion { get; set; }
    }
}
