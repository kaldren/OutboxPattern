using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class OrderReceivedProcessor : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public OrderReceivedProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "created-orders",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Order Received Processor Service is starting.");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine("New order received {0}", message);
            Console.WriteLine("Sending notification email to the user...");
        };
        _channel.BasicConsume(queue: "created-orders",
                             autoAck: true,
                             consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Order Received Processor Service is stopping.");

        _channel.Close(200, "Goodbye");
        _connection.Close();

        return Task.CompletedTask;
    }

    private record Order(int Id, string Product);
}