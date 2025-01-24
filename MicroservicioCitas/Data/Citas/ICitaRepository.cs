
using MicroservicioCitas.Models;

namespace MicroservicioCitas.Data.Citas;
public interface ICitaRepository {
    Task<Persona?> ValidarPacienteAsync(string pacienteId);
    Task<Persona?> ValidarMedicoAsync(string medicoId);
    Task<CitaResponseDto> CrearCitaAsync(Cita cita);
    Task<CitaResponseDto> GetCita(int id);
    
}