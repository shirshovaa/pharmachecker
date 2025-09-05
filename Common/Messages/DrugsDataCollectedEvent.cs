using Common.Contracts;
using Common.Enums;

namespace Common.Messages
{
	public record DrugsDataCollectedEvent
	{
		public Guid CorrelationId { get; init; }

		public string Letter { get; init; }

		public PharmacySiteModule Source { get; set; }

		public DateTime CollectedAt { get; init; }

		public List<DrugPharmacyPackage> Drugs { get; init; }
	}
}
