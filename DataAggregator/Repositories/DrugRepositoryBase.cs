using Common.Contracts;
using DataAggregator.Database;
using DataAggregator.Database.Entities;
using DataAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.Repositories
{
	public abstract class DrugRepositoryBase<TEntity> : IDrugRepository<TEntity> where TEntity : DrugEntity
	{
		private readonly AggregatorDbContext _context;
		protected readonly DbSet<TEntity> _dbSet;

		protected DrugRepositoryBase(AggregatorDbContext context)
		{
			_context = context;
			_dbSet = context.Set<TEntity>();
		}

		public async Task AddOrUpdateAsync(DrugPharmacyPackage package)
		{
			var existingDrug = await GetByNameAsync(package.Drug.NameOriginal);

			if (existingDrug == null)
			{
				var newDrug = CreateEntity(package);
				await _dbSet.AddAsync(newDrug);
			}
			else
			{
				existingDrug.Map(package);
				_dbSet.Update(existingDrug);
			}

			await _context.SaveChangesAsync();
		}

		public async Task AddOrUpdateRangeAsync(IEnumerable<DrugPharmacyPackage> packages)
		{
			var contractsList = packages.ToList();
			var names = contractsList.Select(c => c.Drug.NameOriginal).Distinct();

			// Получаем существующие записи одним запросом
			var existingDrugs = await _dbSet
				.Where(d => names.Contains(d.NameOriginal))
				.ToDictionaryAsync(d => d.NameOriginal);

			var drugsToAdd = new List<TEntity>();
			var drugsToUpdate = new List<TEntity>();

			foreach (var contract in contractsList)
			{
				if (existingDrugs.TryGetValue(contract.Drug.NameOriginal, out var existingDrug))
				{
					// Обновляем существующую запись
					existingDrug.Map(contract);
					drugsToUpdate.Add(existingDrug);
				}
				else
				{
					// Добавляем новую запись
					drugsToAdd.Add(CreateEntity(contract));
				}
			}

			// Массовое добавление и обновление
			if (drugsToAdd.Any())
			{
				await _dbSet.AddRangeAsync(drugsToAdd);
			}

			if (drugsToUpdate.Any())
			{
				_dbSet.UpdateRange(drugsToUpdate);
			}

			await _context.SaveChangesAsync();
		}

		public async Task<TEntity> GetByNameAsync(string nameOriginal)
		{
			return await _dbSet.FirstOrDefaultAsync(d => d.NameOriginal == nameOriginal);
		}

		protected abstract TEntity CreateEntity(DrugPharmacyPackage package);
	}
}
