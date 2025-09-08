using Common.Enums;
using DataHarvester.Consumers;
using DataHarvester.Strategies;

namespace DataHarvesterTabletkaBy.Consumers
{
	public class ProcessDrugsForLetterCommandConsumer : ProcessDrugsForLetterCommandConsumerBase
	{
		protected override PharmacySiteModule Module => PharmacySiteModule.TabletkaBy;

		protected override string Route => "https://tabletka.by/";

		public ProcessDrugsForLetterCommandConsumer(
			ILogger<ProcessDrugsForLetterCommandConsumer> logger,
			IDataHarvesterStrategy strategy) : base(logger, strategy)
		{
		}
	}
}
