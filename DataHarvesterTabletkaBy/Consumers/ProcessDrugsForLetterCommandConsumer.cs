using Common.Enums;
using DataHarvester.Consumers;

namespace DataHarvesterTabletkaBy.Consumers
{
	public class ProcessDrugsForLetterCommandConsumer : ProcessDrugsForLetterCommandConsumerBase
	{
		protected override PharmacySiteModule Module => PharmacySiteModule.TabletkaBy;

		protected override string Route => "https://tabletka.by/";

		public ProcessDrugsForLetterCommandConsumer(ILogger<ProcessDrugsForLetterCommandConsumer> logger) : base(logger)
		{
		}
	}
}
