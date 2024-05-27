using RabbitMQ.Client;
using System.Text;

namespace OrderingService.Orders;

public class OrderOutboxProcessor : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public OrderOutboxProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Outbox Processor Service is starting.");

        // Set up the timer to call the ProcessOutbox method every 20 seconds
        _timer = new Timer(ProcessOutbox, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Outbox Processor Service is stopping.");

        // Stop the timer when the service is stopping
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void ProcessOutbox(object state)
    {
        //Console.WriteLine("Processing Outbox at: {time}", DateTimeOffset.Now);

        using (var scope = _serviceProvider.CreateScope())
        {
            var _context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

            try
            {
                // Create RabbitMQ queue called created-orders
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "created-orders",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Process outbox
                var entries = _context.Outbox.Where(x => x.Processed == false).Take(10).ToList();

                foreach (var entry in entries)
                {
                    // push to created-orders queue
                    var body = Encoding.UTF8.GetBytes(entry.EventPayload);
                    channel.BasicPublish(exchange: "",
                                         routingKey: "created-orders",
                                         basicProperties: null,
                                         body: body);

                    // mark as processed
                    entry.Processed = true;
                }

                _context.Outbox.UpdateRange(entries);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while processing the outbox: " + ex.Message);
            }
        }
    }
}
