using Common.Messages;
using MassTransit;
using Orchestrator.Database;

namespace Orchestrator.Services
{
	public class DrugPharmacySearchHostedService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<DrugPharmacySearchHostedService> _logger;

		public DrugPharmacySearchHostedService(
			IServiceProvider serviceProvider,
			ILogger<DrugPharmacySearchHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
				var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

				_logger.LogInformation("Отправка сообщений с буквами алфавита...");

				// Отдельная транзакция только для Outbox сообщений
				for (int i = 0; i < 26; i++)
				{
					var letter = (char)('A' + i);
					var message = new StartDrugCollectionEvent
					{
						CorrelationId = Guid.NewGuid(),
						Letter = letter.ToString()
					};

					// Каждое сообщение в отдельной мини-транзакции
					await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
					await publishEndpoint.Publish(message, stoppingToken);
					await dbContext.SaveChangesAsync(stoppingToken);
					await transaction.CommitAsync(stoppingToken);

					_logger.LogInformation("Буква {Letter} опубликована", letter);
					await Task.Delay(500, stoppingToken); // Небольшая задержка чтобы не спамить запросами
				}

				_logger.LogInformation("Все 26 букв отправлены в Outbox");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при инициализации алфавита");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}
		}
	}
}
