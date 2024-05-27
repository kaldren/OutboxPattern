using Microsoft.EntityFrameworkCore;

namespace OrderingService.Orders;

public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderOutbox> Outbox { get; set; }

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<OrderOutbox>().HasKey(o => o.Id);
    }
}
