using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class ResourceRepository : Repository<Resource>, IResourceRepository
    {
        public ResourceRepository(EventDbContext context) : base(context) { }

        public async Task<List<Resource>> GetActiveResourcesAsync()
        {
            return await _dbSet
                .Where(r => r.IsActive == true)
                .OrderBy(r => r.Type)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<Resource>> GetByTypeAsync(string type)
        {
            return await _dbSet
                .Where(r => r.Type == type)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<Resource>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(r => r.Name.ToLower().Contains(term) ||
                           (r.Type != null && r.Type.ToLower().Contains(term)))
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(r => r.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(r => r.ResourceId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> IsResourceInUseAsync(int resourceId)
        {
            return await _context.EventResources
                .AnyAsync(er => er.ResourceId == resourceId);
        }

        public async Task<int> GetTotalAllocatedQuantityAsync(int resourceId)
        {
            var allocatedQuantity = await _context.EventResources
                .Where(er => er.ResourceId == resourceId)
                .SumAsync(er => er.QuantityUsed);

            return allocatedQuantity;
        }
    }
}
