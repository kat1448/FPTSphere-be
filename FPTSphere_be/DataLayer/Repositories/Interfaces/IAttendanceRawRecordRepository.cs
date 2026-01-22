using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IAttendanceRawRecordRepository : IRepository<AttendanceRawRecord>
    {
        Task<List<AttendanceRawRecord>> GetByEventAsync(int eventId);
        Task<List<AttendanceRawRecord>> GetBySubEventAsync(int subEventId);
        Task<List<AttendanceRawRecord>> GetByEventWithUserAsync(int eventId);
        Task<List<AttendanceRawRecord>> GetBySubEventWithUserAsync(int subEventId);
        Task<AttendanceRawRecord?> GetByEventAndRowAsync(int eventId, int row);
    }
}

