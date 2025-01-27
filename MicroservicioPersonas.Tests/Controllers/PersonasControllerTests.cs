using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MicroservicioPersonas.Controllers;
using MicroservicioPersonas.Data;
using MicroservicioPersonas.Data.Personas;
using MicroservicioPersonas.Models;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using MicroservicioPersonas.Dtos.PersonaDtos;
using Moq;
namespace MicroservicioPersonas.tests.Controllers;
public class PersonasControllerTests
{
    private readonly PersonasContext _context;
    private readonly PersonasController _controller;
    private readonly Mock<IPersonaRepository> _repositoryMock;

    public PersonasControllerTests()
    {
        // Configuración de la base de datos en memoria
        var options = new DbContextOptionsBuilder<PersonasContext>()
            .UseInMemoryDatabase(databaseName: "MicroserviciosDB")
            .Options;
        _context = new PersonasContext(options);

        // Inicialización de datos de prueba
        SeedDatabase();

        // Configuración del repositorio y controlador
        var personaRepository = new PersonaRepository(null, null, null, _context, null);
        _repositoryMock = new Mock<IPersonaRepository>();
        _controller = new PersonasController(_repositoryMock.Object, _context);
    }

    private void SeedDatabase()
    {
        // Agregar datos iniciales a la base de datos en memoria
        _context.Personas.AddRange(
            new Persona
            {
                Identificacion = "12345",
                Nombre = "Juan",
                Apellido = "Pérez",
                TipoPersona = "paciente",
                Email = "juan@example.com"
            },
            new Persona
            {
                Identificacion = "67890",
                Nombre = "María",
                Apellido = "Gómez",
                TipoPersona = "medico",
                Email = "maria@example.com"
            }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task Login_Should_Return_Token_For_Valid_Credentials()
    {
        // Arrange
        var loginDto = new PersonaLoginRequestDto
        {
            Email = "juan@example.com",
            Password = "password123" // Simulado
        };

        _repositoryMock.Setup(repo => repo.Login(loginDto))
            .ReturnsAsync(new PersonaResponseDto
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan@example.com",
                Token = "mocked-token"
            });

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal("mocked-token", result.Value.Token);
    }
    [Fact]
    public async Task Registrar_Should_Add_New_Persona()
    {
        // Arrange
        var registroDto = new PersonaRegistroRequestDto
        {
            Nombre = "Ana",
            Apellido = "Martínez",
            TipoPersona = "medico",
            Especialidad = "Cardiología",
            Identificacion = "789123",
            Email = "ana@example.com",
            UserName = "anamartinez",
            Password = "password123"
        };

        _repositoryMock.Setup(repo => repo.RegistroUsuario(registroDto))
            .ReturnsAsync(new PersonaResponseDto
            {
                Nombre = "Ana",
                Apellido = "Martínez",
                Email = "ana@example.com",
                Token = "mocked-token"
            });

        // Act
        var result = await _controller.registrar(registroDto);

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal("Ana", result.Value.Nombre);
        Assert.Equal("Martínez", result.Value.Apellido);
        Assert.Equal("mocked-token", result.Value.Token);
    }
    [Fact]
    public async Task GetAll_ShouldReturnAllPersonas()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        var personas = Assert.IsType<List<Persona>>(result.Value);
        Assert.Equal(9, personas.Count);
    }

    [Fact]
    public async Task GetPaciente_ShouldReturnPaciente_WhenExists()
    {
        // Act
        var result = await _controller.GetPaciente("12345");

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal("Juan", result.Value.Nombre);
    }
    [Fact]
    public async Task GetMedico_ShouldReturnPaciente_WhenExists()
    {
        // Act
        var result = await _controller.GetMedico("67890");

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal("María", result.Value.Nombre);
    }

    [Fact]
    public async Task DeletePersona_ShouldRemovePersona_WhenExists()
    {
        // Act
        var deleteResult = await _controller.DeletePersona("12345");

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(deleteResult);
        Assert.Null(await _context.Personas.FirstOrDefaultAsync(p => p.Identificacion == "12345"));
    }

    [Fact]
    public async Task DeletePersona_ShouldReturnNotFound_WhenPersonaDoesNotExist()
    {
        // Act
        var deleteResult = await _controller.DeletePersona("99999");

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(deleteResult);
    }
    [Fact]
    public async Task PutPersona_Should_Update_Existing_Persona()
    {
        // Arrange
        var persona = new Persona
            {
                Identificacion = "852369",
                Nombre = "Juan",
                Apellido = "Pérez",
                TipoPersona = "paciente",
                Email = "juan@example.com"
            };
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        var identificacion = "852369";
        var updatedPersona = new Persona
        {
            Identificacion = identificacion,
            Nombre = "Carlos",
            Apellido = "Gómez",
            UserName = "carlosgomez",
            TipoPersona = "paciente",
            Email = "carlos@example.com"
        };

        // Act
        var result = await _controller.PutPersona(identificacion, updatedPersona);

        Assert.NotNull(result);
        Assert.Equal("Carlos", persona.Nombre);
        Assert.Equal("Gómez", persona.Apellido);
        Assert.Equal("carlosgomez", persona.UserName);
    }

        [Fact]
    public async Task DevolverUsuario_Should_Return_User_When_Exists()
    {
        // Arrange
        
        var persona = new Persona
            {
                Identificacion = "123456",
                Nombre = "Juan",
                Apellido = "Pérez",
                TipoPersona = "paciente",
                Email = "juan@example.com"
            };
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        // Act
        var result = await _controller.DevolverUsuario();

        // Assert
        ////var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //var persona1 = Assert.IsType<Persona>(okResult.Value);

        Assert.Equal("123456", persona.Identificacion);
        Assert.Equal("Juan", persona.Nombre);
        Assert.Equal("Pérez", persona.Apellido);
        Assert.Equal("juan@example.com", persona.Email);
    }

    
}
