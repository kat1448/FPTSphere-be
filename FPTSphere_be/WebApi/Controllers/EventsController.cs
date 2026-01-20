using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using BusinessLayer.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Old code:
    // [AllowAnonymous]
    // Fixed: bỏ AllowAnonymous ở cấp controller để các action [Authorize] (như CreateEvent) bắt buộc phải authenticate
    public class EventsController : ControllerBase

    {
        private readonly IEventService _eventService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly ILocationBookingService _locationBookingService;

        public EventsController(
            IEventService eventService,
            IFileService fileService,
            ILocationBookingService locationBookingService)
        {
            _eventService = eventService;
            _fileService = fileService;
            _locationBookingService = locationBookingService;
        }

        // ==================== MAIN EVENT ENDPOINTS ====================

        [HttpGet]
        [Authorize(Roles = "Admin,Event Manager,Director, Staff")]
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
        [Authorize(Roles = "Admin,Event Manager,Director, Staff")]
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

        /// <summary>
        /// Dùng để hiển thị thông tin phòng đã được book trong UI chọn location.
        /// </summary>
        [HttpGet("location-bookings")]
        [Authorize(Roles = "Admin,Event Manager,Director")]
        public async Task<IActionResult> GetLocationBookings(
            [FromQuery] int locationId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int? ignoreEventId = null,
            [FromQuery] int? ignoreParentEventId = null)
        {
            try
            {
                var events = await _locationBookingService.GetLocationBookingsAsync(
                    locationId, startTime, endTime, ignoreEventId, ignoreParentEventId);
                return Ok(ApiResponse<List<EventDto>>.SuccessResult(
                    events, $"Found {events.Count} bookings for location {locationId}"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        // Fixed: Only Manager, Director, and Admin can create events (Staff cannot create events)
        [Authorize(Roles = "Admin,Event Manager,Director")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto)
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

                // Old code:
                // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Fixed: dùng HttpContext.User (claims đã được middleware JwtBearer validate),
                // ưu tiên claim "UserId", sau đó fallback sang "sub"
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                // Old code:
                // var result = await _eventService.CreateAsync(dto, userId);
                // Fixed:
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var result = await _eventService.CreateAsync(dto, userId, userRole);

                // Determine success message based on role and status
                string successMessage = "Event created successfully";
                if (userRole == "Event Manager" && result.StatusId == 2) // PENDING_STATUS_ID
                {
                    successMessage = "Event created successfully and submitted for Director approval";
                }
                else if ((userRole == "Director" || userRole == "Admin") && result.StatusId == 3) // APPROVED_STATUS_ID
                {
                    successMessage = "Event created successfully and automatically approved";
                }

                // Old code:
                // return CreatedAtAction(nameof(GetEventById), new { id = result.EventId },
                //     ApiResponse<EventDto>.SuccessResult(result, "Event created"));
                // Fixed:
                return CreatedAtAction(nameof(GetEventById), new { id = result.EventId },
                    ApiResponse<EventDto>.SuccessResult(result, successMessage));
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] UpdateEventDto dto)
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

                // Fixed: Parse userId correctly (same as CreateEvent)
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                // Service will handle file upload from IFormFile in DTO
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
                // Fixed: dùng cùng cách parse userId như CreateEvent
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.DeleteAsync(id, userId);

                if (!result) return NotFound(ApiResponse<object>.ErrorResult("Event not found or already deleted"));
                return Ok(ApiResponse<object>.SuccessResult(null, "Event deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResult(ex.Message));
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
        [Authorize(Roles = "Admin,Event Manager,Director")]
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
        /// Staff can create sub-events with pending status (requires Manager approval)
        /// Supports banner image upload via multipart/form-data
        /// </summary>
        [HttpPost("{eventId}/subevents")]
        [Authorize(Roles = "Admin,Event Manager,Staff")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateSubEvent(int eventId, [FromForm] CreateSubEventDto dto)
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

                // Fixed: Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                // Get user role to determine initial status
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var result = await _eventService.CreateSubEventAsync(eventId, dto, userId, userRole);

                // Determine success message based on role and status
                string successMessage = "Sub-event created successfully";
                if ((userRole == "Staff" || userRole == "Event Manager") && result.StatusId == 2) // PENDING_STATUS_ID
                {
                    if (userRole == "Staff")
                        successMessage = "Sub-event created successfully and submitted for Manager approval";
                    else if (userRole == "Event Manager")
                        successMessage = "Sub-event created successfully and submitted for Director approval";
                }
                else if ((userRole == "Director" || userRole == "Admin") && result.StatusId == 3) // APPROVED_STATUS_ID
                {
                    successMessage = "Sub-event created successfully and automatically approved";
                }

                return CreatedAtAction(
                    nameof(GetEventById),
                    new { eventId = result.EventId },
                    ApiResponse<SubEventDto>.SuccessResult(result, successMessage));
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

        /// <summary>
        /// Update a sub-event
        /// Supports banner image upload via multipart/form-data
        /// </summary>
        [HttpPut("subevents/{subEventId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateSubEvent(int subEventId, [FromForm] UpdateSubEventDto dto)
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

                // Fixed: Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.UpdateSubEventAsync(subEventId, dto, userId);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Sub-event not found"));

                return Ok(ApiResponse<SubEventDto>.SuccessResult(result, "Sub-event updated successfully"));
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

        /// <summary>
        /// Delete a sub-event (soft delete)
        /// </summary>
        [HttpDelete("subevents/{subEventId}")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> DeleteSubEvent(int subEventId)
        {
            try
            {
                // Fixed: Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.DeleteSubEventAsync(subEventId, userId);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult("Sub-event not found"));

                return Ok(ApiResponse<object>.SuccessResult(null, "Sub-event deleted successfully"));
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
                // Fixed: Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.SubmitForApprovalAsync(id, userId);

                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Event not found"));

                return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event submitted for approval"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Fixed: Return BadRequest with clear error message instead of Forbid
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

        #region Event approve/reject
        // Get all events pending
        [HttpGet("pending-approval")]
        // Old code:
        // [Authorize(Roles = "Director,Admin")]
        // Fixed:
        [Authorize(Roles = "Director,Admin,Event Manager")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var result = await _eventService.GetPendingApprovalsAsync();
                return Ok(ApiResponse<List<PendingApprovalDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count} events pending approval"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // Approve event
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Director,Admin,Event Manager")]
        public async Task<IActionResult> ApproveEvent(int id, [FromBody] EventDecisionDto dto)
        {
            try
            {
                // Fixed: Parse userId correctly (same as CreateEvent)
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.ApproveEventAsync(id, dto, userId);

                return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event approved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Fixed: Return BadRequest with clear error message instead of Forbid
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

        // Reject event
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Director,Admin,Event Manager")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] EventDecisionDto dto)
        {
            try
            {
                // Fixed: Parse userId correctly (same as CreateEvent)
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.RejectEventAsync(id, dto, userId);

                return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event rejected successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Fixed: Return BadRequest with clear error message instead of Forbid
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

        // Get approval history
        [HttpGet("{id}/approval-history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApprovalHistory(int id)
        {
            try
            {
                var result = await _eventService.GetApprovalHistoryAsync(id);
                return Ok(ApiResponse<List<EventApprovalDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count} approval records"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// <summary>
        /// Đăng ký tham gia sự kiện cho user hiện tại
        /// </summary>
        [HttpPost("{id}/register")]
        [Authorize] // Chỉ cần đăng nhập
        public async Task<IActionResult> RegisterEvent(int id)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                var result = await _eventService.RegisterEventAsync(id, userId);
                return Ok(ApiResponse<EventAttendanceDto>.SuccessResult(result, "Đăng ký thành công"));
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
        /// Checkin sự kiện cho user hiện tại
        /// </summary>
        [HttpPost("{id}/checkin")]
        [Authorize]
        public async Task<IActionResult> CheckinEvent(int id)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                var result = await _eventService.CheckinEventAsync(id, userId);
                return Ok(ApiResponse<EventAttendanceDto>.SuccessResult(result, "Checkin thành công"));
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
        /// Checkout sự kiện cho user hiện tại
        /// </summary>
        [HttpPost("{id}/checkout")]
        [Authorize]
        public async Task<IActionResult> CheckoutEvent(int id)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                var result = await _eventService.CheckoutEventAsync(id, userId);
                return Ok(ApiResponse<EventAttendanceDto>.SuccessResult(result, "Checkout thành công"));
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
        // /// <summary>
        // /// Lấy danh sách sự kiện user đã đăng ký
        // /// </summary>
        // [HttpGet("list-registered-events")]
        // [Authorize]
        // public async Task<IActionResult> GetRegisteredEvents()
        // {
        //     try
        //     {
        //         var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //         if (string.IsNullOrEmpty(userIdString))
        //             return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

        //         var userId = int.Parse(userIdString);
        //         var result = await _eventService.GetRegisteredEventsAsync(userId);
        //         return Ok(ApiResponse<List<EventAttendanceDto>>.SuccessResult(result, "Danh sách sự kiện đã đăng ký"));
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
        //     }
        // }

        /// <summary>
        /// Lấy danh sách sự kiện user đã đăng ký kèm đầy đủ thông tin event và các quan hệ
        /// </summary>
        [HttpGet("list-events-myself")]
        [Authorize]
        public async Task<IActionResult> GetRegisteredEventsFull()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                var result = await _eventService.GetRegisteredEventsFullAsync(userId);
                return Ok(ApiResponse<List<RegisteredEventFullDto>>.SuccessResult(result, "Danh sách sự kiện đã đăng ký (đầy đủ thông tin)"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Hủy đăng ký sự kiện cho user hiện tại
        /// </summary>
        [HttpPost("{id}/unregister")]
        [Authorize]
        public async Task<IActionResult> UnregisterEvent(int id)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot detect current user"));

                var userId = int.Parse(userIdString);
                await _eventService.UnregisterEventAsync(id, userId);
                return Ok(ApiResponse<object>.SuccessResult(null, "Hủy đăng ký thành công"));
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
        #endregion

        #region Sub-event Email and QR Code

        /// <summary>
        /// Generate QR code for sub-event registration form (Google Form URL)
        /// Frontend must create Google Form first and provide the URL
        /// </summary>
        [HttpPost("subevents/{subEventId}/generate-qr")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateQRCodeForSubEvent(int subEventId, [FromBody] GenerateQRCodeDto dto)
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

                var result = await _eventService.GenerateQRCodeForSubEventAsync(subEventId, dto);
                return Ok(ApiResponse<GenerateQRCodeResponseDto>.SuccessResult(result, "QR code generated successfully"));
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
        /// Send email to sub-event attendees
        /// Note: SubEventId is taken from URL path parameter
        /// 
        /// Features:
        /// - Upload image file (will be uploaded to Cloudinary and embedded in email)
        /// - Import Excel file (only Email column will be extracted)
        /// - Use custom email list (optional, can be used instead of Excel file)
        /// - View list of imported emails before sending
        /// </summary>
        [HttpPost("subevents/{subEventId}/send-email")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendEmailToSubEventAttendees(int subEventId, [FromForm] SendSubEventEmailDto dto)
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

                // Validate that at least one email source is provided
                if ((dto.ExcelFile == null || dto.ExcelFile.Length == 0) && 
                    (dto.CustomEmailList == null || dto.CustomEmailList.Count == 0))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Please provide either an Excel file or a custom email list"));
                }

                // Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.SendEmailToSubEventAttendeesAsync(subEventId, dto, userId, _fileService);
                return Ok(ApiResponse<SendSubEventEmailResponseDto>.SuccessResult(result, 
                    $"Email sent successfully to {result.SuccessCount} out of {result.TotalRecipients} recipients"));
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

        #endregion

        #region Staff Email

        /// <summary>
        /// Upload Excel file with attendees list for Staff
        /// File will be saved to Cloudinary and email addresses will be extracted
        /// </summary>
        [HttpPost("staff/upload-excel")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadExcelAndExtractEmails([FromForm] StaffUploadExcelDto dto)
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

                if (dto.ExcelFile == null || dto.ExcelFile.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Excel file is required"));
                }

                var result = await _eventService.UploadExcelAndExtractEmailsAsync(dto.ExcelFile, _fileService);
                return Ok(ApiResponse<StaffUploadExcelResponseDto>.SuccessResult(
                    result,
                    $"Excel file uploaded successfully. Extracted {result.EmailCount} email addresses."));
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
        /// Send email to attendees for Staff
        /// Uses list of emails provided from Excel upload
        /// </summary>
        [HttpPost("staff/send-email")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmailToAttendees([FromBody] StaffSendEmailDto dto)
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

                // Parse userId correctly
                var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                                       ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaimValue))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Cannot extract user ID from authenticated user claims"));
                }

                if (!int.TryParse(userIdClaimValue, out var userId) || userId <= 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult($"Invalid user ID in claims: '{userIdClaimValue}'"));
                }

                var result = await _eventService.SendEmailToAttendeesAsync(dto, userId);
                return Ok(ApiResponse<StaffSendEmailResponseDto>.SuccessResult(result, 
                    $"Email sent successfully to {result.SuccessCount} out of {result.TotalRecipients} recipients"));
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

        #endregion
    }
}