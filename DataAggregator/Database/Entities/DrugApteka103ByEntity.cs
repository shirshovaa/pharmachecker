using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database.Entities
{
	public partial class DrugApteka103ByEntity : DrugEntity
	{
		public string NameTranslate { get; set; }

		public string FormTranslate { get; set; }

		public string ManufacturerTranslate { get; set; }

		public string CountryTranslate { get; set; }

		public static void Setup(ModelBuilder modelBuilder)
		{
			var entity = modelBuilder.Entity<DrugApteka103ByEntity>();

			entity.Property(x => x.Id).IsRequired();
			entity.Property(x => x.NameOriginal).IsRequired();
			entity.Property(x => x.SiteRoute).IsRequired();

			entity.HasIndex(entity => entity.Id);
			entity.HasIndex(entity => entity.NameOriginal);
			entity.HasIndex(entity => entity.SiteRoute);
		}
	}
}
