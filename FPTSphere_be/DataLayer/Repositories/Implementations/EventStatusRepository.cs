using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Implementations;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class EventStatusRepository : Repository<EventStatus>, IEventStatusRepository
    {
        public EventStatusRepository(EventDbContext context) : base(context) { }

        public async Task<EventStatus?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == name.ToLower());
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(s => s.StatusName.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(s => s.StatusId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> IsInUseAsync(int statusId)
        {
            return await _context.Events
                .AnyAsync(e => e.StatusId == statusId && e.IsDeleted != true);
        }
    }
}
