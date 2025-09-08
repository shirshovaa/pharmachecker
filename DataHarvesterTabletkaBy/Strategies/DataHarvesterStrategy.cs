using Common.Contracts;
using Common.Enums;
using DataHarvester.Strategies;

namespace DataHarvesterTabletkaBy.Strategies
{
	public class DataHarvesterStrategy : IDataHarvesterStrategy
	{
		public PharmacySiteModule Module => PharmacySiteModule.TabletkaBy;

		public string DrugLetterRoute => "";

		public Task<ICollection<DrugPharmacyPackage>> GetDrugsByLetterAsync(string letter)
		{
			throw new NotImplementedException();
		}
	}
}
