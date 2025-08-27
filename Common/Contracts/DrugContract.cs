using System.Text;

namespace Common.Contracts
{
	public class DrugContract
	{
		public string Name { get; set; }

		public string Address { get; set; }

		public string Price { get; set; }

		public string LastUpdated { get; set; }

		public bool WasUpdated { get; set; } = false;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append($"В наличии в аптеке {Name} по адресу {Address}. Последнее обновление было в {LastUpdated}.");
			if (WasUpdated)
			{
				sb.Append($" (Осталось после обновления)");
			}
			else
			{
				sb.Append($" (Новое)");
			}

			return sb.ToString();
		}
	}
}
