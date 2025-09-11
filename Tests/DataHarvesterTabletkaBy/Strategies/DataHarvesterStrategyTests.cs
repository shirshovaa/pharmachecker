using System.Net;
using System.Net.Http;
using DataHarvester.Strategies;
using DataHarvesterTabletkaBy.Strategies;
using Moq;
using Moq.Protected;

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
			_httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var list = File.ReadAllText(Path.Combine(FilesDirectory, "drug-list.html"));
			var drug = File.ReadAllText(Path.Combine(FilesDirectory, "drug.html"));

			var listResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(list)
			};

			var drugResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(drug)
			};

			// Первый запрос - Список лекарств
			_httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/drugs/")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(listResponse);

			// Второй запрос - Страница лекарства
			_httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/search/")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(drugResponse);

			_httpClient = new HttpClient(_httpMessageHandlerMock.Object);
			_strategy = new DataHarvesterStrategy(_httpClient);
		}

		[TestMethod]
		public async Task GetDrugsByLetterAsync_Success()
		{
			var drus = await _strategy.GetDrugsByLetterAsync("А");

			Assert.IsNotNull(drus);
			Assert.AreEqual(3, drus.Count);
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
