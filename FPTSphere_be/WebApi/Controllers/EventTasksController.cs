using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;
using WebApi.Models.Requests;
using BusinessLayer.DTOs.EventTask;
using BusinessLayer.DTOs;
using DataLayer.Repositories.Interfaces;
using BusinessLayer.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTasksController : ControllerBase
    {
        private readonly IEventTaskService _eventTaskService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public EventTasksController(
            IEventTaskService eventTaskService,
            IUserService userService,
            IUnitOfWork unitOfWork)
        {
            _eventTaskService = eventTaskService;
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("my-tasks")]
        [Authorize]
        public async Task<IActionResult> GetMyTasks()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));
            var userId = int.Parse(userIdString);
            var tasks = await _eventTaskService.GetTasksWithEventAssignedToUserAsync(userId);
            return Ok(ApiResponse<List<EventTaskWithEventDto>>.SuccessResult(tasks, "Danh sách task được giao"));
        }

        [HttpPut("{taskId}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusDto request)
        {
            try
            {
                var updated = await _eventTaskService.UpdateTaskStatusAsync(taskId, request.Status, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(updated, "Cập nhật trạng thái thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{taskId}/report")]
        [Authorize]
        public async Task<IActionResult> UpdateTaskReport(int taskId, [FromBody] UpdateTaskReportDto request)
        {
            try
            {
                var updated = await _eventTaskService.UpdateTaskReportAsync(taskId, request.Report, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(updated, "Cập nhật báo cáo thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

        // GET TASKS BY EVENT ID
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> GetTasksByEventId(int eventId)
        {
            try
            {
                var tasks = await _eventTaskService.GetTasksByEventIdAsync(eventId);
                return Ok(ApiResponse<List<EventTaskDto>>.SuccessResult(
                    tasks,
                    $"Retrieved {tasks.Count} tasks for event {eventId}"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // CREATE TASK
        [HttpPost]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> CreateTask([FromBody] CreateEventTaskDto dto)
        {
            try
            {
                var created = await _eventTaskService.CreateTaskAsync(dto, User);
                return CreatedAtAction(
                    nameof(GetTasksByEventId),
                    new { eventId = created.EventId },
                    ApiResponse<EventTaskDto>.SuccessResult(created, "Task created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // GET USERS BY ROLE (for assign task dropdown)
        // Auto-detects role based on current user:
        // - Admin → Director
        // - Director → Event Manager
        // - Event Manager → Staff
        [HttpGet("users-by-role")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> GetUsersByRole()
        {
            try
            {
                // Get current user to determine which roles to show
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var currentUserId = int.Parse(userIdString);
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    return Unauthorized(ApiResponse<object>.ErrorResult("User not found"));

                var currentUserRole = currentUser.Role?.RoleName ?? "";

                // Auto-determine target role based on current user role
                string targetRoleName;
                if (currentUserRole == "Admin")
                {
                    targetRoleName = "Director"; // Admin can assign to Directors
                }
                else if (currentUserRole == "Director")
                {
                    targetRoleName = "Event Manager"; // Director can assign to Managers
                }
                else if (currentUserRole == "Event Manager")
                {
                    targetRoleName = "Staff"; // Manager can assign to Staff
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResult($"Invalid role '{currentUserRole}' for task assignment. Only Admin, Director, and Event Manager can assign tasks."));
                }

                // Get role by name
                var role = await _unitOfWork.SystemRoles.GetRoleByNameAsync(targetRoleName);
                if (role == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Role '{targetRoleName}' not found"));

                // Get users by role (only authorized users)
                var users = await _userService.GetUsersByRoleAsync(role.RoleId);
                return Ok(ApiResponse<List<BusinessLayer.DTOs.UserMinimalResponse>>.SuccessResult(
                    users,
                    $"Retrieved {users.Count} users with role '{targetRoleName}'"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // GET TASKS ASSIGNED BY CURRENT USER (Manager xem task đã giao)
        [HttpGet("my-assigned-tasks")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> GetMyAssignedTasks()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                var tasks = await _eventTaskService.GetTasksAssignedByUserAsync(userId);
                return Ok(ApiResponse<List<EventTaskDto>>.SuccessResult(
                    tasks,
                    $"Retrieved {tasks.Count} tasks assigned by you"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // UPDATE TASK (Manager update task đã giao)
        [HttpPut("{taskId}")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateEventTaskDto dto)
        {
            try
            {
                var updated = await _eventTaskService.UpdateTaskAsync(taskId, dto, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(updated, "Task updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // DELETE TASK (Manager delete task đã giao)
        [HttpDelete("{taskId}")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var result = await _eventTaskService.DeleteTaskAsync(taskId, User);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult("Task not found"));

                return Ok(ApiResponse<object>.SuccessResult(null, "Task deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}
