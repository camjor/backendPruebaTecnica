using Microsoft.AspNetCore.Identity;
using MicroservicioCitas.Models;
using MicroservicioCitas.Data;

namespace MicroservicioCitas.Data;

public class LoadDatabase {

    public static async Task InsertarData(CitasContext context, UserManager<Cita> usuarioManager)
    {
        if(!usuarioManager.Users.Any())
        {
            var cita = new Cita {
                PacienteId = "987456",
                MedicoId = "123456",
                
                Lugar = "Bogota",
                Estado = "Pendiente"
            };
            

            await usuarioManager.CreateAsync(cita, "PasswordJorge123$");

        }


        
        context.SaveChanges();
    }

}