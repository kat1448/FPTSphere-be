using BusinessLayer.DTOs.ExternalService;
using BusinessLayer.DTOs.ExternalServices;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExternalServicesController : ControllerBase
    {
        private readonly IExternalServiceService _externalServiceService;

        public ExternalServicesController(IExternalServiceService externalServiceService)
        {
            _externalServiceService = externalServiceService;
        }

        /// <summary>
        /// Get all external services for a specific event
        /// </summary>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEventId(int eventId)
        {
            try
            {
                var services = await _externalServiceService.GetByEventIdAsync(eventId);
                return Ok(ApiResponse<IEnumerable<ExternalServiceDto>>.SuccessResult(
                    services,
                    $"Retrieved {services.Count()} external services"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get external service by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var service = await _externalServiceService.GetByIdAsync(id);
                if (service == null)
                    return NotFound(ApiResponse<object>.ErrorResult("External service not found"));

                return Ok(ApiResponse<ExternalServiceDto>.SuccessResult(service, "Service retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Add external service to an event
        /// </summary>
        [HttpPost("event/{eventId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateExternalServiceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid data", errors));
                }

                var result = await _externalServiceService.CreateAsync(eventId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.ServiceId },
                    ApiResponse<ExternalServiceDto>.SuccessResult(result, "External service added successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update external service
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExternalServiceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid data", errors));
                }

                var result = await _externalServiceService.UpdateAsync(id, dto);
                return Ok(ApiResponse<ExternalServiceDto>.SuccessResult(result, "Service updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete external service
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _externalServiceService.DeleteAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Service deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get event cost breakdown including all external costs
        /// </summary>
        [HttpGet("event/{eventId}/cost-breakdown")]
        public async Task<IActionResult> GetCostBreakdown(int eventId)
        {
            try
            {
                var breakdown = await _externalServiceService.GetEventCostBreakdownAsync(eventId);
                return Ok(ApiResponse<EventCostBreakdownDto>.SuccessResult(breakdown, "Cost breakdown retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search external services by provider name
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchByProvider([FromQuery] string providerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                    return BadRequest(ApiResponse<object>.ErrorResult("Provider name is required"));

                var services = await _externalServiceService.SearchByProviderAsync(providerName);
                return Ok(ApiResponse<IEnumerable<ExternalServiceDto>>.SuccessResult(
                    services,
                    $"Found {services.Count()} services"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}
