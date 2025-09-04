using DataAggregator.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database
{
	public class AggregatorDbContext : DbContext
	{
		public AggregatorDbContext(DbContextOptions<AggregatorDbContext> options) : base(options) { }

		public DbSet<DrugEntity> Drugs { get; set; }

		public DbSet<PharmacySiteEntity> Sites { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			DrugEntity.Setup(modelBuilder);
			PharmacySiteEntity.Setup(modelBuilder);
		}
	}
}
