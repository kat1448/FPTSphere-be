using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class EventAttendanceRepository : Repository<EventAttendance>, IEventAttendanceRepository
    {
        public EventAttendanceRepository(EventDbContext context) : base(context) { }

        public async Task<List<EventAttendance>> GetByEventAsync(int eventId)
        {
            return await _dbSet.Where(x => x.EventId == eventId).ToListAsync();
        }

        public async Task<List<EventAttendance>> GetByUserAsync(int userId)
        {
            return await _dbSet.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<List<EventAttendance>> GetByUserWithEventFullAsync(int userId)
        {
            return await _dbSet
                .Include(x => x.Event)
                    .ThenInclude(e => e.Location)
                .Include(x => x.Event)
                    .ThenInclude(e => e.Status)
                .Include(x => x.Event)
                    .ThenInclude(e => e.ExternalLocation)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
