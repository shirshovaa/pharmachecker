using System.Collections.Concurrent;
using System.Net;
using System.Web;
using Common.Contracts;
using Common.Enums;
using DataHarvester.Strategies;
using DataHarvesterTabletkaBy.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Routing;

namespace DataHarvesterTabletkaBy.Strategies
{
	public class DataHarvesterStrategy : IDataHarvesterStrategy
	{
		public PharmacySiteModule Module => PharmacySiteModule.TabletkaBy;

		public string DrugLetterRoute => "https://tabletka.by/drugs/?search={0}";

		private readonly ConcurrentBag<DrugPharmacyPackage> _results = new();

		private readonly int _maxDegreeOfParallelism = 10;

		private HttpClient _client;

		private string _baseUrl = "https://tabletka.by";

		private string sessionId = string.Empty;

		private string token = string.Empty;

		private int _withoutNameCount = 0;

		private string _notExistOnRegionPattern = "Препарат отсутствует в регионе";

		private string _notExistOnCountryPattern = "Препарат отсутствует в продаже во всех аптеках";

		public DataHarvesterStrategy(HttpClient client)
		{
			_client = client;
		}

		public async Task<ICollection<DrugPharmacyPackage>> GetDrugsByLetterAsync(string letter)
		{
			var drugLinks = await GetDrugListAsync(string.Format(DrugLetterRoute, Uri.EscapeDataString(letter)));

			if (drugLinks.Count == 0)
			{
				return _results.ToList();
			}

			var options = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };
			await Parallel.ForEachAsync(drugLinks, options, async (link, cancellationToken) =>
			{
				try
				{
					await ProcessDrugPageAsync(link.Route);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при обработке {link}: {ex.Message}");
				}
			});

			_client.Dispose();

			Console.WriteLine($"Добавлено лекарств: {_results.Count}");
			Console.WriteLine($"Лекарств без названия: {_withoutNameCount}");
			Console.WriteLine($"Всего: {_withoutNameCount + _results.Count}");

			return _results.ToList();
		}

