using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IEmailService _emailService;

        public EventsController(IEventService eventService) => _eventService = eventService;

        // ==================== MAIN EVENT ENDPOINTS ====================

        [HttpGet]
        public async Task<IActionResult> GetEvents(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] int? statusId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
            [FromQuery] int? locationId = null, [FromQuery] int? externalLocationId = null, [FromQuery] int? createdBy = null,
            [FromQuery] int? minAttendees = null, [FromQuery] int? maxAttendees = null,
            [FromQuery] decimal? minCost = null, [FromQuery] decimal? maxCost = null,
            [FromQuery] bool includeDeleted = false, [FromQuery] string sortBy = "CreatedAt", [FromQuery] bool sortDescending = true)
        {
            try
            {
                var filter = new EventFilterDto
                {
                    StatusId = statusId,
                    StartDate = startDate,
                    EndDate = endDate,
                    LocationId = locationId,
                    ExternalLocationId = externalLocationId,
                    CreatedBy = createdBy,
                    MinAttendees = minAttendees,
                    MaxAttendees = maxAttendees,
                    MinCost = minCost,
                    MaxCost = maxCost,
                    IncludeDeleted = includeDeleted
                };

                var result = await _eventService.GetEventsAsync(page, pageSize, filter, sortBy, sortDescending);
                return Ok(ApiResponse<PagedResult<EventDto>>.SuccessResult(result, $"Retrieved {result.TotalRecords} events"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            try
            {
                var ev = await _eventService.GetEventByIdAsync(id);
                if (ev == null) return NotFound(ApiResponse<object>.ErrorResult("Event not found"));
                return Ok(ApiResponse<EventDto>.SuccessResult(ev, "Event retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.CreateAsync(dto, userId);

                return CreatedAtAction(nameof(GetEventById), new { id = result.EventId },
                    ApiResponse<EventDto>.SuccessResult(result, "Event created"));
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.UpdateAsync(id, dto, userId);

                if (result == null) return NotFound(ApiResponse<object>.ErrorResult("Event not found"));
                return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event updated"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.DeleteAsync(id, userId);

                if (!result) return NotFound(ApiResponse<object>.ErrorResult("Event not found"));
                return Ok(ApiResponse<object>.SuccessResult(null, "Event deleted"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

        // ==================== SUB-EVENTS ENDPOINTS ⭐ NEW ====================

        /// <summary>
        /// Get all sub-events of a main event
        /// </summary>
        [HttpGet("{id}/subevents")]
        public async Task<IActionResult> GetSubEvents(int id)
        {
            try
            {
                var result = await _eventService.GetSubEventsAsync(id);
                return Ok(ApiResponse<List<SubEventDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count} sub-events"));
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

        /// <summary>
        /// Create a new sub-event under a main event
        /// </summary>
        [HttpPost("{id}/subevents")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> CreateSubEvent(int id, [FromBody] CreateSubEventDto dto)
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.CreateSubEventAsync(id, dto, userId);

                return CreatedAtAction(
                    nameof(GetEventById),
                    new { id = result.EventId },
                    ApiResponse<SubEventDto>.SuccessResult(result, "Sub-event created successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

        /// <summary>
        /// Update a sub-event
        /// </summary>
        [HttpPut("subevents/{subEventId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> UpdateSubEvent(int subEventId, [FromBody] UpdateSubEventDto dto)
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.UpdateSubEventAsync(subEventId, dto, userId);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Sub-event not found"));

                return Ok(ApiResponse<SubEventDto>.SuccessResult(result, "Sub-event updated successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

        /// <summary>
        /// Delete a sub-event (soft delete)
        /// </summary>
        [HttpDelete("subevents/{subEventId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> DeleteSubEvent(int subEventId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.DeleteSubEventAsync(subEventId, userId);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult("Sub-event not found"));

                return Ok(ApiResponse<object>.SuccessResult(null, "Sub-event deleted successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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
        [HttpGet("my")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> GetMyEvents(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? statusId = null,
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null,
    [FromQuery] int? locationId = null,
    [FromQuery] int? externalLocationId = null,
    [FromQuery] int? minAttendees = null,
    [FromQuery] int? maxAttendees = null,
    [FromQuery] decimal? minCost = null,
    [FromQuery] decimal? maxCost = null,
    [FromQuery] bool includeDeleted = false,
    [FromQuery] string sortBy = "CreatedAt",
    [FromQuery] bool sortDescending = true)
        {
            try
            {
                // 1. Lấy user id từ token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));
                }

                var currentUserId = int.Parse(userIdString);

                // 2. Tạo filter, ÉP theo EM hiện tại + chỉ main event
                var filter = new EventFilterDto
                {
                    StatusId = statusId,
                    StartDate = startDate,
                    EndDate = endDate,
                    LocationId = locationId,
                    ExternalLocationId = externalLocationId,
                    CreatedBy = currentUserId,   // chỉ event do user này tạo
                    MinAttendees = minAttendees,
                    MaxAttendees = maxAttendees,
                    MinCost = minCost,
                    MaxCost = maxCost,
                    IncludeDeleted = includeDeleted,

                    // ⭐ QUAN TRỌNG: chỉ lấy main event, loại sub-event
                    OnlyMainEvents = true
                };

                // 3. Gọi service như bình thường
                var result = await _eventService.GetEventsAsync(page, pageSize, filter, sortBy, sortDescending);

                return Ok(ApiResponse<PagedResult<EventDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.TotalRecords} events for current Event Manager"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        // ==================== OVERVIEW CHO EM HIỆN TẠI ====================
        [HttpGet("my/overview")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> GetMyOverview()
        {
            try
            {
                // Lấy UserId từ JWT
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));
                }

                var currentUserId = int.Parse(userIdString);

                // Gọi service lấy thống kê
                var stats = await _eventService.GetMyEventStatisticsAsync(currentUserId);

                return Ok(ApiResponse<EventStatistics>.SuccessResult(
                    stats,
                    "Overview statistics retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        // ==================== APPROVAL ENDPOINTS ====================

        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.SubmitForApprovalAsync(id, userId);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Event not found"));

                return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event submitted for approval"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Không đủ quyền hoặc sai trạng thái
                return Forbid(ex.Message);
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
        /// <summary>
        /// Send invitations for an approved event
        /// </summary>
        [HttpPost("{id}/invitations")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> SendInvitations(int id, [FromBody] SendEventInvitationsDto dto)
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _eventService.SendInvitationsAsync(id, userId, dto);

                return Ok(ApiResponse<List<EventInvitationDto>>.SuccessResult(
                    result,
                    $"Sent {result.Count} invitation(s) successfully"));
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
        [HttpPost("test-email")]
        [AllowAnonymous] // Chỉ để test, sau đó xóa hoặc đổi thành Authorize
        public async Task<IActionResult> TestEmail([FromQuery] string toEmail)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    toEmail,
                    "🎉 Test Email từ FPTSphere",
                    @"<html>
                <body style='font-family: Arial;'>
                    <h1 style='color: #F37021;'>Xin chào!</h1>
                    <p>Đây là email test từ hệ thống <strong>FPTSphere</strong>.</p>
                    <p>Nếu bạn nhận được email này, nghĩa là cấu hình SMTP đã hoạt động! ✅</p>
                </body>
            </html>",
                    "Test User"
                );

                return Ok(ApiResponse<object>.SuccessResult(null, $"Email sent to {toEmail}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to send email: {ex.Message}"));
            }
        }
    }
}