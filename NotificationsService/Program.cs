var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<OrderReceivedProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();