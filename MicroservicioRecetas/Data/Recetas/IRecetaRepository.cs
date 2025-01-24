using MicroservicioRecetas.Models;

namespace MicroservicioRecetas.Data.Recetas;

public interface IRecetaRepository
{
    Task<RecetaResponseDto> CrearReceta(Receta receta);
    Task<RecetaResponseDto> GetRecetaByCodigo(string codigo);
    Task<RecetaResponseDto> ActualizarEstado(string codigo, Receta receta);
    
    
}