using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventStatusRepository : IRepository<EventStatus>
    {
        Task<EventStatus?> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> IsInUseAsync(int statusId);
    }
}
