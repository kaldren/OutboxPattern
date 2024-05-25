using System.Text.Json;

namespace OrderingService;

public interface IOrderService
{
    Order? GetOrder(int id);
    ICollection<Outbox>? GetAllOutbox();
    Order? CreateOrder(Order order);
}

public class OrderService : IOrderService
{
    private readonly OrderingDbContext context;

    public OrderService(OrderingDbContext context)
    {
        this.context = context;
    }

    public Order? CreateOrder(Order order)
    {
        using (var transaction = context.Database.BeginTransaction())
        {
            context.Orders.Add(order);
            context.SaveChanges();

            var outbox = new Outbox
            {
                AggregateId = order.Id,
                AggregateType = "Order",
                EventType = "OrderCreated",
                EventPayload = JsonSerializer.Serialize(order),
                DateTimeOffset = DateTimeOffset.Now,
                Processed = false
            };

            context.Outbox.Add(outbox);
            context.SaveChanges();

            transaction.Commit();
        }

        return context.Orders.FirstOrDefault(o => o.Id == order.Id);
    }

    public ICollection<Outbox>? GetAllOutbox()
    {
        return context.Outbox.ToList();
    }

    public Order? GetOrder(int id)
    {
        Console.WriteLine($"Fetching order with id: {id}");

        Order? order = context.Orders.FirstOrDefault<Order>(GetOrder => GetOrder.Id == id);

        return order;
    }
}
