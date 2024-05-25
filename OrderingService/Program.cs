using Microsoft.EntityFrameworkCore;
using OrderingService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    options.UseSqlite("Data Source=OrderingDb.db");
});
builder.Services.AddScoped<OrderingDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/orders", (Order order, IOrderService orderService) =>
{
    Console.WriteLine($"Order received: {order}");

    orderService.CreateOrder(order);

    return Results.Created($"/order/{order.Id}", order);
});

app.MapGet("/orders/{id}", (int id, IOrderService orderService) =>
{
    Console.WriteLine($"Fetching order with id: {id}");

    Order? order = orderService.GetOrder(id);


    return Results.Ok(order);
});

app.MapGet("/outbox", (IOrderService orderService) =>
{
    Console.WriteLine($"Fetching outbox...");

    var outbox = orderService.GetAllOutbox();


    return Results.Ok(outbox);
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

app.Run();

public record Order(int Id, string Product);