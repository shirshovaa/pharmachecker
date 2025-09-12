using System.Text.Json;
using Common.Contracts;
using Common.Messages;
using DataAggregator.Repositories;
using Moq;
using StackExchange.Redis;

namespace Tests.DataAggregator
{

	[TestClass]
	public class RadisCacheRepositoryTests
	{
		private Mock<IDatabase> _databaseMock;
		private Mock<IConnectionMultiplexer> _redisMock;
		private RadisCacheRepository _repository;

		[TestInitialize]
		public void Setup()
		{
			_databaseMock = new Mock<IDatabase>();
			_redisMock = new Mock<IConnectionMultiplexer>();
			_redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
				.Returns(_databaseMock.Object);
			_repository = new RadisCacheRepository(_redisMock.Object);
		}

		#region StoreBatchAsync

		[TestMethod]
		public async Task StoreBatchAsync_ShouldStoreDataAndAddToPendingBatches()
		{
			// Arrange
			var batchId = Guid.NewGuid();
			var batchData = new DrugsDataForAggregationEvent
			{
				Source = Common.Enums.PharmacySiteModule.TabletkaBy,
				Letter = "A",
				Drugs = new List<DrugPharmacyPackage>()
			};

			_databaseMock.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(true);
			_databaseMock.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(true);

			// Act
			await _repository.StoreBatchAsync(batchId, batchData);

			// Assert
			_databaseMock.Verify(x => x.StringSetAsync(
				It.Is<RedisKey>(k => k.ToString().Contains(batchId.ToString())),
				It.IsAny<RedisValue>(),
				It.IsAny<TimeSpan>(),
				It.IsAny<bool>(),
				It.IsAny<When>(),
				It.IsAny<CommandFlags>()), Times.Once);

			_databaseMock.Verify(x => x.SetAddAsync(
				"pending_batches",
				batchId.ToString(),
				It.IsAny<CommandFlags>()), Times.Once);
		}

		#endregion

		#region GetBatchAsync

		[TestMethod]
		public async Task GetBatchAsync_WithValidData_ShouldReturnDeserializedObject()
		{
			// Arrange
			var batchId = Guid.NewGuid();
			var batchData = new DrugsDataForAggregationEvent
			{
				Source = Common.Enums.PharmacySiteModule.TabletkaBy,
				Letter = "A",
				Drugs = new List<DrugPharmacyPackage>()
			};
			var serializedData = JsonSerializer.Serialize(batchData);

			_databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(serializedData);

			// Act
			var result = await _repository.GetBatchAsync(batchId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(Common.Enums.PharmacySiteModule.TabletkaBy, result.Source);
			Assert.AreEqual("A", result.Letter);
		}

		[TestMethod]
		public async Task GetBatchAsync_WithNullData_ShouldReturnNull()
		{
			// Arrange
			var batchId = Guid.NewGuid();
			_databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(RedisValue.Null);

			// Act
			var result = await _repository.GetBatchAsync(batchId);

			// Assert
			Assert.IsNull(result);
		}

		#endregion

		#region RemoveBatchAsync

		[TestMethod]
		public async Task RemoveBatchAsync_ShouldDeleteKeyAndRemoveFromPendingBatches()
		{
			// Arrange
			var batchId = Guid.NewGuid();
			_databaseMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(true);
			_databaseMock.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(true);

			// Act
			await _repository.RemoveBatchAsync(batchId);

			// Assert
			_databaseMock.Verify(x => x.KeyDeleteAsync(
				It.Is<RedisKey>(k => k.ToString().Contains(batchId.ToString())),
				It.IsAny<CommandFlags>()), Times.Once);

			_databaseMock.Verify(x => x.SetRemoveAsync(
				"pending_batches",
				batchId.ToString(),
				It.IsAny<CommandFlags>()), Times.Once);
		}

		#endregion

		#region GetPendingBatchIdsAsync

		[TestMethod]
		public async Task GetPendingBatchIdsAsync_WithPendingBatches_ShouldReturnGuidList()
		{
			// Arrange
			var batchIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
			var redisValues = batchIds.Select(id => (RedisValue)id).ToArray();

			_databaseMock.Setup(x => x.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(redisValues);

			// Act
			var result = await _repository.GetPendingBatchIdsAsync();

			// Assert
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.All(id => batchIds.Contains(id.ToString())));
		}

		[TestMethod]
		public async Task GetPendingBatchIdsAsync_NoPendingBatches_ShouldReturnEmptyList()
		{
			// Arrange
			_databaseMock.Setup(x => x.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
				.ReturnsAsync(Array.Empty<RedisValue>());

			// Act
			var result = await _repository.GetPendingBatchIdsAsync();

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}

		#endregion
	}
}
