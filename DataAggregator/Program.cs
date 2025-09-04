using DataAggregator.Consumers;
using DataAggregator.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("AggregatorDatabase");
builder.Services.AddDbContext<AggregatorDbContext>(_ => _.UseNpgsql(connectionString));

builder.Services.AddMassTransit(mt =>
{
	mt.AddConsumer<DrugsDataAggregationConsumer>();

	mt.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
		{
			h.Username(builder.Configuration["RabbitMQ:Username"]);
			h.Password(builder.Configuration["RabbitMQ:Password"]);
		});

		cfg.ReceiveEndpoint("drugs-aggregation-queue", e =>
		{
			e.PrefetchCount = 1;
			e.ConfigureConsumer<DrugsDataAggregationConsumer>(context);
		});
	});
});

var app = builder.Build();

// Миграция базы данных при запуске
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AggregatorDbContext>();
	dbContext.Database.Migrate();
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();
