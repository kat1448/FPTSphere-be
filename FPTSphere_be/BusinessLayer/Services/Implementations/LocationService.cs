using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Location;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LocationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<LocationDto>> GetLocationsAsync(
      int page, int pageSize, string? search, string? building,
      bool? isActive, string sortBy, bool sortDescending)
        {
            var allLocations = await _unitOfWork.Locations.GetAllAsync();

            var filtered = allLocations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                filtered = filtered.Where(l =>
                    l.Name.ToLower().Contains(s) ||
                    (l.Building != null && l.Building.ToLower().Contains(s)) ||
                    (l.RoomNumber != null && l.RoomNumber.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(building))
                filtered = filtered.Where(l => l.Building == building);

            if (isActive.HasValue)
                filtered = filtered.Where(l => l.IsActive == isActive.Value);

            var total = filtered.Count();

            // Apply sorting
            filtered = sortBy.ToLower() switch
            {
                "capacity" => sortDescending
                    ? filtered.OrderByDescending(l => l.Capacity)
                    : filtered.OrderBy(l => l.Capacity),
                "building" => sortDescending
                    ? filtered.OrderByDescending(l => l.Building)
                    : filtered.OrderBy(l => l.Building),
                "roomnumber" => sortDescending
                    ? filtered.OrderByDescending(l => l.RoomNumber)
                    : filtered.OrderBy(l => l.RoomNumber),
                _ => sortDescending
                    ? filtered.OrderByDescending(l => l.Name)
                    : filtered.OrderBy(l => l.Name)
            };

            // Apply pagination
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            return new PagedResult<LocationDto>
            {
                Data = _mapper.Map<List<LocationDto>>(items), 
                TotalRecords = total,
                Page = page,                                    
                PageSize = pageSize,
                TotalPages = totalPages                         
            };
        }

        public async Task<LocationDto?> GetByIdAsync(int id)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            return location == null ? null : _mapper.Map<LocationDto>(location);
        }

        public async Task<List<LocationDto>> GetActiveAsync()
        {
            var locations = await _unitOfWork.Locations.GetActiveLocationsAsync();
            return _mapper.Map<List<LocationDto>>(locations);
        }

        public async Task<List<LocationDto>> GetByBuildingAsync(string building)
        {
            var locations = await _unitOfWork.Locations.GetByBuildingAsync(building);
            return _mapper.Map<List<LocationDto>>(locations);
        }

        public async Task<List<LocationDto>> SearchAsync(string searchTerm)
        {
            var locations = await _unitOfWork.Locations.SearchAsync(searchTerm);
            return _mapper.Map<List<LocationDto>>(locations);
        }

        public async Task<LocationDto> CreateAsync(CreateLocationDto dto)
        {
            var location = _mapper.Map<Location>(dto);
            await _unitOfWork.Locations.AddAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<LocationDto>(location);
        }

        public async Task<LocationDto?> UpdateAsync(int id, UpdateLocationDto dto)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null) return null;

            _mapper.Map(dto, location);
            await _unitOfWork.Locations.UpdateAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<LocationDto>(location);
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null) return false;

            location.IsActive = !(location.IsActive ?? true);
            await _unitOfWork.Locations.UpdateAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return location.IsActive ?? true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null) return false;

            location.IsActive = false;
            await _unitOfWork.Locations.UpdateAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.Locations.NameExistsAsync(name, excludeId);
        }
        public async Task<List<LocationDto>> GetAvailableLocationsAsync(
             DateTime startTime,
             DateTime endTime,
             int? minCapacity = null,
             string? building = null)
        {
            var locations = await _unitOfWork.Locations.GetAvailableLocationsAsync(
                startTime, endTime, minCapacity, building);

            return _mapper.Map<List<LocationDto>>(locations);
        }
    }
}
