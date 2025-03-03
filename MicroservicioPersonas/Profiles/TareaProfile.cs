using AutoMapper;
using MicroservicioPersonas.Dtos.TareaDtos;
using MicroservicioPersonas.Models;

namespace MicroservicioPersonas.Profiles;
public class TareaProfile : Profile
{
    public TareaProfile()
    {
        CreateMap<Tarea, TareaResponseDto>().ReverseMap();
        CreateMap<TareaRequestDto, Tarea>().ReverseMap();
    }
}