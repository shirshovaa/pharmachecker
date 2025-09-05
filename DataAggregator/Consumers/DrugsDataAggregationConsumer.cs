using Common.Messages;
using DataAggregator.Database;
using DataAggregator.Database.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Consumers
{
	public class DrugsDataAggregationConsumer : IConsumer<DrugsDataForAggregationEvent>
	{
		private readonly AggregatorDbContext _context;
		private readonly ILogger<DrugsDataAggregationConsumer> _logger;

		public DrugsDataAggregationConsumer(
			AggregatorDbContext context,
			ILogger<DrugsDataAggregationConsumer> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<DrugsDataForAggregationEvent> context)
		{
			var message = context.Message;

			_logger.LogInformation("Получены данные от {Source} для буквы {Letter} ({Count} препаратов)",
				message.Source, message.Letter, message.Drugs.Count);

			try
			{
				foreach (var package in message.Drugs)
				{
					var drug = _context.Drugs.Include(_ => _.PharmacySites).FirstOrDefault(_ => _.NameOriginal == package.Drug.NameOriginal);
					var isNewDrug = false;
					PharmacySiteEntity pharmacySite = null;
					if (drug is null)
					{
						drug = new DrugEntity(package.Drug);
						pharmacySite = new PharmacySiteEntity(package.PharmacySite, drug.Id);
						drug.PharmacySites.Add(pharmacySite);
						isNewDrug = true;
					}
					else
					{
						drug.Map(package.Drug);

						if (drug.PharmacySites is null)
						{
							drug.PharmacySites = new List<PharmacySiteEntity>();
						}

						pharmacySite = drug.PharmacySites.FirstOrDefault(_ => _.Module == package.PharmacySite.Module);

						if (pharmacySite is null)
						{
							pharmacySite = new PharmacySiteEntity(package.PharmacySite, drug.Id);
							_context.Sites.Add(pharmacySite);
							drug.PharmacySites.Add(pharmacySite);
						}
						else
						{
							pharmacySite.Map(package.PharmacySite);
						}
					}

					if (isNewDrug)
					{
						_context.Drugs.Add(drug);
					}

					_context.SaveChanges();
				}

				_logger.LogInformation("Данные от {Source} для буквы {Letter} успешно сохранены",
					message.Source, message.Letter);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении данных от {Source} для буквы {Letter}",
					message.Source, message.Letter);
				throw;
			}
		}
	}
}
