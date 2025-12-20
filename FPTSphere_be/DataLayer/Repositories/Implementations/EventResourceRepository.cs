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
        public async Task<int> GetTotalQuantityUsedForResourceInRangeAsync(
            int resourceId,
            DateTime startTime,
            DateTime endTime,
            int[] blockingStatusIds,
            int? ignoreEventId = null)
        {
            var q = _context.EventResources
                .Where(er => er.ResourceId == resourceId)
                .Join(
                    _context.Events.Where(e => e.IsDeleted != true),
                    er => er.EventId,
                    e => e.EventId,
                    (er, e) => new { er, e }
                )
                // chỉ tính event đang giữ chỗ
                .Where(x => blockingStatusIds.Contains(x.e.StatusId))
                // overlap time
                .Where(x => x.e.StartTime < endTime && x.e.EndTime > startTime);

            if (ignoreEventId.HasValue)
                q = q.Where(x => x.e.EventId != ignoreEventId.Value);

            return await q.SumAsync(x => x.er.QuantityUsed);
        }



    }
}
