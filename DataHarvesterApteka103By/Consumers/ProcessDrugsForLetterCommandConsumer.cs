using Common.Enums;
using DataHarvester.Consumers;
using DataHarvester.Strategies;

namespace DataHarvesterApteka103By.Consumers
{
	public class ProcessDrugsForLetterCommandConsumer : ProcessDrugsForLetterCommandConsumerBase
	{
		protected override PharmacySiteModule Module => PharmacySiteModule.Apteka103By;

		protected override string Route => "https://apteka.103.by/";

		public ProcessDrugsForLetterCommandConsumer(
			ILogger<ProcessDrugsForLetterCommandConsumer> logger,
			IDataHarvesterStrategy strategy) : base(logger, strategy)
		{
		}
	}
}
