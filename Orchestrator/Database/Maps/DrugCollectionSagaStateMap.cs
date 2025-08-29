using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestrator.Database.Entities;

namespace Orchestrator.Database.Maps
{
	public class DrugCollectionSagaStateMap : IEntityTypeConfiguration<DrugCollectionSagaState>
	{
		public void Configure(EntityTypeBuilder<DrugCollectionSagaState> builder)
		{
			builder.ToTable("DrugCollectionSagaState");
			builder.HasKey(x => x.CorrelationId);
			builder.Property(x => x.CurrentState).HasMaxLength(64);
			builder.Property(x => x.Letter).HasMaxLength(1);
			builder.Property(x => x.Version).IsConcurrencyToken();
		}
	}
}
