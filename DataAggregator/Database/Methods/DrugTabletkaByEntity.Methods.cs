using Common.Contracts;

namespace DataAggregator.Database.Entities
{
	public partial class DrugTabletkaByEntity : DrugEntity
	{
		public DrugTabletkaByEntity() : base() { }

		public DrugTabletkaByEntity(DrugPharmacyPackage package) : base(package)
		{
			Index = package.Drug.Index;
		}

		public override void Map(DrugPharmacyPackage package)
		{
			NameOriginal = package.Drug.NameOriginal is not null ? package.Drug.NameOriginal : NameOriginal;
			FormOriginal = package.Drug.FormOriginal is not null ? package.Drug.FormOriginal : FormOriginal;
			ManufacturerOriginal = package.Drug.ManufacturerOriginal is not null ? package.Drug.ManufacturerOriginal : ManufacturerOriginal;
			CountryOriginal = package.Drug.CountryOriginal is not null ? package.Drug.CountryOriginal : CountryOriginal;
			Index = package.Drug.Index is not null ? package.Drug.Index : Index;
			SiteRoute = package.PharmacySite.SiteRoute is not null ? package.PharmacySite.SiteRoute : SiteRoute;
		}
	}
}
