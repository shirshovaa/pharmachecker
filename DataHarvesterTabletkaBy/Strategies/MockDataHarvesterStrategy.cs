using Common.Contracts;
using Common.Enums;
using DataHarvester.Strategies;

namespace DataHarvesterTabletkaBy.Strategies
{
	public class MockDataHarvesterStrategy : IDataHarvesterStrategy
	{
		public PharmacySiteModule Module => PharmacySiteModule.TabletkaBy;

		public string DrugLetterRoute => "https://tabletka.by/drugs/?search=";

		public async Task<ICollection<DrugPharmacyPackage>> GetDrugsByLetterAsync(string letter)
		{
			var result = new List<DrugPharmacyPackage>();

			for (int i = 1; i <= 1000; i++)
			{
				result.Add(new DrugPharmacyPackage()
				{
					Drug = new()
					{
						NameOriginal = $"{letter}-NameOriginal-{i}",
						ManufacturerOriginal = $"{letter}-ManufacturerOriginal-{i}",
						FormOriginal = $"{letter}-FormOriginal-{i}",
						CountryOriginal = $"{letter}-FormTranslate-{i}",
						Index = i.ToString(),
					},
					PharmacySite = new()
					{
						Module = Module,
						SiteRoute = DrugLetterRoute,
					}
				});
			}

			return result;
		}
	}
}
