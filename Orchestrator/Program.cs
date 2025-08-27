using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Database;
using Orchestrator.Database.Entities;
using Orchestrator.Saga;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Конфигурация PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("OrchestratorDatabase");
builder.Services.AddDbContext<OrchestratorDbContext>(options =>
	options.UseNpgsql(connectionString));

// Настройка MassTransit
builder.Services.AddMassTransit(mt =>
{
	mt.AddEntityFrameworkOutbox<OrchestratorDbContext>(o =>
	{
		o.UsePostgres();
		o.UseBusOutbox();
	});

	mt.AddSagaStateMachine<DrugCollectionSaga, DrugCollectionSagaState>()
		.EntityFrameworkRepository(r =>
		{
			r.ExistingDbContext<OrchestratorDbContext>();
			r.UsePostgres();
		});

	mt.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
		{
			h.Username(builder.Configuration["RabbitMQ:Username"]);
			h.Password(builder.Configuration["RabbitMQ:Password"]);
		});

		cfg.ConfigureEndpoints(context);
	});
});

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
