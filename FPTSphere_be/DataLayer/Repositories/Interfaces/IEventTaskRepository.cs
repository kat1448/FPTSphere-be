using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventTaskRepository : IRepository<EventTask>
    {
        Task<List<EventTask>> GetByAssignedToAsync(int userId);
        IQueryable<EventTask> Query();
    }
}
