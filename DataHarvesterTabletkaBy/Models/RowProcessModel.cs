using HtmlAgilityPack;

namespace DataHarvesterTabletkaBy.Models
{
	public class RowProcessModel
	{
		/// <summary>
		/// Строки для обработки
		/// </summary>
		public HtmlNodeCollection Rows { get; set; }

		/// <summary>
		/// Путь по которому получена страница
		/// </summary>
		public string Route { get; set; }

		/// <summary>
		/// Путь для получения названия лекарства
		/// </summary>
		public string NamePath { get; set; }

		/// <summary>
		/// Путь для получения формы
		/// </summary>
		public string FormPath { get; set; }

		/// <summary>
		/// Путь для получения производителя
		/// </summary>
		public string ManufacturerPath { get; set; }

		/// <summary>
		/// Путь для получения страны
		/// </summary>
		public string CountryPath { get; set; }
	}
}
