using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroservicioPersonas.Data;
//using MicroservicioPersonas.Models;
using MicroservicioPersonas.Data.Personas;
using MicroservicioPersonas.Dtos.PersonaDtos;
using AutoMapper;
using MicroservicioPersonas.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroservicioPersonas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly IPersonaRepository _repository;
    

    private readonly PersonasContext _context;
    public PersonasController(IPersonaRepository repository, PersonasContext context)
    {
        _repository = repository;
        
        _context = context;
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<PersonaResponseDto>> Login(
        [FromBody] PersonaLoginRequestDto request
    ){
        return await _repository.Login(request);
    }

    [AllowAnonymous]
    [HttpPost("registrar")]
    public async Task<ActionResult<PersonaResponseDto>> registrar(
        [FromBody] PersonaRegistroRequestDto request
    ){
        return await _repository.RegistroUsuario(request);
    }
    
    
    [HttpGet]
    public async Task<ActionResult<PersonaResponseDto>> DevolverUsuario(){
        return await _repository.GetUsuario();
    }

    [AllowAnonymous]
    [HttpGet ("all")]
    public async Task<ActionResult<IEnumerable<Persona>>> GetAll()
    {
         return await _context.Personas.ToListAsync();
        
    }
    // DELETE: api/Personas/5
        [HttpDelete("{identificacion}")]
    public async Task<IActionResult> DeletePersona(string identificacion)
    {
        var persona = await _context.Personas.FirstOrDefaultAsync(p => p.Identificacion == identificacion);//se usa para que la variable primaria cambie a la default
        if (persona == null)
        {
            return NotFound(new { message = $"Persona con identificación '{identificacion}' no encontrada." });
        }
        _context.Personas.Remove(persona);
        await _context.SaveChangesAsync();
        return NoContent();
    }
            // PUT: api/Personas/5
        [HttpPut("{identificacion}")]
    public async Task<IActionResult> PutPersona(string identificacion, Persona persona)
    {
        if (identificacion != persona.Identificacion)
        {
            return BadRequest(new { message = "La identificación no coincide." });
        }
        var existingPersona = await _context.Personas.FirstOrDefaultAsync(p => p.Identificacion == identificacion);
        if (existingPersona == null)
        {
            return NotFound(new { message = $"Persona con identificación '{identificacion}' no encontrada." });
        }
       // _context.Entry(persona).State = EntityState.Modified;
           // Actualizar campos manualmente para evitar sobrescribir datos inesperados
        existingPersona.Nombre = persona.Nombre;
        existingPersona.Apellido = persona.Apellido;
        existingPersona.UserName = persona.UserName;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Conflict(new { message = "Error de concurrencia al actualizar los datos.", details = ex.Message });
        }
        return NoContent();
    }
    

    
}
    