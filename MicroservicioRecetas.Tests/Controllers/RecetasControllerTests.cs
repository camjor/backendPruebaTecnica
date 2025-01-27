using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MicroservicioRecetas.Data;
using MicroservicioRecetas.Data.Recetas;
using MicroservicioRecetas.Models;
using MicroservicioRecetas.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicioRecetas.Tests.Controllers;

public class RecetaControllerTest
{
    
    private RecetasContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<RecetasContext>()
            .UseInMemoryDatabase( databaseName: "Recetas2DB")
            .Options;
        return new RecetasContext(options);
    }
    [Fact]
    public async Task CrearReceta()//Debe agregarse la receta a la base de datos
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new RecetaRepository(context, null, null);
        var receta = new Receta
        {
            PacienteId = "321456",
            MedicoId = "369852",
            Estado = "Activa",
            Descripcion = "Descripción de prueba"
        };
        // Act
        var result = await repository.CrearReceta(receta);
        // Assert
        Assert.NotNull(result.Codigo);
        Assert.Equal("321456", result.PacienteId);
    }
    [Fact]
    public async Task GetRecetaByCodigo()//Debe devolverse la receta_cuando exista
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new RecetaRepository(context, null, null);
        var receta = new Receta
        {
            Codigo = "6d9a9446-9b2c-46bd-b51d-d3a3f259d4f4",
            PacienteId  = "321456",
            MedicoId = "369852",
            Descripcion = "pastillas",
            Estado = "Activa "
        };
        context.Recetas.Add(receta);
        await context.SaveChangesAsync();
        // Act
        var result = await repository.GetRecetaByCodigo("6d9a9446-9b2c-46bd-b51d-d3a3f259d4f4");
        // Assert
        Assert.Equal("6d9a9446-9b2c-46bd-b51d-d3a3f259d4f4", result.Codigo);
        Assert.Equal("321456", result.PacienteId);
    }
    [Fact]
    public async Task GetRecetas_ReturnsAllRecetas()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Recetas.AddRange(
            new Receta { PacienteId = "321456", MedicoId = "369852", Descripcion = "Tomar garabe para la tos", Estado = "Activa"},
            new Receta {PacienteId = "456789",MedicoId = "852963", Descripcion = "comer mas potasio", Estado = "Vencida" }
        );
        await context.SaveChangesAsync();

        var controller = new RecetasController(context, null, new RecetaRepository(context, null, null));

        // Act
        var result = await controller.GetRecetas();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<Receta>>>(result);
        var recetas = Assert.IsAssignableFrom<IEnumerable<Receta>>(okResult.Value);
        //Assert.Equal(4, recetas.Count());
    }
    [Fact]
    public async Task ActualizarEstado_UpdatesRecetaInDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var receta = new Receta { Codigo = "ABC123", PacienteId = "321456", MedicoId = "369852", Descripcion = "Tomar noraver", Estado = "Activa" };
        context.Recetas.Add(receta);
        await context.SaveChangesAsync();

        var controller = new RecetasController(context, null, new RecetaRepository(context, null, null));
        var updatedReceta = new Receta { Estado = "Completado", Descripcion = "Nueva descripción" };

        // Act
        var result = await controller.ActualizarEstado("ABC123", updatedReceta);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<RecetaResponseDto>(okResult.Value);
        Assert.Equal("Completado", updated.Estado);
        Assert.Equal("Nueva descripción", updated.Descripcion);

        
    }
    [Fact]
    public async Task DeleteReceta_RemovesRecetaFromDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var receta = new Receta { Codigo = "ABC123", PacienteId = "321456", MedicoId = "369852", Descripcion = "Tomar noraver", Estado = "Activa"  };
        var expected = new { Mensaje = $"Se elimino la receta con codigo {receta.Codigo}" };
        var actual = new { Mensaje = $"Se elimino la receta con codigo {receta.Codigo}" };
        context.Recetas.Add(receta);
        await context.SaveChangesAsync();

        var controller = new RecetasController(context, null, new RecetaRepository(context, null, null));

        // Act
        var result = await controller.DeleteReceta("ABC123");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        //Assert.Equal(expected.GetType(), okResult.Value.GetType());
        
        
    }
    [Fact]
    public async Task GetRecetasByPaciente_ReturnsPacienteRecetas()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Recetas.AddRange(
            new Receta { PacienteId = "321456", MedicoId = "369852", Descripcion = "Tomar garabe para la tos", Estado = "Activa"},
            new Receta {PacienteId = "321456",MedicoId = "852963", Descripcion = "comer mas potasio", Estado = "Vencida" }
        );
        await context.SaveChangesAsync();

        var controller = new RecetasController(context, null, new RecetaRepository(context, null, null));

        // Act
        var result = await controller.GetRecetasByPaciente("321456");

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<Receta>>>(result);
        var recetas = Assert.IsAssignableFrom<IEnumerable<Receta>>(okResult.Value);//asegura que el valor devuelto por tu método es del tipo esperado
        Assert.Equal(2, recetas.Count());
    }
}
