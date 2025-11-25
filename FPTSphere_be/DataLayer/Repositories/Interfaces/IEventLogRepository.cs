using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventLogRepository : IRepository<EventLog>
    {
        Task<List<EventLog>> GetLogsByEventIdAsync(int eventId);
    }
}
