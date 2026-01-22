using System;
using System.Collections.Generic;
using System.Linq;
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
            
            // Validate status values: chỉ cho phép "Chưa bắt đầu", "Đang làm", "Đã hoàn thành"
            var allowedStatuses = new[] { "Not Started", "In Progress", "Completed" };
            if (!allowedStatuses.Contains(status))
            {
                throw new InvalidOperationException($"Trạng thái không hợp lệ. Chỉ cho phép: {string.Join(", ", allowedStatuses)}");
            }
            
            task.Status = status;
            
            // Set CompletedAt nếu status là "Đã hoàn thành"
            if (status == "Completed")
            {
                task.CompletedAt = DateTime.Now;
            }
            else if (task.CompletedAt.HasValue && status != "Completed")
            {
                // Reset CompletedAt nếu chuyển từ "Đã hoàn thành" sang trạng thái khác
                task.CompletedAt = null;
            }
            
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

        public async Task<EventTaskDto> CreateTaskAsync(CreateEventTaskDto dto, ClaimsPrincipal user)
        {
            // Get current user ID (người giao task)
            var userIdString = user.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value 
                ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdString))
                throw new InvalidOperationException("Cannot detect current user");

            var currentUserId = int.Parse(userIdString);

            // Get current user to verify
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
                throw new InvalidOperationException("User not found");

            var currentUserRole = currentUser.Role?.RoleName ?? "";

            // Get assigned user to verify role
            var assignedUser = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo);
            if (assignedUser == null)
                throw new InvalidOperationException("Assigned user not found");

            var assignedUserRole = assignedUser.Role?.RoleName ?? "";

            // Validate role assignment permissions
            if (currentUserRole == "Event Manager")
            {
                // Manager can only assign to Staff
                if (assignedUserRole != "Staff")
                {
                    throw new UnauthorizedAccessException($"Event Manager can only assign tasks to Staff. Cannot assign to {assignedUserRole}.");
                }
            }
            else if (currentUserRole == "Director")
            {
                // Director can only assign to Event Manager
                if (assignedUserRole != "Event Manager")
                {
                    throw new UnauthorizedAccessException($"Director can only assign tasks to Event Manager. Cannot assign to {assignedUserRole}.");
                }
            }
            else if (currentUserRole == "Admin")
            {
                // Admin can only assign to Director
                if (assignedUserRole != "Director")
                {
                    throw new UnauthorizedAccessException($"Admin can only assign tasks to Director. Cannot assign to {assignedUserRole}.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException($"Role '{currentUserRole}' is not authorized to assign tasks.");
            }

            // Get event to verify it exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(dto.EventId);
            if (eventEntity == null)
                throw new InvalidOperationException("Event not found");

            // Create task entity
            var task = new EventTask
            {
                EventId = dto.EventId,
                AssignedTo = dto.AssignedTo,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status ?? "Not Started", // Default: Chưa bắt đầu
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                AssignBy = currentUserId, // Người giao task = user hiện tại
                IsTemplate = dto.IsTemplate ?? false,
                ParentTaskId = dto.ParentTaskId
            };

            await _eventTaskRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Load task with related entities for mapping
            var createdTask = await _eventTaskRepository.Query()
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.AssignByNavigation)
                .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

            return _mapper.Map<EventTaskDto>(createdTask);
        }

        public async Task<List<EventTaskDto>> GetTasksByEventIdAsync(int eventId)
        {
            // Verify event exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException("Event not found");

            var tasks = await _eventTaskRepository.Query()
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.AssignByNavigation)
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            return _mapper.Map<List<EventTaskDto>>(tasks);
        }

        public async Task<List<EventTaskDto>> GetTasksAssignedByUserAsync(int userId)
        {
            var tasks = await _eventTaskRepository.Query()
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.AssignByNavigation)
                .Where(t => t.AssignBy == userId)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            return _mapper.Map<List<EventTaskDto>>(tasks);
        }

        public async Task<EventTaskDto> UpdateTaskAsync(int taskId, UpdateEventTaskDto dto, ClaimsPrincipal user)
        {
            var task = await _eventTaskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new InvalidOperationException("Task không tồn tại");

            // Get current user ID
            var userIdString = user.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value 
                ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdString))
                throw new InvalidOperationException("Cannot detect current user");

            var currentUserId = int.Parse(userIdString);

            // Only the person who assigned the task can update it
            if (task.AssignBy != currentUserId)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật task này. Chỉ người giao task mới có thể cập nhật.");

            // Update fields if provided
            if (dto.AssignedTo.HasValue)
            {
                // Verify assigned user exists and has correct role
                var assignedUser = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo.Value);
                if (assignedUser == null)
                    throw new InvalidOperationException("Assigned user not found");

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                var currentUserRole = currentUser?.Role?.RoleName ?? "";
                var assignedUserRole = assignedUser.Role?.RoleName ?? "";

                // Validate role assignment permissions (same as CreateTask)
                if (currentUserRole == "Event Manager")
                {
                    if (assignedUserRole != "Staff")
                        throw new UnauthorizedAccessException($"Event Manager can only assign tasks to Staff. Cannot assign to {assignedUserRole}.");
                }
                else if (currentUserRole == "Director")
                {
                    if (assignedUserRole != "Event Manager")
                        throw new UnauthorizedAccessException($"Director can only assign tasks to Event Manager. Cannot assign to {assignedUserRole}.");
                }
                else if (currentUserRole == "Admin")
                {
                    if (assignedUserRole != "Director")
                        throw new UnauthorizedAccessException($"Admin can only assign tasks to Director. Cannot assign to {assignedUserRole}.");
                }

                task.AssignedTo = dto.AssignedTo.Value;
            }

            if (!string.IsNullOrEmpty(dto.Title))
                task.Title = dto.Title;

            if (dto.Description != null)
                task.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                var allowedStatuses = new[] { "Not Started", "In Progress", "Completed" };
                if (!allowedStatuses.Contains(dto.Status))
                    throw new InvalidOperationException($"Trạng thái không hợp lệ. Chỉ cho phép: {string.Join(", ", allowedStatuses)}");
                task.Status = dto.Status;

                // Handle CompletedAt
                if (dto.Status == "Completed")
                {
                    task.CompletedAt = DateTime.Now;
                }
                else if (task.CompletedAt.HasValue && dto.Status != "Completed")
                {
                    task.CompletedAt = null;
                }
            }

            if (dto.StartDate.HasValue)
                task.StartDate = dto.StartDate.Value;

            if (dto.DueDate.HasValue)
                task.DueDate = dto.DueDate.Value;

            if (dto.IsTemplate.HasValue)
                task.IsTemplate = dto.IsTemplate.Value;

            if (dto.ParentTaskId.HasValue)
                task.ParentTaskId = dto.ParentTaskId.Value;

            await _eventTaskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Reload with related entities
            var updatedTask = await _eventTaskRepository.Query()
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.AssignByNavigation)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            return _mapper.Map<EventTaskDto>(updatedTask);
        }

        public async Task<bool> DeleteTaskAsync(int taskId, ClaimsPrincipal user)
        {
            var task = await _eventTaskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new InvalidOperationException("Task không tồn tại");

            // Get current user ID
            var userIdString = user.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value 
                ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdString))
                throw new InvalidOperationException("Cannot detect current user");

            var currentUserId = int.Parse(userIdString);

            // Only the person who assigned the task can delete it
            if (task.AssignBy != currentUserId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa task này. Chỉ người giao task mới có thể xóa.");

            await _eventTaskRepository.DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
