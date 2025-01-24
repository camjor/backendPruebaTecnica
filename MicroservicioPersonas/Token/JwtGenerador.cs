using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MicroservicioPersonas.Models;
using Microsoft.IdentityModel.Tokens;

namespace MicroservicioPersonas.Token;

public class JwtGenerador : IJwtGenerador
{
    public string CrearToken(Persona usuario)
    {
        // Generar palabra secreta usando GUID (64 bytes mínimo)
        string secretKey = $"{Guid.NewGuid()}{Guid.NewGuid()}";
        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.NameId, usuario.UserName!),
            new Claim("userId", usuario.Id),
            new Claim("email", usuario.Email!)

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bc2f6c58-5186-4e2c-93ca-0143b11b0c0394b6923d-53c7-4849-9d9f-a95590d588e1"));
        //Console.WriteLine("secretKey"+secretKey);
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescripcion = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(30),
            SigningCredentials = credenciales
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescripcion);
        return tokenHandler.WriteToken(token);
    }
}