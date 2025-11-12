using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.ExternalLocation;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services.Interfaces
{
    public interface IExternalLocationService
    {
        Task<PagedResult<ExternalLocationDto>> GetExternalLocationsAsync(
            int page, int pageSize, string? search, decimal? minCost,
            decimal? maxCost, string sortBy, bool sortDescending);
        Task<ExternalLocationDto?> GetByIdAsync(int id);
        Task<List<ExternalLocationDto>> SearchAsync(string searchTerm);
        Task<List<ExternalLocationDto>> GetByCostRangeAsync(decimal minCost, decimal maxCost);
        Task<ExternalLocationDto> CreateAsync(CreateExternalLocationDto dto);
        Task<ExternalLocationDto?> UpdateAsync(int id, UpdateExternalLocationDto dto);
        Task<(bool success, string message)> DeleteAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
