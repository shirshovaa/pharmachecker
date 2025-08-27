namespace Common.Commands
{
	public record ProcessDrugsForLetterCommand
	{
		public Guid CorrelationId { get; init; }

		public string Letter { get; init; }
	}
}
