using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event entity);
        Task<List<Event>> GetAllAsync();
        Task<Event?> GetByIdAsync(int id);
    }
}
