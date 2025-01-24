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
using MicroservicioCitas.Data.Citas;


namespace MicroservicioCitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly CitasContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private readonly ICitaRepository _citaRepository;

        public CitasController(
            CitasContext context, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ICitaRepository citaRepository )
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _citaRepository = citaRepository;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            return await _context.Citas.ToListAsync();
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CitaResponseDto>> GetCita(int id)
        {
            var cita = await _citaRepository.GetCita(id);
            var pacienteValido = await _citaRepository.ValidarPacienteAsync(cita.PacienteId);
            var medicoValido = await _citaRepository.ValidarMedicoAsync(cita.MedicoId);
            var resultado = new
            {
                Cita = cita,
                paciente = pacienteValido,
                Medico = medicoValido
            };
            return Ok (resultado);
            
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostCita(Cita cita)
        {
            // Validar médico y paciente
            var pacienteValido = await _citaRepository.ValidarPacienteAsync(cita.PacienteId);
            var medicoValido = await _citaRepository.ValidarMedicoAsync(cita.MedicoId);

            if ( pacienteValido == null || medicoValido==null)
            {
                return BadRequest("Paciente o médico no válido.");
            }

            // Crear cita
            var nuevaCita = await _citaRepository.CrearCitaAsync(cita);
            // Retornar los datos del paciente y médico junto con la cita creada
            var resultado = new
            {
                Cita = nuevaCita,
                paciente = pacienteValido,
                Medico = medicoValido
            };

            return CreatedAtAction(nameof(GetCita), new { id = nuevaCita.Id }, resultado);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound(new { message = $"Persona con identificación '{id}' no encontrada." });
            }
            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
            var mensaje = new
            {
                Mensaje = $"Se elimino la cita con el ID {cita.Id}",
            };
            return Ok (mensaje);
        }

        [AllowAnonymous]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] string nuevoEstado)
        {
                // Generar una receta por defecto

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            cita.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            // Enviar mensaje a RabbitMQ si la cita se finaliza
            if (nuevoEstado == "Finalizada")
            {
                EnviarMensajeRabbitMQ(cita);
                var mensaje = new
                {
                    Mensaje = $"Se ha generado una receta por defecto con el ID de paciente {cita.PacienteId}",
                    Nota = "Se recomienda cambiar la descripción de la receta por la de su concepto."
                };
                return Ok (mensaje);
            }else
            {
                var verCita = await _citaRepository.GetCita(id);
                return Ok (verCita);
            }

            return NoContent();
        }

        private void EnviarMensajeRabbitMQ(Cita cita)
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory()
                {
                    HostName = "localhost", // Cambiar según tu configuración
                    UserName = "guest",
                    Password = "guest"
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "recetas",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                    string message = JsonSerializer.Serialize(cita);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                        routingKey: "recetas",
                                        basicProperties: null,
                                        body: body);

                    Console.WriteLine("Mensaje enviado: {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectarse a RabbitMQ: {0}", ex.Message);
            }
        }
    }
}