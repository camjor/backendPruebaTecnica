using Microsoft.AspNetCore.Identity;
using MicroservicioPersonas.Models;
using MicroservicioPersonas.Data;

namespace MicroservicioPersonas.Data;

public class LoadDatabase {

    public static async Task InsertarData(PersonasContext context, UserManager<Persona> usuarioManager)
    {
        if(!usuarioManager.Users.Any())
        {
            var usuario = new Persona {
                Nombre = "Jorge",
                Apellido = "Fonseca",
                Identificacion = "98142545",
                Email="carlos.vaez@gmail.com"
            };
            

            await usuarioManager.CreateAsync(usuario, "PasswordJorge123$");

        }


        
        context.SaveChanges();
    }

}