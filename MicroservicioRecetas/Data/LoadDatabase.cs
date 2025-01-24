using Microsoft.AspNetCore.Identity;
using MicroservicioRecetas.Models;
using MicroservicioRecetas.Data;

namespace MicroservicioRecetas.Data;

public class LoadDatabase {

    public static async Task InsertarData(RecetasContext context, UserManager<Receta> usuarioManager)
    {
        if(!usuarioManager.Users.Any())
        {
            var receta = new Receta {
                Codigo ="R01P987456",
                PacienteId = "987456",
                MedicoId = "123456",
                
                Descripcion = "Tomar 2 pastillas de dolex cada 12 horas",
                Estado = "Activa"
            };
            

            await usuarioManager.CreateAsync(receta, "PasswordJorge123$");

        }


        
        context.SaveChanges();
    }

}