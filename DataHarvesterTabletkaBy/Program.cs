using System.Net;
using Common.Enums;
using DataHarvester.Strategies;
using DataHarvesterTabletkaBy.Consumers;
using DataHarvesterTabletkaBy.Strategies;
using Logging;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureLogging("DataHarvesterTabletkaBy");

try
{
	Log.Information("Starting DataHarvesterTabletkaBy application");

	builder.Services.AddMassTransit(mt =>
	{
		mt.UsingRabbitMq((context, cfg) =>
		{
			cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
			{
				h.Username(builder.Configuration["RabbitMQ:Username"]);
				h.Password(builder.Configuration["RabbitMQ:Password"]);
			});

			cfg.ReceiveEndpoint($"process-drugs-letter-{PharmacySiteModule.TabletkaBy.ToString()}", e =>
			{
				e.Bind("process-drugs-letter", s =>
				{
					s.RoutingKey = PharmacySiteModule.TabletkaBy.ToString();
					s.ExchangeType = "direct";
				});
				e.PrefetchCount = 5;
				e.ConcurrentMessageLimit = 3;
				e.ConfigureConsumer<ProcessDrugsForLetterCommandConsumer>(context);
			});
		});

		mt.AddConsumer<ProcessDrugsForLetterCommandConsumer>();
	});

	builder.Services.AddHttpClient<IDataHarvesterStrategy, MockDataHarvesterStrategy>()
		.ConfigurePrimaryHttpMessageHandler(() =>
		{
			var handler = new HttpClientHandler
			{
				UseCookies = true,
				CookieContainer = new CookieContainer()
			};

			handler.CookieContainer.Add(new Uri("https://tabletka.by"),
				new Cookie("lim-result", "100000"));
			handler.CookieContainer.Add(new Uri("https://tabletka.by"),
				new Cookie("regionId", "1001"));

			return handler;
		});

	builder.Services.AddScoped<ProcessDrugsForLetterCommandConsumer>();
	builder.Services.AddScoped<IDataHarvesterStrategy, MockDataHarvesterStrategy>();

	var app = builder.Build();

	app.UseHttpsRedirection();

	Log.Information("DataHarvesterTabletkaBy application started successfully");

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application DataHarvesterTabletkaBy terminated unexpectedly");
}
finally
{
	await Log.CloseAndFlushAsync();
}
