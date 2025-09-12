using Common.Enums;
using DataAggregator.Consumers;
using DataAggregator.Database;
using DataAggregator.Repositories;
using DataAggregator.Repositories.Interfaces;
using DataAggregator.Services;
using Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging("Aggregator");

try
{
	Log.Information("Starting Aggregator application");

	var connectionString = builder.Configuration.GetConnectionString("AggregatorDatabase");
	builder.Services.AddDbContext<AggregatorDbContext>(_ => _.UseNpgsql(connectionString));

	var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
	builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
	builder.Services.AddScoped<IRadisCacheRepository, RadisCacheRepository>();

	builder.Services.AddKeyedScoped<IDrugRepository, DrugApteka103ByRepository>(PharmacySiteModule.Apteka103By);
	builder.Services.AddKeyedScoped<IDrugRepository, DrugTabletkaByRepository>(PharmacySiteModule.TabletkaBy);

	builder.Services.AddHostedService<DrugProcessingHostedService>();

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

	Log.Information("Aggregator application started successfully");

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application Aggregator terminated unexpectedly");
}
finally
{
	await Log.CloseAndFlushAsync();
}
