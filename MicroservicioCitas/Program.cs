using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MicroservicioCitas.Data;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using MicroservicioCitas.Data.Citas;
using MicroservicioCitas.Middleware;
using MicroservicioCitas.Models;
using MicroservicioCitas.Token;

var builder = WebApplication.CreateBuilder(args);
string secretKey = $"{Guid.NewGuid()}{Guid.NewGuid()}";
builder.Services.AddDbContext<CitasContext>(opt => {
    opt.LogTo(Console.WriteLine, new [] {//se pita el query se imprime
        DbLoggerCategory.Database.Command.Name}, 
        LogLevel.Information).EnableSensitiveDataLogging();

    opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection")!);
});

// Configuración de HTTP y RabbitMQ 
builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddControllers( opt => {//solo se puede consumir por usuarios autenticados
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));

});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var builderSecurity = builder.Services.AddIdentityCore<Cita>();
var identityBuilder = new IdentityBuilder(builderSecurity.UserType, builder.Services);
identityBuilder.AddEntityFrameworkStores<CitasContext>();
identityBuilder.AddSignInManager<SignInManager<Cita>>();
builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddScoped<IJwtGenerador, JwtGenerador>();
builder.Services.AddScoped<ICitaSesion, CitaSesion>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();


var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bc2f6c58-5186-4e2c-93ca-0143b11b0c0394b6923d-53c7-4849-9d9f-a95590d588e1"));
//Console.WriteLine("secretKey"+secretKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer( opt => {
                    opt.TokenValidationParameters = new TokenValidationParameters 
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateAudience = false,
                        ValidateIssuer = false
                    };
                     opt.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            //Console.WriteLine("secretKey"+secretKey);
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                            
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully");
                            return Task.CompletedTask;
                        }
                    };

                });


builder.Services.AddCors( o => o.AddPolicy("corsapp", builder => {
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));



var app = builder.Build();

if (app.Environment.IsDevelopment()) 
{ 
    app.UseSwagger();
    app.UseSwaggerUI(); 
}
app.UseMiddleware<ManagerMiddleware>();

app.UseAuthentication();
app.UseCors("corsapp");

app.UseHttpsRedirection(); 
app.UseAuthorization();
app.MapControllers();
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

var client = new HttpClient(handler);

using (var ambiente = app.Services.CreateScope())
{
    var services = ambiente.ServiceProvider;

    try {
        var userManager = services.GetRequiredService<UserManager<Cita>>();
        var context = services.GetRequiredService<CitasContext>();
        await context.Database.MigrateAsync();
        await LoadDatabase.InsertarData(context, userManager);

    }catch(Exception e) {
        var logging = services.GetRequiredService<ILogger<Program>>();
        logging.LogError(e, "Ocurrio un error en la migracion");
    }
}



app.Run();