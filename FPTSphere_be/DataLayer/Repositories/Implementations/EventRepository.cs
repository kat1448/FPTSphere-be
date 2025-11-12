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
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(EventDbContext context) : base(context) { }

        public async Task<Event?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.CreatedByNavigation).ThenInclude(u => u.Role)
                .Include(e => e.Location)
                .Include(e => e.ExternalLocation)
                .Include(e => e.Status)
                .Include(e => e.Template)
                .Include(e => e.ParentEvent)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<List<Event>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(e => e.CreatedByNavigation).ThenInclude(u => u.Role)
                .Include(e => e.Location)
                .Include(e => e.ExternalLocation)
                .Include(e => e.Status)
                .Include(e => e.Template)
                .Include(e => e.ParentEvent)
                .ToListAsync();
        }
        public async Task<List<Event>> GetSubEventsByParentIdAsync(int parentEventId)
        {
            return await _dbSet
                .Include(e => e.CreatedByNavigation).ThenInclude(u => u.Role)
                .Include(e => e.Location)
                .Include(e => e.ExternalLocation)
                .Include(e => e.Status)
                .Include(e => e.ParentEvent)
                .Where(e => e.ParentEventId == parentEventId && e.IsDeleted != true)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

    }
}
