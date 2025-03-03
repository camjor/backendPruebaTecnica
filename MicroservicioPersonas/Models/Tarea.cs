using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioPersonas.Models;
public class Tarea{
    [Key]
    [Required]
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }

    public string? Estado { get; set; }
    public string? Fecha { get; set; }
     public Guid UsuarioId { get; set; }
}