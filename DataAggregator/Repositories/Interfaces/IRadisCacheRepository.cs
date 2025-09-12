using Common.Messages;

namespace DataAggregator.Repositories.Interfaces
{
	public interface IRadisCacheRepository
	{
		Task StoreBatchAsync(Guid batchId, DrugsDataForAggregationEvent batchData);

		Task<DrugsDataForAggregationEvent?> GetBatchAsync(Guid batchId);

		Task RemoveBatchAsync(Guid batchId);

		Task<List<Guid>> GetPendingBatchIdsAsync();
	}
}
