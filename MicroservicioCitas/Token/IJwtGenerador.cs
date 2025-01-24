using MicroservicioCitas.Models;

namespace MicroservicioCitas.Token;

public interface IJwtGenerador{
    string CrearToken (Cita usuario);
}