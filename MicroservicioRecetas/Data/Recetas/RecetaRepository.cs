using System.Net;
using MicroservicioRecetas.Data.Recetas;
using MicroservicioRecetas.Middleware;
using MicroservicioRecetas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroservicioRecetas.Data.Recetas;

class RecetaRepository : IRecetaRepository
{
    private readonly RecetasContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RecetaRepository(
        RecetasContext context, 
        HttpClient httpClient, 
        IConfiguration configuration
    ){
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private RecetaResponseDto TransformerRecetaToRecetaDto(Receta receta)
    {
        return new RecetaResponseDto {
           
            PacienteId = receta.PacienteId,
            MedicoId = receta.MedicoId,
            Codigo = receta.Codigo,
            Estado = receta.Estado,
            Id = receta.Id,
            Descripcion= receta.Descripcion
        };
    }



    public async Task<RecetaResponseDto> CrearReceta(Receta receta)
    {
        receta.Codigo = Guid.NewGuid().ToString(); // Generar código único
        _context.Recetas.Add(receta);
        await _context.SaveChangesAsync();
        return TransformerRecetaToRecetaDto(receta);
    }

    public async Task<RecetaResponseDto> GetRecetaByCodigo(string codigo)
    {
        var receta = await _context.Recetas.FirstOrDefaultAsync(r => r.Codigo == codigo);
        if (receta == null)
        {
            throw new MiddlewareException(
                HttpStatusCode.NotFound, 
                new  {mensaje = "No hay Receta con esa codigo en la base de datos"}
            );
        }
        return TransformerRecetaToRecetaDto(receta);
    }

    public async Task<RecetaResponseDto> ActualizarEstado(string codigo, Receta receta )
    {
        var existingReceta = await _context.Recetas.FirstOrDefaultAsync(r => r.Codigo == codigo);
        if (receta == null)
        {
            throw new MiddlewareException(
                HttpStatusCode.NotFound, 
                new  {mensaje = "No hay Receta con esa codigo en la base de datos"}
            );
        }

        existingReceta.Estado = receta.Estado;
        existingReceta.Descripcion=receta.Descripcion;
        await _context.SaveChangesAsync();
        return TransformerRecetaToRecetaDto(existingReceta);
    }


}