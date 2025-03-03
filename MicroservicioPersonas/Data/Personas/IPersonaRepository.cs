using MicroservicioPersonas.Dtos.PersonaDtos;
using MicroservicioPersonas.Models;

namespace MicroservicioPersonas.Data.Personas;

public interface IPersonaRepository {
    Task<PersonaResponseDto> GetUsuario();//las clase Dto es para filtar informacion
    Task<PersonaResponseDto> Login(PersonaLoginRequestDto request);
    Task<PersonaResponseDto> RegistroUsuario(PersonaRegistroRequestDto request);
    
}