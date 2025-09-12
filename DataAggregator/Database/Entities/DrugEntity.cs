using System.ComponentModel.DataAnnotations;

namespace DataAggregator.Database.Entities
{
	public partial class DrugEntity
	{
		[Key]
		public Guid Id { get; set; }

		public string NameOriginal { get; set; }

		public string FormOriginal { get; set; }

		public string ManufacturerOriginal { get; set; }

		public string CountryOriginal { get; set; }

		public string SiteRoute { get; set; }
	}
}
