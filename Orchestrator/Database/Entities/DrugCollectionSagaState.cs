using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Database.Maps;

namespace Orchestrator.Database.Entities
{
	[EntityTypeConfiguration(typeof(DrugCollectionSagaStateMap))]
	public class DrugCollectionSagaState : SagaStateMachineInstance, ISagaVersion
	{
		/// <summary>
		/// Id корреляции MassTransit
		/// </summary>
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// Текущее состояние
		/// </summary>
		public string? CurrentState { get; set; }

		/// <summary>
		/// Буква/Символ на который начинается название лекарства
		/// </summary>
		public required string Letter { get; set; }

		public Guid? RequestId { get; set; }

		public Uri? ResponseAddress { get; set; }

		/// <summary>
		/// Версия
		/// </summary>
		public int Version { get; set; }
	}
}
