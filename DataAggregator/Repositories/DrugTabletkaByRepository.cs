using Common.Contracts;
using DataAggregator.Database;
using DataAggregator.Database.Entities;

namespace DataAggregator.Repositories
{
	public class DrugTabletkaByRepository : DrugRepositoryBase<DrugTabletkaByEntity>
	{
		public DrugTabletkaByRepository(AggregatorDbContext context) : base(context)
		{
		}

		protected override DrugTabletkaByEntity CreateEntity(DrugPharmacyPackage package)
		{
			return new DrugTabletkaByEntity(package);
		}
	}
}
