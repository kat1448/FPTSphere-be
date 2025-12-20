using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.EventTask;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using DataLayer.Repositories.Interfaces;
using System.Security.Claims;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services.Implementations
{
    public class EventTaskService : IEventTaskService
    {
        private readonly IEventTaskRepository _eventTaskRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public EventTaskService(IEventTaskRepository eventTaskRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _eventTaskRepository = eventTaskRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<EventTaskDto>> GetTasksAssignedToUserAsync(int userId)
        {
            var tasks = await _eventTaskRepository.GetByAssignedToAsync(userId);
            return _mapper.Map<List<EventTaskDto>>(tasks);
        }

        public async Task<EventTaskDto> UpdateTaskStatusAsync(int taskId, string status, ClaimsPrincipal user)
        {
            var task = await _eventTaskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new InvalidOperationException("Task không tồn tại");
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (task.AssignedTo != userId)
                throw new InvalidOperationException("Bạn không có quyền cập nhật task này");
            task.Status = status;
            await _eventTaskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventTaskDto>(task);
        }

        public async Task<EventTaskDto> UpdateTaskReportAsync(int taskId, string report, ClaimsPrincipal user)
        {
            var task = await _eventTaskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new InvalidOperationException("Task không tồn tại");
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (task.AssignedTo != userId)
                throw new InvalidOperationException("Bạn không có quyền cập nhật task này");
            task.Report = report;
            await _eventTaskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventTaskDto>(task);
        }

        public async Task<List<EventTaskWithEventDto>> GetTasksWithEventAssignedToUserAsync(int userId)
        {
            var tasks = await _eventTaskRepository.Query()
                .Include(x => x.Event)
                .Where(x => x.AssignedTo == userId)
                .ToListAsync();
            var result = new List<EventTaskWithEventDto>();
            foreach (var t in tasks)
            {
                var dto = _mapper.Map<EventTaskWithEventDto>(t);
                dto.Event = _mapper.Map<EventDto>(t.Event);
                result.Add(dto);
            }
            return result;
        }
    }
}
