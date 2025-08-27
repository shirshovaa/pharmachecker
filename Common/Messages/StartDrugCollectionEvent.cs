namespace Common.Messages
{
	public record StartDrugCollectionEvent
	{
		public Guid CorrelationId { get; init; }
		public string Letter { get; init; }
	}
}
