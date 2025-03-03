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
            Identificacion = usuario.Identificacion,
            Email = usuario.Email,
            Token =  _jwtGenerador.CrearToken(usuario)
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

    
}