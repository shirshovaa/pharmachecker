using Common.Contracts;

namespace DataAggregator.Database.Entities
{
	public partial class PharmacySiteEntity
	{
		public PharmacySiteEntity() { }

		public PharmacySiteEntity(PharmacySiteContract contract, Guid drugId) 
		{
			Id = Guid.NewGuid();
			DrugId = drugId;
			Module = contract.Module;
			Route = contract.SiteRoute;
		}

		public void Map(PharmacySiteContract contract)
		{
			Route = contract.SiteRoute;
		}
	}
}
