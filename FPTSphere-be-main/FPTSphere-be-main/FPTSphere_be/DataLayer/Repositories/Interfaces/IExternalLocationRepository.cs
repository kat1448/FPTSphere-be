using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IExternalLocationRepository : IRepository<ExternalLocation>
    {
        Task<List<ExternalLocation>> SearchAsync(string searchTerm);
        Task<List<ExternalLocation>> GetByCostRangeAsync(decimal minCost, decimal maxCost);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> IsInUseAsync(int externalLocationId);
    }
}
