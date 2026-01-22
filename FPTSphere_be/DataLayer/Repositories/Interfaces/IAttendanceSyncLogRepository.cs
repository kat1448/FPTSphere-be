using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IAttendanceSyncLogRepository : IRepository<AttendanceSyncLog>
    {
        Task<List<AttendanceSyncLog>> GetByEventAsync(int eventId);
        Task<List<AttendanceSyncLog>> GetBySubEventAsync(int subEventId);
        Task<AttendanceSyncLog?> GetByEventAndRowIndexAsync(int eventId, int rowIndex);
        Task<List<AttendanceSyncLog>> GetByEventAndSourceAsync(int eventId, string source);
    }
}

