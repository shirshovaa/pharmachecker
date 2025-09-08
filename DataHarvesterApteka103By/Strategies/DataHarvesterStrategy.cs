using System.Collections.Concurrent;
using Common.Contracts;
using Common.Enums;
using DataHarvester.Strategies;
using DataHarvesterApteka103By.Models;
using HtmlAgilityPack;

namespace DataHarvesterApteka103By.Strategies
{
	public class DataHarvesterStrategy : IDataHarvesterStrategy
	{
		public PharmacySiteModule Module => PharmacySiteModule.Apteka103By;

		public string DrugLetterRoute => "https://apteka.103.by/lekarstva-minsk/?l=";

		private readonly ConcurrentBag<DrugPharmacyPackage> _results = new();

		private readonly int _maxDegreeOfParallelism = 10;

		private readonly HttpClient _client;

		public DataHarvesterStrategy(HttpClient client)
		{
			_client = client;
		}

		public async Task<ICollection<DrugPharmacyPackage>> GetDrugsByLetterAsync(string letter)
		{
			var drugLinks = await GetDrugListAsync($"{DrugLetterRoute}{letter}");

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

			return _results.ToList();
		}

		private async Task<List<DrugLink>> GetDrugListAsync(string route)
		{
			var list = new List<DrugLink>();

			_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

			var response = await _client.GetAsync(route);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var html = new HtmlDocument();
			html.LoadHtml(content);

			var listNodes = html.DocumentNode.SelectNodes("//ul[@class='list']");

			if (listNodes != null)
			{
				foreach (var listNode in listNodes)
				{
					var liNodes = listNode.SelectNodes("./li");

					if (liNodes != null)
					{
						foreach (var liNode in liNodes)
						{
							var linkNode = liNode.SelectSingleNode("./a[@href]");

							if (linkNode != null)
							{
								string href = linkNode.GetAttributeValue("href", "");
								string name = linkNode.InnerText.Trim();

								if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(name))
								{
									list.Add(new DrugLink
									{
										Name = name,
										Route = href
									});
								}
							}
						}
					}
				}
			}

			return list;
		}

		private async Task ProcessDrugPageAsync(string route)
		{
			Console.WriteLine($"Обрабатываем: {route}");

			var drugPage = await _client.GetStringAsync(route);
			var links = ParseDrugPharmaciesPage(drugPage);

			Console.WriteLine($"Найдено ссылок 'где купить' для {route}: {links.Count}");

			var options = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };
			await Parallel.ForEachAsync(links, options, async (buyLink, cancellationToken) =>
			{
				try
				{
					await ProcessPharmacyPageAsync(route, buyLink);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при обработке аптеки {buyLink}: {ex.Message}");
				}
			});
		}

		private List<string> ParseDrugPharmaciesPage(string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var links = new List<string>();
			var optionSection = doc.DocumentNode.SelectSingleNode("//section[@id='option']");

			if (optionSection != null)
			{
				var buyButtons = optionSection.SelectNodes(".//a[@data-btn='where-to-by']");
				if (buyButtons != null)
				{
					foreach (var button in buyButtons)
					{
						if (button.Attributes["href"] != null)
						{
							links.Add(button.Attributes["href"].Value);
						}
					}
				}
			}

			return links;
		}

		private async Task ProcessPharmacyPageAsync(string medicineUrl, string pharmacyUrl)
		{
			Console.WriteLine($"Получаем данные из аптеки: {pharmacyUrl}");

			var pharmacyHtml = await _client.GetStringAsync(pharmacyUrl);

			var package = ParseDrug(pharmacyHtml, pharmacyUrl);

			_results.Add(package);
		}

		private DrugPharmacyPackage ParseDrug(string html, string drugRoute)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='drugInfo__title']");
			var descriptionNode = doc.DocumentNode.SelectSingleNode("//span[@class='drugInfo__description']");

			string nameOriginal = string.Empty;
			string formOriginal = string.Empty;
			string manufacturerOriginal = string.Empty;
			string countryOriginal = string.Empty;

			// Парсим название и форму из заголовка
			if (titleNode != null)
			{
				var titleText = titleNode.InnerText.Trim();
				var titleParts = titleText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				if (titleParts.Length >= 1)
					nameOriginal = titleParts[0].Trim();

				if (titleParts.Length >= 2)
					formOriginal = titleParts[1].Trim();
			}

			// Парсим производителя и страну из описания
			if (descriptionNode != null)
			{
				var descriptionText = descriptionNode.InnerText.Trim();
				var descriptionParts = descriptionText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				if (descriptionParts.Length >= 1)
					manufacturerOriginal = descriptionParts[0].Trim();

				if (descriptionParts.Length >= 2)
					countryOriginal = descriptionParts[1].Trim();
			}

			// Дополнительно парсим информацию из URL
			var fromUrl = ParseUrlInfo(drugRoute);

			return new DrugPharmacyPackage
			{
				Drug = new()
				{
					NameOriginal = nameOriginal,
					NameTranslate = fromUrl.name,
					FormOriginal = formOriginal,
					FormTranslate = fromUrl.form,
					ManufacturerOriginal = manufacturerOriginal,
					ManufacturerTranslate = fromUrl.manufacturer,
					CountryOriginal = countryOriginal,
					CountryTranslate = string.Empty,
					Index = string.Empty
				},
				PharmacySite = new()
				{
					Module = PharmacySiteModule.Apteka103By,
					SiteRoute = drugRoute
				}
			};
		}

		private (string name, string form, string manufacturer) ParseUrlInfo(string url)
		{
			var name = string.Empty;
			var form = string.Empty;
			var manufacturer = string.Empty;

			try
			{
				var uri = new Uri(url);
				var pathSegments = uri.AbsolutePath.Trim('/').Split('/');

				// Пример URL: https://apteka.103.by/a-bronhiks/61061-capsules-n60/sashera-med/minsk/
				// pathSegments: ["a-bronhiks", "61061-capsules-n60", "sashera-med", "minsk"]

				if (pathSegments.Length >= 1)
				{
					// Первый сегмент - название лекарства
					name = pathSegments[0];
				}

				if (pathSegments.Length >= 2)
				{
					// Второй сегмент - форма и количество
					form = pathSegments[1];
				}

				if (pathSegments.Length >= 3)
				{
					// Третий сегмент - производитель
					manufacturer = pathSegments[2];
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при парсинге URL {url}: {ex.Message}");
			}

			return (name, form, manufacturer);
		}
	}
}
