using Common.Enums;
using DataHarvester.Strategies;
using DataHarvesterApteka103By.Consumers;
using DataHarvesterApteka103By.Strategies;
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

		cfg.ReceiveEndpoint($"process-drugs-letter-{PharmacySiteModule.Apteka103By.ToString()}", e =>
		{
			e.Bind("process-drugs-letter", s =>
			{
				s.RoutingKey = PharmacySiteModule.Apteka103By.ToString();
				s.ExchangeType = "direct";
			});
			e.PrefetchCount = 5;
			e.ConcurrentMessageLimit = 3;
			e.ConfigureConsumer<ProcessDrugsForLetterCommandConsumer>(context);
		});
	});

	mt.AddConsumer<ProcessDrugsForLetterCommandConsumer>();
});

builder.Services.AddHttpClient<DataHarvesterStrategy>();

builder.Services.AddScoped<ProcessDrugsForLetterCommandConsumer>();
builder.Services.AddScoped<IDataHarvesterStrategy, DataHarvesterStrategy>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
