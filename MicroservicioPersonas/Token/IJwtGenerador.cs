using MicroservicioPersonas.Models;

namespace MicroservicioPersonas.Token;

public interface IJwtGenerador{
    string CrearToken (Persona usuario);
}