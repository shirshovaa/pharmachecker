using Common.Commands;
using Common.Contracts;
using Common.Enums;
using Common.Messages;
using DataHarvester.Strategies;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DataHarvester.Consumers
{
	public abstract class ProcessDrugsForLetterCommandConsumerBase : IConsumer<ProcessDrugsForLetterCommand>
	{
		protected abstract PharmacySiteModule Module { get; }

		protected abstract string Route { get; }

		protected readonly ILogger<ProcessDrugsForLetterCommandConsumerBase> _logger;
		protected readonly IDataHarvesterStrategy _strategy;

		public ProcessDrugsForLetterCommandConsumerBase(
			ILogger<ProcessDrugsForLetterCommandConsumerBase> logger, 
			IDataHarvesterStrategy strategy)
		{
			_logger = logger;
			_strategy = strategy;
		}

		public async Task Consume(ConsumeContext<ProcessDrugsForLetterCommand> context)
		{
			if (context.Message.Source != Module)
			{
				_logger.LogDebug("Команда для источника {ExpectedSource} пропущена текущим источником {CurrentSource}",
					context.Message.Source.ToString(), Module.ToString());
				return;
			}

			var drugs = await _strategy.GetDrugsByLetterAsync(context.Message.Letter);

			var letter = context.Message.Letter;
			_logger.LogInformation("Начинается сбор данных для буквы: {Letter}", letter);

			await Task.Delay(TimeSpan.FromSeconds(new Random().Next(2, 5)));

			_logger.LogInformation("Сбор данных для буквы {Letter} завершен. Найдено {Count} препаратов.",
				letter, drugs.Count);

			await context.RespondAsync(new DrugsDataCollectedEvent
			{
				CorrelationId = context.Message.CorrelationId,
				Letter = letter,
				Source = Module,
				CollectedAt = DateTime.UtcNow,
				Drugs = (List<DrugPharmacyPackage>)drugs
			});
		}
	}
}
