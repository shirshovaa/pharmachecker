using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database.Entities
{
	public partial class DrugTabletkaByEntity : DrugEntity
	{
		public string Index { get; set; }

		public static void Setup(ModelBuilder modelBuilder)
		{
			var entity = modelBuilder.Entity<DrugTabletkaByEntity>();

			entity.Property(x => x.Id).IsRequired();
			entity.Property(x => x.NameOriginal).IsRequired();
			entity.Property(x => x.SiteRoute).IsRequired();

			entity.HasIndex(entity => entity.Id);
			entity.HasIndex(entity => entity.NameOriginal);
			entity.HasIndex(entity => entity.SiteRoute);
		}
	}
}
