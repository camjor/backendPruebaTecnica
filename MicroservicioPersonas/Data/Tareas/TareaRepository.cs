using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MicroservicioPersonas.Middleware;
using MicroservicioPersonas.Models;
using MicroservicioPersonas.Token;

namespace MicroservicioPersonas.Data.Tareas;

public class TareaRepository : ITareaRepository
{
    private readonly PersonasContext _contexto;
    private readonly IUsuarioSesion _usuarioSesion;
    private readonly UserManager<Persona> _userManager;

    public TareaRepository(
        PersonasContext contexto,
        IUsuarioSesion sesion,
        UserManager<Persona> userManager
    )
    {
        _contexto = contexto;
        _usuarioSesion = sesion;
        _userManager = userManager;
    }

    public async Task CreateTarea(Tarea tareas)
    {
        var usuario = await _userManager.FindByNameAsync(_usuarioSesion.ObtenerUsuarioSesion());
        if (usuario is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.Unauthorized,
                new { mensaje = "El usuario no es válido para hacer esta inserción" }
            );
        }

        if (tareas is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.BadRequest,
                new { mensaje = "Los datos de la tarea son incorrectos" }
            );
        }

        //tareas.TareaId = Guid.Parse(usuario.Id); // Asigna el ID del usuario a la tarea

        await _contexto.Tareas!.AddAsync(tareas); // Corrige el objeto insertado
        await _contexto.SaveChangesAsync(); // Guarda los cambios en la base de datos
    }

    public async Task DeleteTarea(int id)
    {
        var Tarea = await _contexto.Tareas!
                            .FirstOrDefaultAsync(x => x.Id == id);

        _contexto.Tareas!.Remove(Tarea!);
    }

    public async Task<IEnumerable<Tarea>> GetAllTareas()
    {
         return await _contexto.Tareas!.ToListAsync();
    }

    public async Task<Tarea> GetTareaById(int id)
    {
         return await _contexto.Tareas!.FirstOrDefaultAsync(x => x.Id == id)!;
    }

    public async Task<bool> SaveChanges()
    {
         return (  (await _contexto.SaveChangesAsync())   >= 0);
    }
}
