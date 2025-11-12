using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IResourceRepository : IRepository<Resource>
    {
        Task<List<Resource>> GetActiveResourcesAsync();
        Task<List<Resource>> GetByTypeAsync(string type);
        Task<List<Resource>> SearchAsync(string searchTerm);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> IsResourceInUseAsync(int resourceId);
        Task<int> GetTotalAllocatedQuantityAsync(int resourceId);
    }
}
