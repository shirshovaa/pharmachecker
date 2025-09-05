using Common.Contracts;
using Common.Enums;

namespace Common.Messages
{
	public record DrugsDataForAggregationEvent
	{
		public Guid CorrelationId { get; set; }

		public string Letter { get; set; } = string.Empty;

		public PharmacySiteModule Source { get; set; }

		public DateTime CollectedAt { get; set; }

		public List<DrugPharmacyPackage> Drugs { get; set; } = new();
	}
}
