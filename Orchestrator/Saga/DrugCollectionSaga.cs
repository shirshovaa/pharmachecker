using Common.Commands;
using Common.Enums;
using Common.Messages;
using MassTransit;
using Orchestrator.Database.Entities;

namespace Orchestrator.Saga
{
	public class DrugCollectionSaga : MassTransitStateMachine<DrugCollectionSagaState>
	{
		public State Pending { get; private set; }
		public State Completed { get; private set; }

		public Event<StartDrugCollectionEvent> StartCollection { get; set; }
		public Event<DrugsDataCollectedEvent> DataCollected { get; set; }

		public DrugCollectionSaga()
		{
			var sources = Enum.GetValues<PharmacySiteModule>();

			InstanceState(x => x.CurrentState);

			Event(() => StartCollection, e => e.CorrelateById(context => context.Message.CorrelationId));
			Event(() => DataCollected, e => e.CorrelateById(context => context.Message.CorrelationId));

			Initially(
				When(StartCollection)
					.Then(context =>
					{
						context.Saga.CorrelationId = context.Message.CorrelationId;
						context.Saga.Letter = context.Message.Letter;
						context.Saga.ReceivedSources = new List<PharmacySiteModule>();
					})
					.ThenAsync(async context =>
					{
						foreach (var source in sources)
						{
							if (context.Saga.ReceivedSources.Contains(source))
							{
								continue;
							}

							var sendEndpoint = await context.GetSendEndpoint(new Uri($"queue:process-drugs-letter-{source.ToString()}"));
							await sendEndpoint.Send(new ProcessDrugsForLetterCommand()
							{
								CorrelationId = context.Saga.CorrelationId,
								Letter = context.Saga.Letter,
								Source = source
							});
						}
					})
					.TransitionTo(Pending)
			);

			During(Pending,
				When(DataCollected)
					.ThenAsync(async context =>
					{
						if (context.Saga.ReceivedSources is null)
						{
							context.Saga.ReceivedSources = new List<PharmacySiteModule>();
						}

						context.Saga.ReceivedSources.Add(context.Message.Source);

						await context.Publish(new DrugsDataForAggregationEvent
						{
							CorrelationId = context.Message.CorrelationId,
							Letter = context.Message.Letter,
							Source = context.Message.Source,
							CollectedAt = context.Message.CollectedAt,
							Drugs = context.Message.Drugs
						});


						Console.WriteLine($"Данные от {context.Message.Source} для буквы {context.Message.Letter} отправлены в агрегатор");
					})
					.If(context => context.Saga.ReceivedSources.Count() == sources.Count() && context.Saga.ReceivedSources.All(_ => sources.Contains(_)), then => then.TransitionTo(Completed))
			);

			During(Completed,
				When(DataCollected)
					.Then(context =>
					{
						Console.WriteLine($"Все данные для буквы {context.Saga.Letter} получены от {context.Saga.ReceivedSources.Count} источников");
					})
					.Finalize()
			);

			SetCompletedWhenFinalized();
		}
	}
}
