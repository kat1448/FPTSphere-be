using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories.Implementations
{
    public class EventLogRepository : Repository<EventLog>, IEventLogRepository
    {
        public EventLogRepository(EventDbContext context) : base(context) { }

        public async Task<List<EventLog>> GetLogsByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Where(l => l.EventId == eventId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
