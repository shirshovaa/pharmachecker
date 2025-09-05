using Common.Contracts;

namespace DataAggregator.Database.Entities
{
	public partial class DrugEntity
	{
		public DrugEntity() { }

		public DrugEntity(DrugContract contract)
		{
			Id = Guid.NewGuid();
			NameOriginal = contract.NameOriginal;
			NameTranslate = contract.NameTranslate;
			Index = contract.Index;
			FormOriginal = contract.FormOriginal;
			FormTranslate = contract.FormTranslate;
			ManufacturerOriginal = contract.ManufacturerOriginal;
			ManufacturerTranslate = contract.ManufacturerTranslate;
			CountryOriginal = contract.CountryOriginal;
			CountryTranslate = contract.CountryTranslate;
			PharmacySites = new List<PharmacySiteEntity>();
		}

		public void Map(DrugContract contract)
		{
			NameTranslate = contract.NameTranslate is not null ? contract.NameTranslate : NameTranslate;
			Index = contract.Index is not null ? contract.Index : Index;
			FormOriginal = contract.FormOriginal is not null ? contract.FormOriginal : FormOriginal;
			FormTranslate = contract.FormTranslate is not null ? contract.FormTranslate : FormTranslate;
			ManufacturerOriginal = contract.ManufacturerOriginal is not null ? contract.ManufacturerOriginal : ManufacturerOriginal;
			ManufacturerTranslate = contract.ManufacturerTranslate is not null ? contract.ManufacturerTranslate : ManufacturerTranslate;
			CountryOriginal = contract.CountryOriginal is not null ? contract.CountryOriginal : CountryOriginal;
			CountryTranslate = contract.CountryTranslate is not null ? contract.CountryTranslate : CountryTranslate;
		}
	}
}
