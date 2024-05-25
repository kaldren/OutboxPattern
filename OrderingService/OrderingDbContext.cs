using Microsoft.EntityFrameworkCore;

namespace OrderingService;

public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Outbox> Outbox { get; set; }

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<Outbox>().HasKey(o => o.Id);
    }
}
