using System.Net;

namespace MicroservicioRecetas.Middleware;
public class MiddlewareException: Exception//es una libreria propia de .net
{
    public HttpStatusCode Codigo{get;set;}
    public object? Errores { get; set; }
    public MiddlewareException(HttpStatusCode codigo, object? errores = null){
        Codigo = codigo;
        Errores = errores;
    }
}