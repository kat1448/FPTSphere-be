using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventAttendanceRepository : IRepository<EventAttendance>
    {
        Task<List<EventAttendance>> GetByEventAsync(int eventId);
        Task<List<EventAttendance>> GetByUserAsync(int userId);
        Task<List<EventAttendance>> GetByUserWithEventFullAsync(int userId);
    }
}
