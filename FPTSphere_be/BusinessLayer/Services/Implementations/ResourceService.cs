using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Resource;
using BusinessLayer.DTOs;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class ResourceService : IResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ResourceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ResourceDto>> GetResourcesAsync(
            int page, int pageSize, string? search, string? type,
            bool? isActive, string sortBy, bool sortDescending)
        {
            var allResources = await _unitOfWork.Resources.GetAllAsync();
            var filtered = allResources.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                filtered = filtered.Where(r =>
                    r.Name.ToLower().Contains(s) ||
                    (r.Type != null && r.Type.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(type))
                filtered = filtered.Where(r => r.Type == type);

            if (isActive.HasValue)
                filtered = filtered.Where(r => r.IsActive == isActive.Value);

            var total = filtered.Count();

            filtered = sortBy.ToLower() switch
            {
                "quantity" => sortDescending
                    ? filtered.OrderByDescending(r => r.Quantity)
                    : filtered.OrderBy(r => r.Quantity),
                "type" => sortDescending
                    ? filtered.OrderByDescending(r => r.Type)
                    : filtered.OrderBy(r => r.Type),
                _ => sortDescending
                    ? filtered.OrderByDescending(r => r.Name)
                    : filtered.OrderBy(r => r.Name)
            };

            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            return new PagedResult<ResourceDto>
            {
                Data = _mapper.Map<List<ResourceDto>>(items),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ResourceDto?> GetByIdAsync(int id)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(id);
            return resource == null ? null : _mapper.Map<ResourceDto>(resource);
        }

        public async Task<List<ResourceDto>> GetActiveAsync()
        {
            var resources = await _unitOfWork.Resources.GetActiveResourcesAsync();
            return _mapper.Map<List<ResourceDto>>(resources);
        }

        public async Task<List<ResourceDto>> GetByTypeAsync(string type)
        {
            var resources = await _unitOfWork.Resources.GetByTypeAsync(type);
            return _mapper.Map<List<ResourceDto>>(resources);
        }

        public async Task<List<ResourceDto>> SearchAsync(string searchTerm)
        {
            var resources = await _unitOfWork.Resources.SearchAsync(searchTerm);
            return _mapper.Map<List<ResourceDto>>(resources);
        }

        public async Task<ResourceDto> CreateAsync(CreateResourceDto dto)
        {
            var resource = _mapper.Map<Resource>(dto);
            await _unitOfWork.Resources.AddAsync(resource);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ResourceDto>(resource);
        }

        public async Task<ResourceDto?> UpdateAsync(int id, UpdateResourceDto dto)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(id);
            if (resource == null) return null;

            resource.Name = dto.Name;
            resource.Type = dto.Type;
            resource.Quantity = dto.Quantity;
            resource.ImageUrl = dto.ImageUrl;

            await _unitOfWork.Resources.UpdateAsync(resource);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ResourceDto>(resource);
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(id);
            if (resource == null) return false;

            resource.IsActive = !(resource.IsActive ?? true);
            await _unitOfWork.Resources.UpdateAsync(resource);
            await _unitOfWork.SaveChangesAsync();
            return resource.IsActive ?? true;
        }

        public async Task<(bool success, string message)> HardDeleteAsync(int id)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(id);
            if (resource == null)
                return (false, "Resource not found");

            // Check if resource is being used in any events
            var isInUse = await _unitOfWork.Resources.IsResourceInUseAsync(id);
            if (isInUse)
            {
                var allocatedQty = await _unitOfWork.Resources.GetTotalAllocatedQuantityAsync(id);
                return (false, $"Cannot delete. Resource is allocated to events (total: {allocatedQty} units)");
            }

            // Hard delete - remove from database
            await _unitOfWork.Resources.DeleteAsync(resource);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Resource deleted permanently");
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.Resources.NameExistsAsync(name, excludeId);
        }

        public async Task<int> GetAvailableQuantityAsync(int id)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(id);
            if (resource == null) return 0;

            var allocatedQty = await _unitOfWork.Resources.GetTotalAllocatedQuantityAsync(id);
            return resource.Quantity - allocatedQty;
        }
    }
}
