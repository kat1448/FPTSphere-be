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
    public class EventResourceRepository : Repository<EventResource>, IEventResourceRepository
    {
        public EventResourceRepository(EventDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EventResource>> GetResourcesByEventIdAsync(int eventId)
        {
            return await _context.EventResources
                .Include(er => er.Event)
                .Include(er => er.Resource)
                .Where(er => er.EventId == eventId)
                .OrderBy(er => er.Resource.Name)
                .ToListAsync();
        }

        public async Task<EventResource?> GetEventResourceAsync(int eventId, int resourceId)
        {
            return await _context.EventResources
                .Include(er => er.Event)
                .Include(er => er.Resource)
                .FirstOrDefaultAsync(er => er.EventId == eventId && er.ResourceId == resourceId);
        }

        public async Task<bool> IsResourceAssignedAsync(int eventId, int resourceId)
        {
            return await _context.EventResources
                .AnyAsync(er => er.EventId == eventId && er.ResourceId == resourceId);
        }

        public async Task RemoveResourceFromEventAsync(int eventId, int resourceId)
        {
            var eventResource = await _context.EventResources
                .FirstOrDefaultAsync(er => er.EventId == eventId && er.ResourceId == resourceId);

            if (eventResource != null)
            {
                _context.EventResources.Remove(eventResource);
            }
        }

        public async Task<IEnumerable<EventResource>> GetEventsByResourceIdAsync(int resourceId)
        {
            return await _context.EventResources
                .Include(er => er.Event)
                .Include(er => er.Resource)
                .Where(er => er.ResourceId == resourceId)
                .OrderByDescending(er => er.Event.StartTime)
                .ToListAsync();
        }

        public async Task<int> GetTotalQuantityUsedForResourceAsync(int resourceId)
        {
            return await _context.EventResources
                .Where(er => er.ResourceId == resourceId)
                .SumAsync(er => er.QuantityUsed);
        }
    }
}
