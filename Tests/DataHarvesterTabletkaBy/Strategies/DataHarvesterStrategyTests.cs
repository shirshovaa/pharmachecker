using System.Net;
using System.Net.Http;
using DataHarvester.Strategies;
using DataHarvesterTabletkaBy.Strategies;
using Moq;

namespace Tests.DataHarvesterTabletkaBy.Strategies
{
	[TestClass]
	public class DataHarvesterStrategyTests
	{
		private IDataHarvesterStrategy _strategy;
		private Mock<HttpMessageHandler> _httpMessageHandlerMock;
		private HttpClient _httpClient;
		private static string FilesDirectory => Path.Combine(
			Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
			"DataHarvesterTabletkaBy",
			"Strategies",
			"Files");

		public DataHarvesterStrategyTests()
		{

		}

		[TestMethod]
		public async Task GetDrugsByLetterAsync_Success_Real_Test()
		{
			var httpClientHandler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseCookies = true,
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				UseProxy = false
			};
			var client = new HttpClient(httpClientHandler);

			_strategy = new DataHarvesterStrategy(client);
			var drus = await _strategy.GetDrugsByLetterAsync("А");

			Assert.IsNotNull(drus);
			Assert.AreEqual(3195, drus.Count);
		}
	}
}
