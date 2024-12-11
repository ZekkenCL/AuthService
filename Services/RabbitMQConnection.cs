using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using AuthService.Config;

namespace CarreraService.Services
{
    public class RabbitMQConnection
    {
        private readonly RabbitMQSettings _settings;
        private readonly IConnection _connection;

        public RabbitMQConnection(IOptions<RabbitMQSettings> options)
        {
            _settings = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.User,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
        }

        public IModel CreateChannel()
        {
            return _connection.CreateModel();
        }
    }
}
