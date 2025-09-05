using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Contracts;
using Common.Enums;

namespace DataHarvester.Strategies
{
	public interface IDataHarvesterStrategy
	{
		public PharmacySiteModule Module { get; }

		public string DrugLetterRoute { get; }

		public Task<ICollection<DrugPharmacyPackage>> GetDrugsByLetterAsync(string letter);
	}
}
