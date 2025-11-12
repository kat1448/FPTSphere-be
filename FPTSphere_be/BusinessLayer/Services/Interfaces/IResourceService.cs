using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.Resource;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services.Interfaces
{
    public interface IResourceService
    {
        Task<PagedResult<ResourceDto>> GetResourcesAsync(
            int page, int pageSize, string? search, string? type,
            bool? isActive, string sortBy, bool sortDescending);
        Task<ResourceDto?> GetByIdAsync(int id);
        Task<List<ResourceDto>> GetActiveAsync();
        Task<List<ResourceDto>> GetByTypeAsync(string type);
        Task<List<ResourceDto>> SearchAsync(string searchTerm);
        Task<ResourceDto> CreateAsync(CreateResourceDto dto);
        Task<ResourceDto?> UpdateAsync(int id, UpdateResourceDto dto);
        Task<bool> ToggleStatusAsync(int id);
        Task<(bool success, string message)> HardDeleteAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetAvailableQuantityAsync(int id);
    }
}
