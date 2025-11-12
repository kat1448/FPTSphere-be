using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.ExternalLocation;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class ExternalLocationService : IExternalLocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExternalLocationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ExternalLocationDto>> GetExternalLocationsAsync(
            int page, int pageSize, string? search, decimal? minCost,
            decimal? maxCost, string sortBy, bool sortDescending)
        {
            var allLocations = await _unitOfWork.ExternalLocations.GetAllAsync();
            var filtered = allLocations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                filtered = filtered.Where(el =>
                    el.Name.ToLower().Contains(s) ||
                    el.Address.ToLower().Contains(s) ||
                    (el.ContactPerson != null && el.ContactPerson.ToLower().Contains(s)));
            }

            if (minCost.HasValue)
                filtered = filtered.Where(el => el.Cost >= minCost.Value);

            if (maxCost.HasValue)
                filtered = filtered.Where(el => el.Cost <= maxCost.Value);

            var total = filtered.Count();

            filtered = sortBy.ToLower() switch
            {
                "cost" => sortDescending
                    ? filtered.OrderByDescending(el => el.Cost)
                    : filtered.OrderBy(el => el.Cost),
                "address" => sortDescending
                    ? filtered.OrderByDescending(el => el.Address)
                    : filtered.OrderBy(el => el.Address),
                _ => sortDescending
                    ? filtered.OrderByDescending(el => el.Name)
                    : filtered.OrderBy(el => el.Name)
            };

            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            return new PagedResult<ExternalLocationDto>
            {
                Data = _mapper.Map<List<ExternalLocationDto>>(items),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ExternalLocationDto?> GetByIdAsync(int id)
        {
            var location = await _unitOfWork.ExternalLocations.GetByIdAsync(id);
            return location == null ? null : _mapper.Map<ExternalLocationDto>(location);
        }

        public async Task<List<ExternalLocationDto>> SearchAsync(string searchTerm)
        {
            var locations = await _unitOfWork.ExternalLocations.SearchAsync(searchTerm);
            return _mapper.Map<List<ExternalLocationDto>>(locations);
        }

        public async Task<List<ExternalLocationDto>> GetByCostRangeAsync(decimal minCost, decimal maxCost)
        {
            var locations = await _unitOfWork.ExternalLocations.GetByCostRangeAsync(minCost, maxCost);
            return _mapper.Map<List<ExternalLocationDto>>(locations);
        }

        public async Task<ExternalLocationDto> CreateAsync(CreateExternalLocationDto dto)
        {
            var location = _mapper.Map<ExternalLocation>(dto);
            await _unitOfWork.ExternalLocations.AddAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ExternalLocationDto>(location);
        }

        public async Task<ExternalLocationDto?> UpdateAsync(int id, UpdateExternalLocationDto dto)
        {
            var location = await _unitOfWork.ExternalLocations.GetByIdAsync(id);
            if (location == null) return null;

            location.Name = dto.Name;
            location.Address = dto.Address;
            location.ContactPerson = dto.ContactPerson;
            location.ContactPhone = dto.ContactPhone;
            location.Cost = dto.Cost;
            location.Note = dto.Note;
            location.ImageUrl = dto.ImageUrl;

            await _unitOfWork.ExternalLocations.UpdateAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ExternalLocationDto>(location);
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var location = await _unitOfWork.ExternalLocations.GetByIdAsync(id);
            if (location == null)
                return (false, "External location not found");

            var isInUse = await _unitOfWork.ExternalLocations.IsInUseAsync(id);
            if (isInUse)
                return (false, "Cannot delete. External location is being used in events");

            await _unitOfWork.ExternalLocations.DeleteAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return (true, "External location deleted successfully");
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.ExternalLocations.NameExistsAsync(name, excludeId);
        }
    }
}
