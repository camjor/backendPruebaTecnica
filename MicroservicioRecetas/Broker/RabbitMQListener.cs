using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MicroservicioRecetas.Models;
using MicroservicioRecetas.Data;

public class RabbitMQListener : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public RabbitMQListener(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { 
            HostName = _configuration["RabbitMQ:HostName"] ,
             DispatchConsumersAsync = true // Permite manejar consumidores asíncronos
            };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        // Declarar la cola
        channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName"],
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Deserializar el mensaje recibido
                var cita = JsonSerializer.Deserialize<Cita>(message);
                // Procesar el mensaje y guardar la receta
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<RecetasContext>();

                var receta = new Receta
                {
                    Codigo = Guid.NewGuid().ToString(),
                    PacienteId = cita.PacienteId,
                    MedicoId =cita.MedicoId,
                    Descripcion = "Receta generada a partir de una cita finalizada.",
                    Estado = "Activa"
                };

                context.Recetas.Add(receta);
                await context.SaveChangesAsync();

                Console.WriteLine($"Receta creada para el paciente con ID: {cita.PacienteId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando mensaje: {ex.Message}");
            }
        };

        channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"],
                             autoAck: true,
                             consumer: consumer);
        
        // Mantener el listener activo
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
