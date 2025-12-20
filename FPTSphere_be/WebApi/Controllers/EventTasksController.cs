using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
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
    }
}
