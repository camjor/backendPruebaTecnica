using System;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MicroservicioPersonas.Data.Tareas;
using MicroservicioPersonas.Dtos.TareaDtos;
using MicroservicioPersonas.Middleware;
using MicroservicioPersonas.Models;
using MicroservicioPersonas.Data;
using Microsoft.AspNetCore.Authorization;

namespace  MicroservicioPersonas.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TareaController: ControllerBase {

    private readonly PersonasContext _context;
    private readonly ITareaRepository _repository;
    private IMapper _mapper;
    
    public TareaController(
        ITareaRepository repository,
        IMapper mapper, PersonasContext context
    )
    {
        _mapper = mapper;
        _repository = repository;
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TareaResponseDto>>> GetTareas()
    {
        var tareas = await _repository.GetAllTareas();
        return Ok(_mapper.Map<IEnumerable<TareaResponseDto>>(tareas));
    }


    [HttpGet("{id}", Name = "GetTareaById")]
    public async Task<ActionResult<TareaResponseDto>> GetTareaById(int id)
    {
        var inmueble = await  _repository.GetTareaById(id);

        if(inmueble is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.NotFound,
                new {mensaje = $"No se encontro el tarea por este id {id}"}
            );

        }


        return Ok(_mapper.Map<TareaResponseDto>(inmueble));

    }
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<TareaResponseDto>> CreateTarea( [FromBody] TareaRequestDto tarea)
    {
        var tareaModel = _mapper.Map<Tarea>(tarea);
        await _repository.CreateTarea(tareaModel);
        await _repository.SaveChanges();

        var TareaResponse = _mapper.Map<TareaResponseDto>(tareaModel);

        return CreatedAtRoute(nameof(GetTareaById), new {TareaResponse.Id}, TareaResponse);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteInmueble(int id)
    {
        await _repository.DeleteTarea(id);
        await _repository.SaveChanges();
        return Ok();
    }

  
    
}