using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MicroservicioPersonas.Dtos.PersonaDtos;
using MicroservicioPersonas.Middleware;
using MicroservicioPersonas.Models;
using MicroservicioPersonas.Token;

namespace MicroservicioPersonas.Data.Personas;

public class PersonaRepository : IPersonaRepository
{
    private readonly UserManager<Persona> _userManager;
    private readonly SignInManager<Persona> _signInManager;

    private readonly IJwtGenerador _jwtGenerador;

    private readonly PersonasContext _contexto;

    private readonly IUsuarioSesion _usuarioSesion;

    public PersonaRepository(
        UserManager<Persona> userManager,
        SignInManager<Persona> signInManager,
        IJwtGenerador jwtGenerador,
        PersonasContext contexto,
        IUsuarioSesion usuarioSesion
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerador = jwtGenerador;
        _contexto = contexto;
        _usuarioSesion = usuarioSesion;
    }

    private PersonaResponseDto TransformerUserToUserDto(Persona usuario)
    {
        return new PersonaResponseDto {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            TipoPersona = usuario.TipoPersona,
            Especialidad = usuario.Especialidad,
            Identificacion = usuario.Identificacion,
            Email = usuario.Email,
            Token =  _jwtGenerador.CrearToken(usuario)
        };
    }
    private PersonaResponseDto TransformerUserToPacienteDto(Persona usuario)
    {
        return new PersonaResponseDto {
           
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            TipoPersona = usuario.TipoPersona,
            Identificacion = usuario.Identificacion,
            Email = usuario.Email,
        };
    }
    private PersonaResponseDto TransformerUserToMedicoDto(Persona usuario)
    {
        return new PersonaResponseDto {
           
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            TipoPersona = usuario.TipoPersona,
            Especialidad = usuario.Especialidad,
            Identificacion = usuario.Identificacion,
            Email = usuario.Email,
        };
    }

    public async Task<PersonaResponseDto> GetUsuario()
    {
        var usuario = await _userManager.FindByNameAsync(_usuarioSesion.ObtenerUsuarioSesion());
        if(usuario is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.Unauthorized, 
                new  {mensaje = "El usuario del token no existe en la base de datos"}
            );
        }
        return TransformerUserToUserDto(usuario!);
    }

    public async Task<PersonaResponseDto> Login(PersonaLoginRequestDto request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email!);
        if(usuario is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.Unauthorized, 
                new  {mensaje = "El email del usuario no existe en mi base de datos"}
            );
        }

       var resultado = await _signInManager.CheckPasswordSignInAsync(usuario!, request.Password!, false);
       if(resultado.Succeeded)
       {
            return TransformerUserToUserDto(usuario);
       }

       throw new MiddlewareException(
            HttpStatusCode.Unauthorized,
            new {mensaje = "Las credenciales son incorrectas"}
       );
       

        
    }

    public async Task<PersonaResponseDto> RegistroUsuario(PersonaRegistroRequestDto request)
    {
        var existeEmail = await _contexto.Personas.Where(x => x.Email == request.Email).AnyAsync();
        if(existeEmail){
            throw new MiddlewareException(
                HttpStatusCode.BadRequest,
                new {mensaje = "El email del usuario ya existe en la base de datos"}
            );
        }
        var existeUsername = await _contexto.Users.Where(x => x.UserName == request.UserName).AnyAsync();
        if(existeUsername){
            throw new MiddlewareException(
                HttpStatusCode.BadRequest,
                new {mensaje = "El username del usuario ya existe en la base de datos"}
            );
        }



        var usuario = new Persona {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            TipoPersona = request.TipoPersona,
            Especialidad = request.Especialidad,
            Identificacion=request.Identificacion,
            Email = request.Email,
            UserName = request.UserName,
        };

        var resultado = await _userManager.CreateAsync(usuario!, request.Password!);

        if(resultado.Succeeded)
        {
            return TransformerUserToUserDto(usuario);
        }

        throw new Exception("No se pudo registrar el usuario");

       
    }

    public async Task<PersonaResponseDto> GetPaciente(string identificacion)
    {
        // Buscar al paciente por su identificación
        var paciente = await _contexto.Personas
                                     .FirstOrDefaultAsync(p => p.Identificacion == identificacion);
        
        // Filtrar usuarios con "tipodepersona" igual a "paciente" (ignorar mayúsculas/minúsculas)
        if (paciente == null)
        {
            throw new MiddlewareException(
                HttpStatusCode.NotFound, 
                new  {mensaje = "No hay Personas con esa identificacion en la base de datos"}
            );
        }
        // Verificar el campo "tipopersona"
        if (!string.Equals(paciente.TipoPersona, "paciente", StringComparison.OrdinalIgnoreCase))
        {
             throw new MiddlewareException(
                HttpStatusCode.BadRequest, 
                new  {mensaje = "La persona no es un paciente."}
            );
        }


        return TransformerUserToPacienteDto(paciente);
        
    }
    public async Task<PersonaResponseDto> GetMedico(string identificacion)
    {
        // Buscar al medico por su identificación
        var medico = await _contexto.Personas
                                     .FirstOrDefaultAsync(p => p.Identificacion == identificacion);
        var tiposValidos = new[] { "medico", "doctor", "enfermera" };
        // Filtrar usuarios con "tipodepersona" igual a "tiposvalidos" (ignorar mayúsculas/minúsculas)
        if (medico == null)
        {
            throw new MiddlewareException(
                HttpStatusCode.NotFound, 
                new  {mensaje = "No hay Personas con esa identificacion en la base de datos"}
            );
        }
        // Verificar el campo "tipopersona"
        if (!tiposValidos.Any(tipo => string.Equals(medico.TipoPersona, tipo, StringComparison.OrdinalIgnoreCase)))
        {
             throw new MiddlewareException(
                HttpStatusCode.BadRequest, 
                new  {mensaje = "La persona no es un medico."}
            );
        }


        return TransformerUserToMedicoDto(medico);
        
    }
}