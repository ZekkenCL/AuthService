using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using AuthService.Config;

public class RabbitMQPublisher
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQPublisher(IOptions<RabbitMQSettings> options)
    {
        _settings = options.Value;
    }

    public void Publish(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.User,
            Password = _settings.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _settings.Queue,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: _settings.Exchange,
                             routingKey: _settings.RoutingKey,
                             basicProperties: null,
                             body: body);

        Console.WriteLine($"Message Published: {message}");
    }
}
