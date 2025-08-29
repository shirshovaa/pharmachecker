using Common.Commands;
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

		public Request<DrugCollectionSagaState, ProcessDrugsForLetterCommand, DrugsDataCollectedEvent> SendToProcess { get; set; }

		public DrugCollectionSaga()
		{
			InstanceState(x => x.CurrentState);

			Event(() => StartCollection, e => e.CorrelateById(context => context.Message.CorrelationId));

			Request(() => SendToProcess);

			Initially(
				When(StartCollection)
					.Then(context =>
					{
						if (!context.TryGetPayload(out SagaConsumeContext<DrugCollectionSagaState, StartDrugCollectionEvent> payload))
							throw new Exception("Unable to retrieve required payload for callback data.");

						context.Saga.CorrelationId = payload.Message.CorrelationId;
						context.Saga.Letter = payload.Message.Letter;
					})
					.Request(SendToProcess,
					context => new ProcessDrugsForLetterCommand
					{
						CorrelationId = context.Saga.CorrelationId,
						Letter = context.Saga.Letter
					})
					.TransitionTo(Pending)
			);

			During(Pending,
				When(SendToProcess.Completed)
					.Then(context =>
					{
						context.Saga.CurrentState = Completed.Name;
						Console.WriteLine($"Data collected for letter {context.Saga.Letter}");
					})
					.Finalize(),
				When(SendToProcess.Faulted)
					.Then(context => Console.WriteLine($"{context.RequestId} {context.Saga.CorrelationId} {string.Join("; ", context.Message.Exceptions.Select(x => x.Message))}"))
			);

			SetCompletedWhenFinalized();
		}
	}
}
