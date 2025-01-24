using System.Security.Claims;

namespace MicroservicioCitas.Token;
public class CitaSesion : ICitaSesion
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CitaSesion (IHttpContextAccessor httpContextAccessor){
        _httpContextAccessor = httpContextAccessor;
    }
    public string ObtenerCitaSesion(){
        var UserName = _httpContextAccessor.HttpContext!.User?.Claims?
                            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        
        return UserName!;
    }
}