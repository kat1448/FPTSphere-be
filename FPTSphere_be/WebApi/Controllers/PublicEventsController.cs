using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    /// <summary>
    /// Public Events API - No authentication required
    /// Returns ONLY Approved main events that are Ongoing or Upcoming
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicEventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public PublicEventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// GET: api/publicevents
        /// Get all public events (list view - no sub-events)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PublicEventDto>>), 200)]
        public async Task<IActionResult> GetPublicEvents()
        {
            try
            {
                var events = await _eventService.GetPublicEventsAsync();

                return Ok(ApiResponse<List<PublicEventDto>>.SuccessResult(
                    events,
                    $"Retrieved {events.Count} public events"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// ⭐ GET: api/publicevents/{id}
        /// Get public event by ID WITH sub-events
        /// OPTIMIZED: Single query loads everything!
        /// Used for EVENT DETAIL PAGE
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PublicEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetPublicEventById(int id)
        {
            try
            {
                // ⭐ Get event WITH sub-events
                var eventDetail = await _eventService.GetPublicEventWithSubEventsAsync(id);

                if (eventDetail == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult(
                        "Event not found or not available for public viewing"
                    ));
                }

                return Ok(ApiResponse<PublicEventDto>.SuccessResult(
                    eventDetail,
                    $"Event retrieved with {eventDetail.SubEventCount} sub-events"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// GET: api/publicevents/upcoming
        /// Get ONLY upcoming events
        /// </summary>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(ApiResponse<List<PublicEventDto>>), 200)]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            try
            {
                var allEvents = await _eventService.GetPublicEventsAsync();
                var upcomingEvents = allEvents.Where(e => e.IsUpcoming).ToList();

                return Ok(ApiResponse<List<PublicEventDto>>.SuccessResult(
                    upcomingEvents,
                    $"Retrieved {upcomingEvents.Count} upcoming events"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// GET: api/publicevents/ongoing
        /// Get ONLY ongoing events
        /// </summary>
        [HttpGet("ongoing")]
        [ProducesResponseType(typeof(ApiResponse<List<PublicEventDto>>), 200)]
        public async Task<IActionResult> GetOngoingEvents()
        {
            try
            {
                var allEvents = await _eventService.GetPublicEventsAsync();
                var ongoingEvents = allEvents.Where(e => e.IsOngoing).ToList();

                return Ok(ApiResponse<List<PublicEventDto>>.SuccessResult(
                    ongoingEvents,
                    $"Retrieved {ongoingEvents.Count} ongoing events"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}