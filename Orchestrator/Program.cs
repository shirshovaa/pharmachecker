using Common.Commands;
using Common.Messages;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.QuartzIntegration;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Database;
using Orchestrator.Database.Entities;
using Orchestrator.Saga;
using Orchestrator.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("OrchestratorDatabase");
builder.Services.AddDbContext<OrchestratorDbContext>(_ => _.UseNpgsql(connectionString));

// Регистрация Quartz
builder.Services.AddQuartz();

// Регистрация хоста Quartz
builder.Services.AddQuartzHostedService(_ =>
{
	// Ожидать завершения заданий при остановке
	_.WaitForJobsToComplete = true;
});

// Настройка MassTransit
builder.Services.AddMassTransit(x =>
{
	// Добавление Quartz для отложенных сообщений
	x.AddQuartzConsumers();

	// Добавление конфигурации для использования EF в Outbox
	x.AddConfigureEndpointsCallback((context, name, cfg) =>
	{
		cfg.UseEntityFrameworkOutbox<OrchestratorDbContext>(context);
	});

	// Конфигурация Саги
	x.AddSagaStateMachine<DrugCollectionSaga, DrugCollectionSagaState>()
		.EntityFrameworkRepository(r =>
		{
			r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
			r.ExistingDbContext<OrchestratorDbContext>();
			r.LockStatementProvider = new PostgresLockStatementProvider();
		});

	// Настройка Entity Framework Outbox
	x.AddEntityFrameworkOutbox<OrchestratorDbContext>(o =>
	{
		// Опционально: настройка Query Delay
		o.QueryDelay = TimeSpan.FromSeconds(5);
		o.QueryTimeout = TimeSpan.FromSeconds(2);

		o.UsePostgres().UseBusOutbox();

		// Опционально: настройка количества сообщений
		o.QueryMessageLimit = 100;
	});

	// Настройка брокера RabbitMQ
	x.UsingRabbitMq((context, cfg) =>
	{
		// Настройка переотправки сообщений
		cfg.UseMessageRetry(r =>
		{
			r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
		});

		// Добавления очереди сообщений
		cfg.UseMessageScheduler(new Uri("queue:scheduler"));

		// Настройка Quartz endpoint
		cfg.ReceiveEndpoint("scheduler", e =>
		{
			e.UseInMemoryOutbox(context);
			e.ConfigureConsumer<ScheduleMessageConsumer>(context);
			e.ConfigureConsumer<CancelScheduledMessageConsumer>(context);
		});

		// Настройка очереди обработки лекарств
		cfg.Message<ProcessDrugsForLetterCommand>(x =>
		{
			x.SetEntityName("process-drugs-letter");
		});

		cfg.Publish<ProcessDrugsForLetterCommand>(x =>
		{
			x.ExchangeType = "direct";
		});

		// Подключение RabbitMQ
		cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
		{
			h.Username(builder.Configuration["RabbitMQ:Username"]);
			h.Password(builder.Configuration["RabbitMQ:Password"]);
		});

		cfg.ConfigureEndpoints(context);
	});
});

builder.Services.AddHostedService<DrugPharmacySearchHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Миграция базы данных при запуске
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
	dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.Run();
