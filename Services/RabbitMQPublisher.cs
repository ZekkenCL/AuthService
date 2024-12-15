using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using AuthService.Config;

namespace AuthServiceNamespace.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQSettings _settings;

        public RabbitMQPublisher(IOptions<RabbitMQSettings> options)
        {
            _settings = options.Value;
        }

        public void Publish(string message, string queueName, string routingKey)
        {
            // Configuración de la conexión a RabbitMQ
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.User,
                Password = _settings.Password
            };

            // Crear la conexión y el canal
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declarar la cola (asegura que exista)
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Declarar el intercambio (si es necesario)
            channel.ExchangeDeclare(exchange: _settings.Exchange, type: ExchangeType.Direct, durable: true);

            // Vincular la cola con la clave de enrutamiento
            channel.QueueBind(queue: queueName, exchange: _settings.Exchange, routingKey: routingKey);

            // Serializar el mensaje en bytes
            var body = Encoding.UTF8.GetBytes(message);

            // Publicar el mensaje
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // Mensajes persistentes en la cola

            channel.BasicPublish(exchange: _settings.Exchange,
                                 routingKey: routingKey,
                                 basicProperties: properties,
                                 body: body);

            Console.WriteLine($"Message published to RabbitMQ: Queue='{queueName}', RoutingKey='{routingKey}', Message='{message}'");
        }
    }
}
