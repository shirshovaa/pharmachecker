using Common.Commands;
using Common.Messages;
using MassTransit;
using Orchestrator.Database.Entities;

namespace Orchestrator.Saga
{
	public class DrugCollectionSaga : MassTransitStateMachine<DrugCollectionSagaState>
	{
		public State AwaitingCollection { get; private set; }
		public State Completed { get; private set; }

		public Event<StartDrugCollectionEvent> StartCollection { get; private set; }
		public Event<DrugsDataCollectedEvent> DataCollected { get; private set; }

		public DrugCollectionSaga()
		{
			InstanceState(x => x.CurrentState);

			Event(() => StartCollection, e => e.CorrelateById(context => context.Message.CorrelationId));
			Event(() => DataCollected, e => e.CorrelateById(context => context.Message.CorrelationId));

			Initially(
				When(StartCollection)
					.Then(context => context.Saga.Letter = context.Message.Letter)
					.Publish(context => new ProcessDrugsForLetterCommand
					{
						CorrelationId = context.Saga.CorrelationId,
						Letter = context.Saga.Letter
					})
					.TransitionTo(AwaitingCollection)
			);

			During(AwaitingCollection,
				When(DataCollected)
					.Then(context => Console.WriteLine($"Data collected for letter {context.Saga.Letter}"))
					.Finalize()
			);

			SetCompletedWhenFinalized();
		}
	}
}
