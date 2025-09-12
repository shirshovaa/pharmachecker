using System.Text.Json;
using Common.Messages;
using DataAggregator.Repositories.Interfaces;
using StackExchange.Redis;

namespace DataAggregator.Repositories
{
	public class RadisCacheRepository : IRadisCacheRepository
	{
		private readonly IDatabase _database;
		private const string BatchKeyPrefix = "drugs_batch:";
		private const string PendingBatchesKey = "pending_batches";

		public RadisCacheRepository(IConnectionMultiplexer redis)
		{
			_database = redis.GetDatabase();
		}

		public async Task StoreBatchAsync(Guid batchId, DrugsDataForAggregationEvent batchData)
		{
			var key = GetBatchKey(batchId);
			var serializedData = JsonSerializer.Serialize(batchData);

			// Сохраняем пакет данных
			await _database.StringSetAsync(key, serializedData, TimeSpan.FromHours(24));

			// Добавляем ID пакета в список ожидающих обработки
			await _database.SetAddAsync(PendingBatchesKey, batchId.ToString());
		}

		public async Task<DrugsDataForAggregationEvent?> GetBatchAsync(Guid batchId)
		{
			var key = GetBatchKey(batchId);
			var serializedData = await _database.StringGetAsync(key);

			if (serializedData.IsNullOrEmpty)
				return null;

			return JsonSerializer.Deserialize<DrugsDataForAggregationEvent>(serializedData!);
		}

		public async Task RemoveBatchAsync(Guid batchId)
		{
			var key = GetBatchKey(batchId);

			// Удаляем пакет данных
			await _database.KeyDeleteAsync(key);

			// Удаляем ID из списка ожидающих обработки
			await _database.SetRemoveAsync(PendingBatchesKey, batchId.ToString());
		}

		public async Task<List<Guid>> GetPendingBatchIdsAsync()
		{
			var batchIds = await _database.SetMembersAsync(PendingBatchesKey);
			return batchIds
				.Select(id => Guid.Parse(id!))
				.ToList();
		}

		private static string GetBatchKey(Guid batchId) => $"{BatchKeyPrefix}{batchId}";
	}
}
