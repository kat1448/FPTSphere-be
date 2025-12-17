using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Models.Requests;
using BusinessLayer.DTOs.EventTask;
using BusinessLayer.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTasksController : ControllerBase
    {
        private readonly IEventTaskService _eventTaskService;

        public EventTasksController(IEventTaskService eventTaskService)
        {
            _eventTaskService = eventTaskService;
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
            return Ok(ApiResponse<List<EventTaskWithEventDto>>.SuccessResult(tasks, "Assigned tasks"));
        }

        [HttpPut("{taskId:int}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusDto request)
        {
            try
            {
                var updated = await _eventTaskService.UpdateTaskStatusAsync(taskId, request.Status, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(updated, "Status updated"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{taskId:int}/report")]
        [Authorize]
        public async Task<IActionResult> UpdateTaskReport(int taskId, [FromBody] UpdateTaskReportDto request)
        {
            try
            {
                var updated = await _eventTaskService.UpdateTaskReportAsync(taskId, request.Report, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(updated, "Report updated"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

        // =========================
        // TASK TEMPLATES (Event Manager creates before approval)
        // =========================

        [HttpPost("templates")]
        [Authorize(Roles = "Event Manager,Admin")]
        public async Task<IActionResult> CreateTemplate([FromQuery] int eventId, [FromBody] CreateTaskTemplateDto dto)
        {
            try
            {
                var created = await _eventTaskService.CreateTaskTemplateForEventAsync(eventId, dto, User);
                return Ok(ApiResponse<EventTaskDto>.SuccessResult(created, "Template created"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("templates")]
        [Authorize(Roles = "Event Manager,Admin,Director")]
        public async Task<IActionResult> GetTemplates([FromQuery] int eventId)
        {
            try
            {
                var list = await _eventTaskService.GetTaskTemplatesByEventIdAsync(eventId, User);
                return Ok(ApiResponse<List<EventTaskDto>>.SuccessResult(list, "Templates"));
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }
        [Authorize(Roles = "Event Manager,Admin")]
        [HttpPost("{parentTaskId}/sub-tasks")]
        public async Task<IActionResult> CreateSubTask(int parentTaskId, [FromBody] CreateSubTaskDto dto)
        {
            try
            {
                var result = await _eventTaskService
                    .CreateSubTaskAsync(parentTaskId, dto, User);

                return Ok(ApiResponse<EventTaskDto>
                    .SuccessResult(result, "Sub task created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }

    }
}
