using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<Event?> GetByIdWithDetailsAsync(int id);
        Task<List<Event>> GetAllWithDetailsAsync();
        Task<List<Event>> GetSubEventsByParentIdAsync(int parentEventId);
    }
}
