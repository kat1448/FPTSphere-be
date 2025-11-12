using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using EventDbContext = DataLayer.Data.EventDbContext;

namespace DataLayer.Repositories.Implementations
{
    public class ExternalLocationRepository : Repository<ExternalLocation>, IExternalLocationRepository
    {
        public ExternalLocationRepository(EventDbContext context) : base(context) { }

        public async Task<List<ExternalLocation>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(el => el.Name.ToLower().Contains(term) ||
                            el.Address.ToLower().Contains(term) ||
                            (el.ContactPerson != null && el.ContactPerson.ToLower().Contains(term)))
                .ToListAsync();
        }

        public async Task<List<ExternalLocation>> GetByCostRangeAsync(decimal minCost, decimal maxCost)
        {
            return await _dbSet
                .Where(el => el.Cost >= minCost && el.Cost <= maxCost)
                .OrderBy(el => el.Cost)
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(el => el.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(el => el.ExternalLocationId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> IsInUseAsync(int externalLocationId)
        {
            return await _context.Events
                .AnyAsync(e => e.ExternalLocationId == externalLocationId && e.IsDeleted != true);
        }
    }
}
