using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class EventTaskRepository : Repository<EventTask>, IEventTaskRepository
    {
        public EventTaskRepository(EventDbContext context) : base(context) { }

        public async Task<List<EventTask>> GetByAssignedToAsync(int userId)
        {
            return await _dbSet
                .Where(x => x.AssignedTo.HasValue && x.AssignedTo.Value == userId)
                .ToListAsync();
        }


        public IQueryable<EventTask> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
