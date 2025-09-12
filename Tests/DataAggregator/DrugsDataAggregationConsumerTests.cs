using Common.Contracts;
using Common.Messages;
using DataAggregator.Consumers;
using DataAggregator.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.DataAggregator
{

	[TestClass]
	public class DrugsDataAggregationConsumerTests
	{
		private Mock<IRadisCacheRepository> _cacheRepositoryMock;
		private Mock<ILogger<DrugsDataAggregationConsumer>> _loggerMock;
		private DrugsDataAggregationConsumer _consumer;

		[TestInitialize]
		public void Setup()
		{
			_cacheRepositoryMock = new Mock<IRadisCacheRepository>();
			_loggerMock = new Mock<ILogger<DrugsDataAggregationConsumer>>();
			_consumer = new DrugsDataAggregationConsumer(_cacheRepositoryMock.Object, _loggerMock.Object);
		}

		#region Consume

		[TestMethod]
		public async Task Consume_ShouldStoreBatchInRedis()
		{
			// Arrange
			var message = new DrugsDataForAggregationEvent
			{
				Source = Common.Enums.PharmacySiteModule.Apteka103By,
				Letter = "A",
				Drugs = new List<DrugPharmacyPackage>()
			};
			var contextMock = new Mock<ConsumeContext<DrugsDataForAggregationEvent>>();
			contextMock.Setup(x => x.Message).Returns(message);

			_cacheRepositoryMock.Setup(x => x.StoreBatchAsync(It.IsAny<Guid>(), It.IsAny<DrugsDataForAggregationEvent>()))
				.Returns(Task.CompletedTask);

			// Act
			await _consumer.Consume(contextMock.Object);

			// Assert
			_cacheRepositoryMock.Verify(x => x.StoreBatchAsync(It.IsAny<Guid>(), message), Times.Once);
			_loggerMock.Verify(x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Получены данные от")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[TestMethod]
		public async Task Consume_WithException_ShouldThrow()
		{
			// Arrange
			var message = new DrugsDataForAggregationEvent
			{
				Source = Common.Enums.PharmacySiteModule.Apteka103By,
				Letter = "A",
				Drugs = new List<DrugPharmacyPackage>()
			};
			var contextMock = new Mock<ConsumeContext<DrugsDataForAggregationEvent>>();
			contextMock.Setup(x => x.Message).Returns(message);

			_cacheRepositoryMock.Setup(x => x.StoreBatchAsync(It.IsAny<Guid>(), It.IsAny<DrugsDataForAggregationEvent>()))
				.ThrowsAsync(new Exception("Test error"));

			// Act & Assert
			await Assert.ThrowsExceptionAsync<Exception>(() => _consumer.Consume(contextMock.Object));
		}

		#endregion
	}
}
