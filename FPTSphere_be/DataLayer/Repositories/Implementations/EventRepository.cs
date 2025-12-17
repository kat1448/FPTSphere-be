using System;
using System.Collections.Generic;
using System.Linq;
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

        public IQueryable<Event> Query() => _dbSet.AsQueryable();

        /// <summary>
        /// Get event by ID with all details (no sub-events)
        /// </summary>
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

        /// <summary>
        /// ⭐ NEW: Get event by ID with details AND sub-events
        /// Single optimized query - no N+1 problem!
        /// </summary>
        public async Task<Event?> GetByIdWithSubEventsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Location)
                .Include(e => e.ExternalLocation)
                .Include(e => e.Status)
                // ⭐ Include sub-events with their location details
                .Include(e => e.InverseParentEvent.Where(se => se.IsDeleted != true))
                    .ThenInclude(se => se.Location)
                .Include(e => e.InverseParentEvent)
                    .ThenInclude(se => se.ExternalLocation)
                .Include(e => e.InverseParentEvent)
                    .ThenInclude(se => se.Status)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        /// <summary>
        /// Get all events with details (no sub-events)
        /// </summary>
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

        /// <summary>
        /// Get sub-events by parent ID
        /// </summary>
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