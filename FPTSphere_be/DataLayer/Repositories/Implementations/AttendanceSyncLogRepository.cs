using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class AttendanceSyncLogRepository : Repository<AttendanceSyncLog>, IAttendanceSyncLogRepository
    {
        public AttendanceSyncLogRepository(EventDbContext context) : base(context) { }

        public async Task<List<AttendanceSyncLog>> GetByEventAsync(int eventId)
        {
            return await _dbSet
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.SyncedAt)
                .ToListAsync();
        }

        public async Task<List<AttendanceSyncLog>> GetBySubEventAsync(int subEventId)
        {
            return await _dbSet
                .Where(x => x.SubEventId == subEventId)
                .OrderByDescending(x => x.SyncedAt)
                .ToListAsync();
        }

        public async Task<AttendanceSyncLog?> GetByEventAndRowIndexAsync(int eventId, int rowIndex)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.SheetRowIndex == rowIndex);
        }

        public async Task<List<AttendanceSyncLog>> GetByEventAndSourceAsync(int eventId, string source)
        {
            return await _dbSet
                .Where(x => x.EventId == eventId && x.Source == source)
                .OrderByDescending(x => x.SyncedAt)
                .ToListAsync();
        }
    }
}

