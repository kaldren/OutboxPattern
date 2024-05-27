using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class OrderReceivedProcessor : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderReceivedProcessor> _logger;
    private Timer _timer;
    private IConnection _connection;
    private IModel _channel;

    public OrderReceivedProcessor(IServiceProvider serviceProvider, ILogger<OrderReceivedProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order Received Processor Service is starting.");
        _timer = new Timer(TryConnect, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    private void TryConnect(object state)
    {
        if (_connection != null && _connection.IsOpen)
        {
            return; // Already connected
        }

        _logger.LogInformation("Attempting to connect to RabbitMQ...");

        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "created-orders",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("New order received: {0}", message);
                _logger.LogInformation("Sending notification email to the user...");
            };

            _channel.BasicConsume(queue: "created-orders",
                                 autoAck: true,
                                 consumer: consumer);

            _logger.LogInformation("Successfully connected to RabbitMQ.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ.");
            _connection?.Dispose();
            _channel?.Dispose();
            _connection = null;
            _channel = null;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order Received Processor Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);

        _channel?.Close(200, "Goodbye");
        _connection?.Close();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
