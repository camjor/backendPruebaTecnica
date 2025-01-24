namespace MicroservicioPersonas.Dtos.PersonaDtos;

public class PersonaRegistroRequestDto
{
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? TipoPersona { get; set; } // "Médico" o "Paciente"
        public string? Especialidad { get; set; } // Solo para médicos
        public string? Identificacion { get; set; }
        public string? Email { get; set; }

        public string? UserName {get; set;}
        public string? Password { get; set; }
}