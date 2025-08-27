using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Database.Entities;

namespace Orchestrator.Database
{
	public class OrchestratorDbContext : SagaDbContext
	{
		public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options)
		: base(options) { }

		public DbSet<DrugCollectionSagaState> DrugCollectionSagaStates { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// MassTransit Outbox entities
			modelBuilder.AddInboxStateEntity();
			modelBuilder.AddOutboxMessageEntity();
			modelBuilder.AddOutboxStateEntity();
		}

		protected override IEnumerable<ISagaClassMap> Configurations
		{
			get { yield break; }
		}
	}
}
