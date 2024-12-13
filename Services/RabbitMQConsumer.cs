using AuthService.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServiceNamespace.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQConsumer(
            IOptions<RabbitMQSettings> options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _settings = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.User,
                Password = _settings.Password
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _settings.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message Received: {message}");

                // Crear un ámbito manual para usar servicios Scoped
                using var scope = _serviceScopeFactory.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                // Deserializar el mensaje
                var editProfileDto = JsonSerializer.Deserialize<EditProfileDto>(message);

                if (editProfileDto != null)
                {
                    await userService.EditProfile(editProfileDto.UserId, editProfileDto);
                }
            };

            channel.BasicConsume(
                queue: _settings.Queue,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
