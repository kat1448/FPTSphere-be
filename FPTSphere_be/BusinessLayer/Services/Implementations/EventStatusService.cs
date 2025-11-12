using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.EventStatus;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class EventStatusService : IEventStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventStatusService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EventStatusDto>> GetAllAsync()
        {
            var statuses = await _unitOfWork.EventStatuses.GetAllAsync();
            return _mapper.Map<List<EventStatusDto>>(statuses.OrderBy(s => s.StatusId).ToList());
        }

        public async Task<EventStatusDto?> GetByIdAsync(int id)
        {
            var status = await _unitOfWork.EventStatuses.GetByIdAsync(id);
            return status == null ? null : _mapper.Map<EventStatusDto>(status);
        }

        public async Task<EventStatusDto?> GetByNameAsync(string name)
        {
            var status = await _unitOfWork.EventStatuses.GetByNameAsync(name);
            return status == null ? null : _mapper.Map<EventStatusDto>(status);
        }

        public async Task<EventStatusDto> CreateAsync(CreateEventStatusDto dto)
        {
            var status = _mapper.Map<EventStatus>(dto);
            await _unitOfWork.EventStatuses.AddAsync(status);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventStatusDto>(status);
        }

        public async Task<EventStatusDto?> UpdateAsync(int id, UpdateEventStatusDto dto)
        {
            var status = await _unitOfWork.EventStatuses.GetByIdAsync(id);
            if (status == null) return null;

            status.StatusName = dto.StatusName;
            await _unitOfWork.EventStatuses.UpdateAsync(status);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventStatusDto>(status);
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var status = await _unitOfWork.EventStatuses.GetByIdAsync(id);
            if (status == null)
                return (false, "Event status not found");

            var isInUse = await _unitOfWork.EventStatuses.IsInUseAsync(id);
            if (isInUse)
                return (false, "Cannot delete. Status is being used by events");

            await _unitOfWork.EventStatuses.DeleteAsync(status);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Event status deleted successfully");
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.EventStatuses.NameExistsAsync(name, excludeId);
        }
    }
}
