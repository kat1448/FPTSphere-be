using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<List<Location>> GetActiveLocationsAsync();
        Task<List<Location>> GetByBuildingAsync(string building);
        Task<List<Location>> SearchAsync(string searchTerm);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<List<Location>> GetAvailableLocationsAsync(
            DateTime startTime,
            DateTime endTime,
            int? minCapacity = null,
            string? building = null,
            int? ignoreEventId = null,
            int? ignoreParentEventId = null);
    }
}
