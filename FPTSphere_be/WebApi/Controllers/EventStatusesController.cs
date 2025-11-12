using BusinessLayer.DTOs.EventStatus;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventStatusesController : ControllerBase
    {
        private readonly IEventStatusService _eventStatusService;

        public EventStatusesController(IEventStatusService eventStatusService)
        {
            _eventStatusService = eventStatusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var statuses = await _eventStatusService.GetAllAsync();
                return Ok(ApiResponse<List<EventStatusDto>>.SuccessResult(
                    statuses, $"Retrieved {statuses.Count} event statuses"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var status = await _eventStatusService.GetByIdAsync(id);
                if (status == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Event status {id} not found"));

                return Ok(ApiResponse<EventStatusDto>.SuccessResult(status, "Event status retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                var status = await _eventStatusService.GetByNameAsync(name);
                if (status == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Event status '{name}' not found"));

                return Ok(ApiResponse<EventStatusDto>.SuccessResult(status, "Event status retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEventStatusDto dto)
        {
            try
            {
                if (await _eventStatusService.NameExistsAsync(dto.StatusName))
                    return BadRequest(ApiResponse<object>.ErrorResult("Event status name already exists"));

                var status = await _eventStatusService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = status.StatusId },
                    ApiResponse<EventStatusDto>.SuccessResult(status, "Event status created"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventStatusDto dto)
        {
            try
            {
                if (await _eventStatusService.NameExistsAsync(dto.StatusName, id))
                    return BadRequest(ApiResponse<object>.ErrorResult("Event status name already exists"));

                var status = await _eventStatusService.UpdateAsync(id, dto);
                if (status == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Event status {id} not found"));

                return Ok(ApiResponse<EventStatusDto>.SuccessResult(status, "Event status updated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _eventStatusService.DeleteAsync(id);
                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResult(message));

                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}
