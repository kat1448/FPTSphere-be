using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.Location;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services.Interfaces
{
    public interface ILocationService
    {
        Task<PagedResult<LocationDto>> GetLocationsAsync(
            int page, int pageSize, string? search, string? building,
            bool? isActive, string sortBy, bool sortDescending);
        Task<LocationDto?> GetByIdAsync(int id);
        Task<List<LocationDto>> GetActiveAsync();
        Task<List<LocationDto>> GetByBuildingAsync(string building);
        Task<List<LocationDto>> SearchAsync(string searchTerm);
        Task<LocationDto> CreateAsync(CreateLocationDto dto);
        Task<LocationDto?> UpdateAsync(int id, UpdateLocationDto dto);
        Task<bool> ToggleStatusAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<List<LocationDto>> GetAvailableLocationsAsync(
             DateTime startTime,
             DateTime endTime,
             int? minCapacity = null,
             string? building = null);
    }
}