		private async Task<CookieContainer> GetSessionAsync()
		{
			try
			{
				// Делаем первоначальный запрос для получения сессионных куков
				using var initialRequest = new HttpRequestMessage(HttpMethod.Get, _baseUrl);

				// Используем отдельный HttpClient для получения куков
				using var handler = new HttpClientHandler
				{
					UseCookies = true,
					AllowAutoRedirect = true
				};

				using var tempClient = new HttpClient(handler);

				var response = await tempClient.SendAsync(initialRequest);
				response.EnsureSuccessStatusCode();

				// Извлекаем куки из ответа
				var cookies = handler.CookieContainer.GetCookies(new Uri(_baseUrl));

				if (string.IsNullOrEmpty(cookies["PHPSESSID"]?.Value) || string.IsNullOrEmpty(cookies["_csrf"]?.Value))
				{
					throw new Exception("Не удалось получить сессионные куки");
				}

				sessionId = cookies["PHPSESSID"].Value;
				token = cookies["_csrf"].Value;

				return handler.CookieContainer;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private void AddCookiesToRequest(HttpRequestMessage request)
		{
			// Формируем строку куков в точном формате как в Postman
			var cookieString = $"PHPSESSID={sessionId}; " +
							  $"_csrf={Uri.EscapeDataString(token)}; " +
							  $"lim-result=10000; " +
							  $"regionId=1001";

			request.Headers.Add("Cookie", cookieString);

			// Также добавляем необходимые заголовки
			request.Headers.Add("Accept", "*/*");
			request.Headers.Add("Connection", "keep-alive");
		}

		private async Task<List<DrugLink>> GetDrugListAsync(string route)
		{
			var list = new List<DrugLink>();

			await GetSessionAsync();
			using var request = new HttpRequestMessage(HttpMethod.Get, route);
			AddCookiesToRequest(request);

			// Выполняем запрос
			var response = await _client.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var html = new HtmlDocument();
			html.LoadHtml(content);

			var drugItems = html.DocumentNode.SelectNodes("//li[@class='search-result__item']/a");

			if (drugItems == null)
			{
				return list;
			}

			foreach (var item in drugItems)
			{
				var name = item.GetAttributeValue("title", "");
				var url = item.GetAttributeValue("href", "");

				if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
				{
					list.Add(new()
					{
						Name = name,
						Route = $"{_baseUrl}{url}"
					});
				}
			}

			Console.WriteLine($"Найдено {drugItems.Count} лекарств");

			return list;
		}

		private async Task ProcessDrugPageAsync(string route)
		{
			Console.WriteLine($"Обрабатываем: {route}");

			RowProcessModel model;
			var drugPage = await _client.GetStringAsync(route);

			if (drugPage.Contains(_notExistOnCountryPattern))
			{
				Console.WriteLine($"Отсутствуют зарегистрированые лекарства: {route}");
				return;
			}

			var html = new HtmlDocument();
			html.LoadHtml(drugPage);

			if (drugPage.Contains(_notExistOnRegionPattern))
			{
				model = new()
				{
					Rows = html.DocumentNode.SelectNodes("//table[@id='base-select']/tbody/tr"),
					Route = route,
					NamePath = ".//td[@class='name']//a",
					FormPath = ".//td[@class='form']//span[@class='form-title']",
					ManufacturerPath = ".//td[@class='produce']//span",
					CountryPath = ".//td[@class='produce']//span[@class='capture']"
				};
				Console.WriteLine($"Обработка лекарств отсутствующих в регионе: {route}");
			}
			else
			{
				model = new()
				{
					Rows = html.DocumentNode.SelectNodes("//table[@id='base-select']/tbody/tr"),
					Route = route,
					NamePath = ".//td[@class='name tooltip-info']//a",
					FormPath = ".//td[@class='form tooltip-info']//a",
					ManufacturerPath = ".//td[@class='produce tooltip-info']//a",
					CountryPath = ".//td[@class='produce tooltip-info']//span[@class='capture']/span"
				};
				Console.WriteLine($"Обработка лекарств: {route}");
			}

			ProcessRows(model);
		}

		private void ProcessRows(RowProcessModel model)
		{
			foreach (var row in model.Rows)
			{
				var package = new DrugPharmacyPackage() { Drug = new(), PharmacySite = new() { Module = PharmacySiteModule.TabletkaBy } };

				// Извлекаем название и ссылку
				var nameNode = row.SelectSingleNode(model.NamePath);
				if (nameNode != null)
				{
					package.Drug.NameOriginal = nameNode.InnerText.Trim();
					package.PharmacySite.SiteRoute = $"{_baseUrl}{nameNode.GetAttributeValue("href", "")}";
					var query = HttpUtility.ParseQueryString(package.PharmacySite.SiteRoute);
					package.Drug.Index = query.Get(0);
				}
				else
				{
					Console.WriteLine($"Отсутствует имя лекарства {model.Route}");
					_withoutNameCount++;
					continue;
				}

				// Извлекаем форму
				var formNode = row.SelectSingleNode(model.FormPath);
				if (formNode != null)
				{
					package.Drug.FormOriginal = formNode.InnerText.Trim();
				}

				// Извлекаем производителя и страну
				var manufacturerNode = row.SelectSingleNode(model.ManufacturerPath);

				if (manufacturerNode != null)
				{
					package.Drug.ManufacturerOriginal = manufacturerNode.InnerText.Trim();
				}

				var countryNode = row.SelectSingleNode(model.CountryPath);
				if (countryNode != null)
				{
					package.Drug.CountryOriginal = countryNode.InnerText.Trim();
				}

				_results.Add(package);
				Console.WriteLine($"Добавлено {package.PharmacySite.SiteRoute}");
			}

			Console.WriteLine($"Обработано {model.Rows.Count} лекарств");
		}
	}
}
