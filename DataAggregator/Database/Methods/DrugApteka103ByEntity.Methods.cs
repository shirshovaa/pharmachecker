using Common.Contracts;

namespace DataAggregator.Database.Entities
{
	public partial class DrugApteka103ByEntity : DrugEntity
	{
		public DrugApteka103ByEntity() : base() { }

		public DrugApteka103ByEntity(DrugPharmacyPackage package) : base(package)
		{
			NameTranslate = package.Drug.NameTranslate;
			FormTranslate = package.Drug.FormTranslate;
			ManufacturerTranslate = package.Drug.ManufacturerTranslate;
			CountryTranslate = package.Drug.CountryTranslate;
		}

		public override void Map(DrugPharmacyPackage package)
		{
			NameOriginal = package.Drug.NameOriginal is not null ? package.Drug.NameOriginal : NameOriginal;
			NameTranslate = package.Drug.NameTranslate is not null ? package.Drug.NameTranslate : NameTranslate;
			FormOriginal = package.Drug.FormOriginal is not null ? package.Drug.FormOriginal : FormOriginal;
			FormTranslate = package.Drug.FormTranslate is not null ? package.Drug.FormTranslate : FormTranslate;
			ManufacturerOriginal = package.Drug.ManufacturerOriginal is not null ? package.Drug.ManufacturerOriginal : ManufacturerOriginal;
			ManufacturerTranslate = package.Drug.ManufacturerTranslate is not null ? package.Drug.ManufacturerTranslate : ManufacturerTranslate;
			CountryOriginal = package.Drug.CountryOriginal is not null ? package.Drug.CountryOriginal : CountryOriginal;
			CountryTranslate = package.Drug.CountryTranslate is not null ? package.Drug.CountryTranslate : CountryTranslate;
			SiteRoute = package.PharmacySite.SiteRoute is not null ? package.PharmacySite.SiteRoute : SiteRoute;
		}
	}
}
