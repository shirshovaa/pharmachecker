using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataHarvester.Strategies;
using DataHarvesterApteka103By.Strategies;
using Moq;
using Moq.Protected;

namespace Tests.DataHarvesterApteka103By.Strategies
{
	[TestClass]
	public class DataHarvesterStrategyTests
	{
		private IDataHarvesterStrategy _strategy;
		private Mock<HttpMessageHandler> _httpMessageHandlerMock;
		private HttpClient _httpClient;
		private static string FilesDirectory => Path.Combine(
			Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
			"DataHarvesterApteka103By",
			"Strategies",
			"Files");

		public DataHarvesterStrategyTests()
		{
			_httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var list = File.ReadAllText(Path.Combine(FilesDirectory, "drug-list.html"));
			var drug = File.ReadAllText(Path.Combine(FilesDirectory, "drug.html"));
			var drugInfo = File.ReadAllText(Path.Combine(FilesDirectory, "drug-info.html"));

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

			var drugInfoResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(drugInfo)
			};

			// Первый запрос - Список лекарств
			_httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("lekarstva-minsk")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(listResponse);

			// Второй запрос - Страница лекарства
			_httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("drug/")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(drugResponse);

			// Второй запрос - Страница аптек
			_httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/minsk/")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(drugInfoResponse);

			_httpClient = new HttpClient(_httpMessageHandlerMock.Object);
			_strategy = new DataHarvesterStrategy(_httpClient);
		}

		[TestMethod]
		public async Task GetDrugsByLetterAsync_Success()
		{
			var drus = await _strategy.GetDrugsByLetterAsync("А");

			Assert.IsNotNull(drus);
			Assert.AreEqual(1, drus.Count);
		}

		[TestMethod]
		public async Task GetDrugsByLetterAsync_Success_Real_Test()
		{
			_strategy = new DataHarvesterStrategy(new HttpClient());
			var drus = await _strategy.GetDrugsByLetterAsync("А");

			Assert.IsNotNull(drus);
			Assert.AreEqual(1058, drus.Count);
		}
	}
}
