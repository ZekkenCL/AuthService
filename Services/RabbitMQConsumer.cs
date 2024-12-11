using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using AuthService.Config;

public class RabbitMQConsumer : BackgroundService
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQConsumer(IOptions<RabbitMQSettings> options)
    {
        _settings = options.Value;
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

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _settings.Queue,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Message Consumed: {message}");
        };

        channel.BasicConsume(queue: _settings.Queue,
                             autoAck: true,
                             consumer: consumer);

        return Task.CompletedTask;
    }
}
