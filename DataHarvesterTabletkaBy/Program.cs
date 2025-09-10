using System.Net;
using Common.Enums;
using DataHarvester.Strategies;
using DataHarvesterTabletkaBy.Consumers;
using DataHarvesterTabletkaBy.Strategies;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHttpClient<IDataHarvesterStrategy, DataHarvesterStrategy>()
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
builder.Services.AddScoped<IDataHarvesterStrategy, DataHarvesterStrategy>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
