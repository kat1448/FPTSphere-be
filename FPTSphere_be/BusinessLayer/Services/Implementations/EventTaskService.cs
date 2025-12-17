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
using BusinessLayer.Helpers;

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
            if (!task.AssignedTo.HasValue || task.AssignedTo.Value != userId)
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
            if (!task.AssignedTo.HasValue || task.AssignedTo.Value != userId)
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
                .Where(x => x.AssignedTo.HasValue && x.AssignedTo.Value == userId)
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
        public async Task<EventTaskDto> CreateTaskTemplateForEventAsync(int eventId, CreateTaskTemplateDto dto, ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (role != "Event Manager" && role != "Admin")
                throw new InvalidOperationException("Access denied");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Title is required");

            if (dto.StartDate.HasValue && dto.DueDate.HasValue && dto.DueDate.Value < dto.StartDate.Value)
                throw new InvalidOperationException("DueDate must be greater than or equal to StartDate");

            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null || evt.IsDeleted == true)
                throw new InvalidOperationException("Event not found");

            var status = TaskStatusHelper.Normalize(dto.Status); // null -> "To Do"


            if (!dto.StartDate.HasValue)
                throw new InvalidOperationException("StartDate is required");

            if (!dto.DueDate.HasValue)
                throw new InvalidOperationException("DueDate is required");

            if (dto.DueDate.Value < dto.StartDate.Value)
                throw new InvalidOperationException("DueDate must be greater than or equal to StartDate");

            var task = new EventTask
            {
                EventId = eventId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Status = status,
                StartDate = dto.StartDate.Value,
                DueDate = dto.DueDate.Value,
                CompletedAt = null,
                Report = null,
                AssignedTo = null,
                ParentTaskId = null,
                IsTemplate = true
            };


            await _eventTaskRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventTaskDto>(task);
        }

        public async Task<List<EventTaskDto>> GetTaskTemplatesByEventIdAsync(int eventId, ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var roleName = user.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null || evt.IsDeleted == true)
                throw new InvalidOperationException("Event not found");

            if (evt.CreatedBy != userId
                && roleName != "Admin"
                && roleName != "Event Manager"
                && roleName != "Director")
                throw new InvalidOperationException("Access denied");

            var templates = await _eventTaskRepository.Query()
                .Where(t => t.EventId == eventId && t.IsTemplate && t.ParentTaskId == null)
                .OrderBy(t => t.TaskId)
                .ToListAsync();

            return _mapper.Map<List<EventTaskDto>>(templates);
        }
        public async Task<EventTaskDto> CreateSubTaskAsync(int parentTaskId, CreateSubTaskDto dto, ClaimsPrincipal user)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Event Manager" && role != "Admin")
                throw new InvalidOperationException("Access denied");

            // Lấy task cha
            var parentTask = await _eventTaskRepository.GetByIdAsync(parentTaskId);
            if (parentTask == null || parentTask.IsTemplate != true)
                throw new InvalidOperationException("Parent task not found or not a template");

            // Validate thời gian
            if (dto.StartDate < parentTask.StartDate ||
                dto.DueDate > parentTask.DueDate ||
                dto.DueDate < dto.StartDate)
                throw new InvalidOperationException(
                    "Subtask time must be within parent task time range");

            // Validate staff tồn tại
            var staff = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo);
            if (staff == null)
                throw new InvalidOperationException("Assigned staff not found");

            var subTask = new EventTask
            {
                EventId = parentTask.EventId,
                ParentTaskId = parentTask.TaskId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                Status = TaskStatusHelper.ToDo,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                IsTemplate = false,
                CompletedAt = null,
                Report = null
            };

            await _eventTaskRepository.AddAsync(subTask);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventTaskDto>(subTask);
        }



    }
}
