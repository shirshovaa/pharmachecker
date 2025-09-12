using Common.Messages;
using DataAggregator.Repositories.Interfaces;
using MassTransit;

namespace DataAggregator.Consumers
{
	public class DrugsDataAggregationConsumer : IConsumer<DrugsDataForAggregationEvent>
	{
		private readonly IRadisCacheRepository _cacheRepository;
		private readonly ILogger<DrugsDataAggregationConsumer> _logger;

		public DrugsDataAggregationConsumer(
			IRadisCacheRepository cacheRepository,
			ILogger<DrugsDataAggregationConsumer> logger)
		{
			_cacheRepository = cacheRepository;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<DrugsDataForAggregationEvent> context)
		{
			var message = context.Message;
			var batchId = Guid.NewGuid();

			_logger.LogInformation("Получены данные от {Source} для буквы {Letter} ({Count} препаратов). BatchId: {BatchId}",
				message.Source, message.Letter, message.Drugs.Count, batchId);

			try
			{
				// Сохраняем пакет в Redis вместо немедленной обработки
				await _cacheRepository.StoreBatchAsync(batchId, message);

				_logger.LogInformation("Пакет {BatchId} сохранен в Redis для последующей обработки", batchId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении пакета {BatchId} в Redis", batchId);
				throw;
			}
		}
	}
}
