using Common.Contracts;

namespace Common.Messages
{
	public record DrugsDataCollectedEvent
	{
		public Guid CorrelationId { get; init; }

		public string Letter { get; init; }

		public DateTime CollectedAt { get; init; }

		public List<DrugPharmacyPackage> Drugs { get; init; }
	}
}
