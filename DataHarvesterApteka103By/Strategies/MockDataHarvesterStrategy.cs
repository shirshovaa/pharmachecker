using Common.Contracts;
using Common.Enums;
using DataHarvester.Strategies;

namespace DataHarvesterApteka103By.Strategies
{
	public class MockDataHarvesterStrategy : IDataHarvesterStrategy
	{
		public PharmacySiteModule Module => PharmacySiteModule.Apteka103By;

		public string DrugLetterRoute => "https://apteka.103.by/lekarstva-minsk/?l=";

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
						NameTranslate = $"{letter}-NameTranslate-{i}",
						ManufacturerOriginal = $"{letter}-ManufacturerOriginal-{i}",
						ManufacturerTranslate = $"{letter}-ManufacturerTranslate-{i}",
						FormOriginal = $"{letter}-FormOriginal-{i}",
						FormTranslate = $"{letter}-FormOriginal-{i}",
						CountryOriginal = $"{letter}-FormTranslate-{i}",
						CountryTranslate = $"{letter}-CountryOriginal-{i}"
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
