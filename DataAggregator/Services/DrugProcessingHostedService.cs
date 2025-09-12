using DataAggregator.Repositories.Interfaces;

namespace DataAggregator.Services
{
	public class DrugProcessingHostedService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<DrugProcessingHostedService> _logger;
		private const int BatchSize = 100;
		private const int ProcessingIntervalSeconds = 1;

		public DrugProcessingHostedService(
			IServiceProvider serviceProvider,
			ILogger<DrugProcessingHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Сервис обработки пакетов из Redis запущен");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessPendingBatchAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Ошибка в сервисе обработки пакетов");
				}

				await Task.Delay(TimeSpan.FromSeconds(ProcessingIntervalSeconds), stoppingToken);
			}
		}

		private async Task ProcessPendingBatchAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var cacheRepository = scope.ServiceProvider.GetRequiredService<IRadisCacheRepository>();

			var pendingBatchIds = await cacheRepository.GetPendingBatchIdsAsync();

			if (pendingBatchIds is null || pendingBatchIds.Count == 0)
			{
				return;
			}

			var batchId = pendingBatchIds.First();

			try
			{
				var batchData = await cacheRepository.GetBatchAsync(batchId);
				if (batchData == null)
				{
					_logger.LogWarning("Пакет {BatchId} не найден в Redis", batchId);
					await cacheRepository.RemoveBatchAsync(batchId);
					return;
				}

				var repository = scope.ServiceProvider.GetRequiredKeyedService<IDrugRepository>(batchData.Source);

				_logger.LogInformation("Обработка пакета {BatchId} от {Source}", batchId, batchData.Source);

				var chunks = batchData.Drugs.Chunk(BatchSize);

				foreach (var chunk in chunks)
				{
					if (cancellationToken.IsCancellationRequested)
						break;

					await repository.AddOrUpdateRangeAsync(chunk.ToList());
				}

				await cacheRepository.RemoveBatchAsync(batchId);
				_logger.LogInformation("Пакет {BatchId} успешно обработан и удален из Redis", batchId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке пакета {BatchId}", batchId);
			}
		}
	}
}
