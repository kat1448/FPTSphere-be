using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventApprovalsController : ControllerBase
    {
        private readonly IEventApprovalService _approvalService;

        public EventApprovalsController(IEventApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        // Submit event for approval (Event Manager only)
        // Changes event status from Draft to Pending
        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Admin,Event Manager")]
        public async Task<IActionResult> SubmitForApproval(int id, [FromBody] SubmitForApprovalDto dto)
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
                var result = await _approvalService.SubmitForApprovalAsync(id, dto, userId);

                return Ok(ApiResponse<EventDto>.SuccessResult(
                    result,
                    "Event submitted for approval successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    $"Error submitting event: {ex.Message}"));
            }
        }

        // Approve an event
        // Changes event status from Pending to Approved
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Director")]
        public async Task<IActionResult> ApproveEvent(int id, [FromBody] ApproveEventDto dto)
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

                var directorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _approvalService.ApproveEventAsync(id, dto, directorId);

                return Ok(ApiResponse<EventDto>.SuccessResult(
                    result,
                    "Event approved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    $"Error approving event: {ex.Message}"));
            }
        }

        // Reject an event
        // Changes event status from Pending to Rejected
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Director")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] RejectEventDto dto)
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

                var directorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _approvalService.RejectEventAsync(id, dto, directorId);

                return Ok(ApiResponse<EventDto>.SuccessResult(
                    result,
                    "Event rejected successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    $"Error rejecting event: {ex.Message}"));
            }
        }

        // Get all events pending approval
        // Returns list of events with status = Pending
        [HttpGet("pending-approvals")]
        [Authorize(Roles = "Admin,Director")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var result = await _approvalService.GetPendingApprovalsAsync();

                return Ok(ApiResponse<List<PendingApprovalDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count} events pending approval"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    $"Error retrieving pending approvals: {ex.Message}"));
            }
        }

        // Get approval history for a specific event
        // Returns all approval/rejection records for the event
        [HttpGet("{id}/approval-history")]
        public async Task<IActionResult> GetApprovalHistory(int id)
        {
            try
            {
                var result = await _approvalService.GetApprovalHistoryAsync(id);

                return Ok(ApiResponse<List<EventApprovalDto>>.SuccessResult(
                    result,
                    $"Retrieved {result.Count} approval records"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    $"Error retrieving approval history: {ex.Message}"));
            }
        }
    }
}
