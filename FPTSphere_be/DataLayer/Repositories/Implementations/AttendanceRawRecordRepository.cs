using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class AttendanceRawRecordRepository : Repository<AttendanceRawRecord>, IAttendanceRawRecordRepository
    {
        public AttendanceRawRecordRepository(EventDbContext context) : base(context) { }

        public async Task<List<AttendanceRawRecord>> GetByEventAsync(int eventId)
        {
            return await _dbSet
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.SheetRow)
                .ToListAsync();
        }

        public async Task<List<AttendanceRawRecord>> GetBySubEventAsync(int subEventId)
        {
            return await _dbSet
                .Where(x => x.SubEventId == subEventId)
                .OrderBy(x => x.SheetRow)
                .ToListAsync();
        }

        public async Task<List<AttendanceRawRecord>> GetByEventWithUserAsync(int eventId)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.SheetRow)
                .ToListAsync();
        }

        public async Task<List<AttendanceRawRecord>> GetBySubEventWithUserAsync(int subEventId)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.SubEventId == subEventId)
                .OrderBy(x => x.SheetRow)
                .ToListAsync();
        }

        public async Task<AttendanceRawRecord?> GetByEventAndRowAsync(int eventId, int row)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.SheetRow == row);
        }
    }
}

