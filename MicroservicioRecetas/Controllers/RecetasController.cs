using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioRecetas.Data;
using MicroservicioRecetas.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using MicroservicioRecetas.Data.Recetas;
using MicroservicioRecetas.Middleware;
using System.Net;

namespace MicroservicioRecetas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecetasController : ControllerBase
    {
        private readonly RecetasContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRecetaRepository _recetaRepository;

        public RecetasController(
            RecetasContext context, 
            IConfiguration configuration,
            IRecetaRepository recetaRepository
            )
        {
            _context = context;
            _configuration = configuration;
            _recetaRepository = recetaRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetas()
        {
            return await _context.Recetas.ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet("{codigo}")]
        public async Task<ActionResult<Receta>> GetRecetaByCodigo(string codigo)
        {
            var receta = await _recetaRepository.GetRecetaByCodigo(codigo);
            if (receta == null)
            {
                return NotFound();
            }
            return Ok (receta);
        }

        [AllowAnonymous]
        [HttpGet("paciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetasByPaciente(string pacienteId)
        {
            return await _context.Recetas.Where(r => r.PacienteId == pacienteId).ToListAsync();
        }

        [AllowAnonymous]
        [HttpPut("{codigo}/estadoYdescripcion")]
        public async Task<IActionResult> ActualizarEstado(string codigo, Receta receta)
        {
             var recetaActualizada= await _recetaRepository.ActualizarEstado(codigo, receta);
            return Ok (recetaActualizada);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Receta>> CrearReceta(Receta receta)
        {
           var nuevaReceta = await _recetaRepository.CrearReceta(receta);
            return CreatedAtAction(nameof(GetRecetaByCodigo), new { codigo = nuevaReceta.Codigo }, nuevaReceta);
        }
    
        [AllowAnonymous]
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> DeleteReceta(string codigo)
        {
            
            var receta = await _context.Recetas.FirstOrDefaultAsync(r => r.Codigo == codigo);
            if (receta == null)
            {
                throw new MiddlewareException(
                    HttpStatusCode.NotFound, 
                    new  {mensaje = $"Persona con identificación '{receta.Codigo}' no encontrada."}
                );
                
            }
            _context.Recetas.Remove(receta);
            await _context.SaveChangesAsync();
            var mensaje = new
            {
                Mensaje = $"Se elimino la receta con codigo {receta.Codigo}",
            };
            return Ok (mensaje);
        }
    }
}
