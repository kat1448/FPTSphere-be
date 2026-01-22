using BusinessLayer.DTOs;
using BusinessLayer.DTOs.EventType;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTypesController : ControllerBase
    {
        private readonly IEventTypeService _eventTypeService;

        public EventTypesController(IEventTypeService eventTypeService)
        {
            _eventTypeService = eventTypeService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTypes()
        {
            try
            {
                var result = await _eventTypeService.GetAllAsync();
                return Ok(ApiResponse<List<EventTypeDto>>.SuccessResult(result, $"Retrieved {result.Count} event types"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTypeById(int id)
        {
            try
            {
                var result = await _eventTypeService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Event type not found"));
                return Ok(ApiResponse<EventTypeDto>.SuccessResult(result, "Event type retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}

