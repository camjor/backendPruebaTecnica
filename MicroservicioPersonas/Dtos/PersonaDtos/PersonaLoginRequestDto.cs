namespace MicroservicioPersonas.Dtos.PersonaDtos;

public class PersonaLoginRequestDto
{
        
        //public string? TipoPersona { get; set; } // "Médico" o "Paciente"
        //public string? Especialidad { get; set; } // Solo para médicos
        public string? Email { get; set; }
        public string? Password { get; set; }
       /// public string? Identificacion { get; set; }
}