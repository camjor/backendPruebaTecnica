using Microsoft.EntityFrameworkCore;
using MicroservicioRecetas.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MicroservicioRecetas.Middleware;
using Microsoft.AspNetCore.Identity;
using MicroservicioRecetas.Models;
using Microsoft.AspNetCore.Authentication;
using MicroservicioRecetas.Data.Recetas;

var builder = WebApplication.CreateBuilder(args);
string secretKey = $"{Guid.NewGuid()}{Guid.NewGuid()}";
// Configuración de la base de datos
builder.Services.AddDbContext<RecetasContext>(opt => {
    opt.LogTo(Console.WriteLine, new [] {//se pita el query se imprime
        DbLoggerCategory.Database.Command.Name}, 
        LogLevel.Information).EnableSensitiveDataLogging();

    opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection")!);
});
// Configuración de HTTP y RabbitMQ 
builder.Services.AddHttpClient();
builder.Services.AddHostedService<RabbitMQListener>();

// Configuración de servicios básicos
builder.Services.AddControllers( opt => {//solo se puede consumir por usuarios autenticados
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var builderSecurity = builder.Services.AddIdentityCore<Receta>();
var identityBuilder = new IdentityBuilder(builderSecurity.UserType, builder.Services);
identityBuilder.AddEntityFrameworkStores<RecetasContext>();
identityBuilder.AddSignInManager<SignInManager<Receta>>();
builder.Services.AddSingleton<ISystemClock, SystemClock>();
//builder.Services.AddScoped<IJwtGenerador, JwtGenerador>();
//builder.Services.AddScoped<IRecetaSesion, RecetaSesion>();
builder.Services.AddScoped<IRecetaRepository, RecetaRepository>();

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
        var userManager = services.GetRequiredService<UserManager<Receta>>();
        var context = services.GetRequiredService<RecetasContext>();
        await context.Database.MigrateAsync();
        await LoadDatabase.InsertarData(context, userManager);

    }catch(Exception e) {
        var logging = services.GetRequiredService<ILogger<Program>>();
        logging.LogError(e, "Ocurrio un error en la migracion");
    }
}

app.Run();
