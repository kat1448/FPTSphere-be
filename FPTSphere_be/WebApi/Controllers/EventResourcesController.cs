using BusinessLayer.DTOs;
using BusinessLayer.DTOs.EventResource;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventResourcesController : ControllerBase
    {
        private readonly IEventResourceService _eventResourceService;

        public EventResourcesController(IEventResourceService eventResourceService)
        {
            _eventResourceService = eventResourceService;
        }

        /// <summary>
        /// Assign a resource to an event (works for main events and sub-events)
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="dto">Resource assignment details</param>
        /// <returns>Created resource assignment</returns>
        [HttpPost("{eventId}/resources")]
        [Authorize(Roles = "Admin,Event Manager")]
        [ProducesResponseType(typeof(ApiResponse<EventResourceResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> AssignResourceToEvent(int eventId, [FromBody] AssignResourceToEventDto dto)
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

                var result = await _eventResourceService.AssignResourceToEventAsync(eventId, dto);
                return CreatedAtAction(
                    nameof(GetEventResource),
                    new { eventId, resourceId = dto.ResourceId },
                    ApiResponse<EventResourceResponseDto>.SuccessResult(
                        result,
                        "Resource assigned to event successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all resources assigned to an event with summary
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Event resources summary</returns>
        [HttpGet("{eventId}/resources")]
        [ProducesResponseType(typeof(ApiResponse<EventResourcesSummaryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetEventResources(int eventId)
        {
            try
            {
                var result = await _eventResourceService.GetEventResourcesAsync(eventId);
                return Ok(ApiResponse<EventResourcesSummaryDto>.SuccessResult(
                    result,
                    $"Retrieved {result.TotalResourcesAssigned} resources for event"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get specific resource assignment for an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="resourceId">Resource ID</param>
        /// <returns>Resource assignment details</returns>
        [HttpGet("{eventId}/resources/{resourceId}")]
        [ProducesResponseType(typeof(ApiResponse<EventResourceResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetEventResource(int eventId, int resourceId)
        {
            try
            {
                var result = await _eventResourceService.GetEventResourceAsync(eventId, resourceId);
                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult(
                        "Resource assignment not found"));

                return Ok(ApiResponse<EventResourceResponseDto>.SuccessResult(
                    result,
                    "Resource assignment retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update resource assignment for an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="dto">Updated quantity</param>
        /// <returns>Updated resource assignment</returns>
        [HttpPut("{eventId}/resources/{resourceId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        [ProducesResponseType(typeof(ApiResponse<EventResourceResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateEventResource(
            int eventId,
            int resourceId,
            [FromBody] UpdateEventResourceDto dto)
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

                var result = await _eventResourceService.UpdateEventResourceAsync(eventId, resourceId, dto);
                return Ok(ApiResponse<EventResourceResponseDto>.SuccessResult(
                    result,
                    "Resource assignment updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove resource assignment from event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="resourceId">Resource ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{eventId}/resources/{resourceId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> RemoveResourceFromEvent(int eventId, int resourceId)
        {
            try
            {
                await _eventResourceService.RemoveResourceFromEventAsync(eventId, resourceId);
                return Ok(ApiResponse<bool>.SuccessResult(
                    true,
                    "Resource removed from event successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all events using a specific resource
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <returns>List of events using this resource</returns>
        [HttpGet("resources/{resourceId}/events")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResourceResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetEventsUsingResource(int resourceId)
        {
            try
            {
                var result = await _eventResourceService.GetEventsUsingResourceAsync(resourceId);
                return Ok(ApiResponse<IEnumerable<EventResourceResponseDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count()} events using this resource"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get current availability of a resource
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <returns>Available quantity</returns>
        [HttpGet("resources/{resourceId}/availability")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetResourceAvailability(int resourceId)
        {
            try
            {
                var available = await _eventResourceService.GetResourceAvailabilityAsync(resourceId);
                return Ok(ApiResponse<int>.SuccessResult(
                    available,
                    $"Available quantity: {available}"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }
}