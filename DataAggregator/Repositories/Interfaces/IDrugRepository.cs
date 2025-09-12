using Common.Contracts;
using DataAggregator.Database.Entities;

namespace DataAggregator.Repositories.Interfaces
{
	public interface IDrugRepository<TEntity> : IDrugRepository where TEntity : DrugEntity 
	{
		Task<TEntity> GetByNameAsync(string nameOriginal);
	}

	public interface IDrugRepository
	{
		Task AddOrUpdateAsync(DrugPharmacyPackage package);

		Task AddOrUpdateRangeAsync(IEnumerable<DrugPharmacyPackage> package);
	}
}
