using DataAggregator.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Database
{
	public class AggregatorDbContext : DbContext
	{
		public AggregatorDbContext(DbContextOptions<AggregatorDbContext> options) : base(options) { }

		public DbSet<DrugApteka103ByEntity> DrugsApteka103By { get; set; }

		public DbSet<DrugTabletkaByEntity> DrugsTabletkaBy { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			DrugApteka103ByEntity.Setup(modelBuilder);
			DrugTabletkaByEntity.Setup(modelBuilder);
		}
	}
}
