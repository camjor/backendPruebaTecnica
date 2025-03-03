using MicroservicioPersonas.Models;

namespace MicroservicioPersonas.Data.Tareas;
public interface ITareaRepository
{
    Task<bool> SaveChanges();

    Task<IEnumerable<Tarea>> GetAllTareas();

    Task<Tarea> GetTareaById(int id);

    Task CreateTarea(Tarea tareas);

    Task DeleteTarea(int id);
}