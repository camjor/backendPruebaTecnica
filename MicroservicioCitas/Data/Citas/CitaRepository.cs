using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioCitas.Data;
using MicroservicioCitas.Models;
using System.Net.Http.Json;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using MicroservicioCitas.Middleware;
using System.Net;
using Newtonsoft.Json;

namespace MicroservicioCitas.Data.Citas;
public class CitaRepository : ICitaRepository
{
    private readonly CitasContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CitaRepository(
        CitasContext context, 
        HttpClient httpClient, 
        IConfiguration configuration
    ){
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private CitaResponseDto TransformerCitaToCitaDto(Cita cita)
    {
        return new CitaResponseDto {
           
            PacienteId = cita.PacienteId,
            MedicoId = cita.MedicoId,
            Fecha = cita.Fecha,
            Lugar = cita.Lugar,
            Estado = cita.Estado,
            Id = cita.Id,
        };
    }
 
    private Persona TransformerUserToMedicoDto(Persona usuario)
    {
        return new Persona {
           
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            TipoPersona = usuario.TipoPersona,
            Especialidad = usuario.Especialidad,
            Identificacion = usuario.Identificacion,
            Email = usuario.Email,
        };
    }

    public async Task<CitaResponseDto> CrearCitaAsync(Cita cita)
    {
        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();
        
        return TransformerCitaToCitaDto(cita) ;
    }

    public async Task<Persona?> ValidarMedicoAsync(string medicoId)
    {
        var personasApi = _configuration["PersonasApi"];
        var response = await _httpClient.GetAsync($"{personasApi}/medico/{medicoId}");
        if (!response.IsSuccessStatusCode)
        {
            return null; // Paciente no válido
        }

        var content = await response.Content.ReadAsStringAsync();
        var medico=JsonConvert.DeserializeObject<Persona>(content);
        return TransformerUserToMedicoDto(medico);
        
    }

    public async Task<Persona?> ValidarPacienteAsync(string pacienteId)
    {
        var personasApi = _configuration["PersonasApi"];
        var response = await _httpClient.GetAsync($"{personasApi}/{"paciente"}/{pacienteId}");
        if (!response.IsSuccessStatusCode)
        {
            return null; // Paciente no válido
        }

        var content = await response.Content.ReadAsStringAsync();
        var paciente=JsonConvert.DeserializeObject<Persona>(content);
        return TransformerUserToMedicoDto(paciente);
    }

    public async Task<CitaResponseDto> GetCita(int id)
    {
        var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                throw new MiddlewareException(
                    HttpStatusCode.NotFound, 
                    new  {mensaje = "No hay Personas con esa identificacion en la base de datos"}
                );
            }
             return TransformerCitaToCitaDto(cita);
    }

    
}