using Common.Contracts;
using DataAggregator.Database;
using DataAggregator.Database.Entities;

namespace DataAggregator.Repositories
{
	public class DrugApteka103ByRepository : DrugRepositoryBase<DrugApteka103ByEntity>
	{
		public DrugApteka103ByRepository(AggregatorDbContext context) : base(context)
		{
		}

		protected override DrugApteka103ByEntity CreateEntity(DrugPharmacyPackage package)
		{
			return new DrugApteka103ByEntity(package);
		}
	}
}
