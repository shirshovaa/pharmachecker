using Common.Messages;
using DataAggregator.Repositories.Interfaces;
using DataAggregator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAggregator.Tests
{
	[TestClass]
	public class DrugProcessingHostedServiceTests
	{
		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILogger<DrugProcessingHostedService>> _loggerMock;
		private DrugProcessingHostedService _service;

		[TestInitialize]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();
			_loggerMock = new Mock<ILogger<DrugProcessingHostedService>>();
			_service = new DrugProcessingHostedService(_serviceProviderMock.Object, _loggerMock.Object);
		}

		#region ExecuteAsync

		[TestMethod]
		public async Task ExecuteAsync_ShouldProcessBatchesAndHandleExceptions()
		{
			// Arrange
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			// Act
			var task = _service.StartAsync(cancellationToken);
			await Task.Delay(100);
			cancellationTokenSource.Cancel();
			await task;

			// Assert
			_loggerMock.Verify(x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Сервис обработки пакетов из Redis запущен")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.AtLeastOnce);
		}

		#endregion

		#region ProcessPendingBatchAsync

		[TestMethod]
		public async Task ProcessPendingBatchAsync_NoPendingBatches_ShouldReturnEarly()
		{
			// Arrange
			var scopeMock = new Mock<IServiceScope>();
			var scopeFactoryMock = new Mock<IServiceScopeFactory>();
			var cacheRepositoryMock = new Mock<IRadisCacheRepository>();

			_serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
				.Returns(scopeFactoryMock.Object);
			scopeFactoryMock.Setup(x => x.CreateScope())
				.Returns(scopeMock.Object);
			scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IRadisCacheRepository)))
				.Returns(cacheRepositoryMock.Object);

			cacheRepositoryMock.Setup(x => x.GetPendingBatchIdsAsync())
				.ReturnsAsync(new List<Guid>());

			var cancellationToken = CancellationToken.None;

			// Act
			var method = typeof(DrugProcessingHostedService)
				.GetMethod("ProcessPendingBatchAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			await (Task)method.Invoke(_service, new object[] { cancellationToken });

			// Assert
			cacheRepositoryMock.Verify(x => x.GetPendingBatchIdsAsync(), Times.Once);
			cacheRepositoryMock.Verify(x => x.GetBatchAsync(It.IsAny<Guid>()), Times.Never);
		}

		[TestMethod]
		public async Task ProcessPendingBatchAsync_BatchNotFound_ShouldRemoveBatch()
		{
			// Arrange
			var batchId = Guid.NewGuid();
			var scopeMock = new Mock<IServiceScope>();
			var scopeFactoryMock = new Mock<IServiceScopeFactory>();
			var cacheRepositoryMock = new Mock<IRadisCacheRepository>();

			_serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
				.Returns(scopeFactoryMock.Object);
			scopeFactoryMock.Setup(x => x.CreateScope())
				.Returns(scopeMock.Object);
			scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IRadisCacheRepository)))
				.Returns(cacheRepositoryMock.Object);

			cacheRepositoryMock.Setup(x => x.GetPendingBatchIdsAsync())
				.ReturnsAsync(new List<Guid> { batchId });
			cacheRepositoryMock.Setup(x => x.GetBatchAsync(batchId))
				.ReturnsAsync((DrugsDataForAggregationEvent)null);

			var cancellationToken = CancellationToken.None;

			// Act
			var method = typeof(DrugProcessingHostedService)
				.GetMethod("ProcessPendingBatchAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			await (Task)method.Invoke(_service, new object[] { cancellationToken });

			// Assert
			cacheRepositoryMock.Verify(x => x.RemoveBatchAsync(batchId), Times.Once);
			_loggerMock.Verify(x => x.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Пакет {batchId} не найден в Redis")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		#endregion
	}
}