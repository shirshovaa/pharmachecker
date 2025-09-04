using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database.Entities
{
	public partial class DrugEntity
	{
		[Key]
		public Guid Id { get; set; }

		public string NameOriginal { get; set; }

		public string NameTranslate { get; set; }

		public string Index { get; set; }

		public string FormOriginal { get; set; }

		public string FormTranslate { get; set; }

		public string ManufacturerOriginal { get; set; }

		public string ManufacturerTranslate { get; set; }

		public string CountryOriginal { get; set; }

		public string CountryTranslate { get; set; }

		public ICollection<PharmacySiteEntity> PharmacySites { get; set; }

		public static void Setup(ModelBuilder modelBuilder)
		{
			var entity = modelBuilder.Entity<DrugEntity>();

			entity.Property(x => x.Id).IsRequired();
			entity.Property(x => x.NameOriginal).IsRequired();

			entity.HasIndex(entity => entity.Id);
			entity.HasIndex(entity => entity.NameOriginal);
		}
	}
}
