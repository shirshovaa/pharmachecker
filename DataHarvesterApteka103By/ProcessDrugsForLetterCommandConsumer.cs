using Common.Enums;
using DataHarvester.Consumers;

namespace DataHarvesterApteka103By
{
	public class ProcessDrugsForLetterCommandConsumer : ProcessDrugsForLetterCommandConsumerBase
	{
		protected override PharmacySiteModule Module => PharmacySiteModule.Apteka103By;

		protected override string Route => "https://apteka.103.by/";

		public ProcessDrugsForLetterCommandConsumer(ILogger<ProcessDrugsForLetterCommandConsumer> logger) : base(logger)
		{
		}
	}
}
