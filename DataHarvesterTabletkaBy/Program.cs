using Common.Enums;
using DataHarvesterTabletkaBy.Consumers;
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

builder.Services.AddScoped<ProcessDrugsForLetterCommandConsumer>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
