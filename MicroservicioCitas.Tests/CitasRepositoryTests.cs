

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MicroservicioCitas.Data;
using MicroservicioCitas.Data.Citas;
using MicroservicioCitas.Models;
using MicroservicioCitas.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MicroservicioCitas.Tests
{
    public class CitasRepositoryTests
    {
        
        private CitasContext CrearContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<CitasContext>()
                .UseInMemoryDatabase(databaseName: "CitasDB") // Genera una nueva base de datos en memoria para cada prueba
                .Options;

            var context = new CitasContext(options);
            
           // _CitasController = new CitasController(context, null, null,null);

            return context;
        }

        [Fact]
        public async Task GetCitas_DebeRetornarCitas()
        {
            // Arrange
            var context = CrearContextoEnMemoria();
            context.Citas.AddRange(
                new Cita { Id = 3004,
                PacienteId = "258963",
                MedicoId = "123456",
                Fecha = DateTime.Now.AddDays(2),
                Lugar = "Consultorio 5",
                Estado = "En proceso"
                },
                new Cita {Id = 3005,
                PacienteId = "258963",
                MedicoId = "147852",
                Fecha = DateTime.Now.AddDays(2),
                Lugar = "Consultorio 6",
                Estado = "En proceso"
                }
            );
            await context.SaveChangesAsync();

            var controller = new CitasController(context, null,null, new CitaRepository(context, null, null));

            // Act
            var result = await controller.GetCitas();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Cita>>>(result);
            var recetas = Assert.IsAssignableFrom<IEnumerable<Cita>>(okResult.Value);
            Assert.Equal(2, recetas.Count());
        }
        
        [Fact]
        public async Task GetCita_DebeRetornarCitaPorId()
        {
            // Arrange
            var context = CrearContextoEnMemoria();
            var repository = new CitaRepository(context, null, null);
            var cita = new Cita
            {
                Id = 3001,
                PacienteId = "258963",
                MedicoId = "123456",
                Fecha = DateTime.Now.AddDays(2),
                Lugar = "Consultorio 2",
                Estado = "En proceso"
            };
            context.Citas.Add(cita);
            await context.SaveChangesAsync();
            // Act
            var result = await repository.GetCita(3001);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("258963", result.PacienteId);
            Assert.Equal("123456", result.MedicoId);
        }

        [Fact]
        public async Task CrearCita_DebeAgregarUnaNuevaCita()
        {
            // Arrange
            var context = CrearContextoEnMemoria();
            var repository = new CitaRepository(context, null, null);

            var nuevaCita = new Cita
            {
                PacienteId = "456789",
                MedicoId = "123456",
                Fecha = DateTime.Now.AddDays(3),
                Lugar = "Consultorio 3",
                Estado = "Pendiente"
            };

            // Act
            var resultado = await repository.CrearCitaAsync(nuevaCita);

            // Assert
            var citaGuardada = await context.Citas.FindAsync(resultado.Id);
            Assert.NotNull(citaGuardada);
            Assert.Equal("456789", citaGuardada.PacienteId);
            Assert.Equal("123456", citaGuardada.MedicoId);
        }

        [Fact]
        public async Task EliminarCita_DebeEliminarCitaExistente()
        {
            // Arrange
            var context = CrearContextoEnMemoria();
            var repository = new CitaRepository(context, null, null);

            // Act
            var cita = await context.Citas.FindAsync(1);
            context.Citas.Remove(cita);
            await context.SaveChangesAsync();

            var citaEliminada = await context.Citas.FindAsync(1);

            // Assert
            Assert.Null(citaEliminada);
        }

        [Fact]
        public async Task ActualizarEstado_DebeActualizarElEstadoDeUnaCita()
        {
            // Arrange
            var context = CrearContextoEnMemoria();
            
             var cita = new Cita
            {
                Id = 3003,
                PacienteId = "258963",
                MedicoId = "123456",
                Fecha = DateTime.Now.AddDays(2),
                Lugar = "Consultorio 3",
                Estado = "En proceso"
            };
            context.Citas.Add(cita);
            await context.SaveChangesAsync();
            var repository = new CitaRepository(context, null, null);
            var controller = new CitasController(context, null, null, null);
            int citaId = 3003;
            var nuevoEstado = "Finalizada";

            // Act
            var citaActualizada = await controller.ActualizarEstado(citaId, nuevoEstado);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(citaActualizada);
            Assert.NotNull(citaActualizada);
            Assert.Equal(nuevoEstado, cita.Estado);
        }
    }
}
