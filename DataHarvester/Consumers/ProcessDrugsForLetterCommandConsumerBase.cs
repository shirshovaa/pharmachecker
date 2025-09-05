using Common.Commands;
using Common.Contracts;
using Common.Enums;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DataHarvester.Consumers
{
	public abstract class ProcessDrugsForLetterCommandConsumerBase : IConsumer<ProcessDrugsForLetterCommand>
	{
		protected abstract PharmacySiteModule Module { get; }

		protected abstract string Route { get; }

		protected readonly ILogger<ProcessDrugsForLetterCommandConsumerBase> _logger;

		public ProcessDrugsForLetterCommandConsumerBase(ILogger<ProcessDrugsForLetterCommandConsumerBase> logger)
		{
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ProcessDrugsForLetterCommand> context)
		{
			if (context.Message.Source != Module)
			{
				_logger.LogDebug("Команда для источника {ExpectedSource} пропущена текущим источником {CurrentSource}",
					context.Message.Source.ToString(), Module.ToString());
				return;
			}

			var letter = context.Message.Letter;
			_logger.LogInformation("Начинается сбор данных для буквы: {Letter}", letter);

			await Task.Delay(TimeSpan.FromSeconds(new Random().Next(2, 5)));

			var fakeDrugs = new List<DrugPharmacyPackage>
		{
			new() { 
				Drug = new DrugContract()
			{
				NameOriginal = $"{letter}абексол",
				NameTranslate = $"{letter}abeksol",
				FormOriginal = "Гель",
				FormTranslate = "Gel",
				ManufacturerOriginal = "Экофарм",
				ManufacturerTranslate = "Ecofarm",
				CountryOriginal = "Беларусь", 
				CountryTranslate = "Belarus", 
				Index = "1" 
			}, 
				PharmacySite = new() 
				{ 
					Module = Module, 
					SiteRoute = Route 
				} 
			},
			new() {
				Drug = new DrugContract()
			{
				NameOriginal = $"{letter}менадин",
				NameTranslate = $"{letter}menadin",
				FormOriginal = "Гель",
				FormTranslate = "Gel",
				ManufacturerOriginal = "Экофарм",
				ManufacturerTranslate = "Ecofarm",
				CountryOriginal = "Беларусь",
				CountryTranslate = "Belarus",
				Index = "1"
			},
				PharmacySite = new()
				{
					Module = Module,
					SiteRoute = Route
				}
			},
		};

			_logger.LogInformation("Сбор данных для буквы {Letter} завершен. Найдено {Count} препаратов.",
				letter, fakeDrugs.Count);

			await context.RespondAsync(new DrugsDataCollectedEvent
			{
				CorrelationId = context.Message.CorrelationId,
				Letter = letter,
				Source = Module,
				CollectedAt = DateTime.UtcNow,
				Drugs = fakeDrugs
			});
		}
	}
}
