using System.ComponentModel.DataAnnotations;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database.Entities
{
	public partial class PharmacySiteEntity
	{
		[Key]
		public Guid Id { get; set; }

		public Guid DrugId { get; set; }

		public DrugEntity Drug { get; set; }

		public PharmacySiteModule Module { get; set; }

		public string Route { get; set; }

		public static void Setup(ModelBuilder modelBuilder)
		{
			modelBuilder.HasPostgresEnum<PharmacySiteModule>();

			var entity = modelBuilder.Entity<PharmacySiteEntity>();

			entity.Property(x => x.Id).IsRequired();
			entity.Property(x => x.DrugId).IsRequired();
			entity.Property(x => x.Module).IsRequired();
			entity.Property(x => x.Route).IsRequired();

			entity.HasIndex(entity => entity.Id);
			entity.HasIndex(entity => entity.DrugId);

			entity
				.HasOne(x => x.Drug)
				.WithMany(x => x.PharmacySites)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
