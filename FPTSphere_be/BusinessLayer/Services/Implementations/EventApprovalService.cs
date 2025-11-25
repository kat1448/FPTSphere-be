using AutoMapper;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Helpers;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Implementations
{
    public class EventApprovalService : IEventApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EventPermissionHelper _permissionHelper;

        // Status IDs (should match your database)
        private const int DRAFT_STATUS_ID = 1;
        private const int PENDING_STATUS_ID = 2;
        private const int APPROVED_STATUS_ID = 3;
        private const int REJECTED_STATUS_ID = 4;

        public EventApprovalService(IUnitOfWork unitOfWork, IMapper mapper, EventPermissionHelper permissionHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _permissionHelper = permissionHelper;
        }

        /// <summary>
        /// Submit event for approval - changes status from Draft to Pending
        /// </summary>
        public async Task<EventDto> SubmitForApprovalAsync(int eventId, SubmitForApprovalDto dto, int currentUserId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            // Check permission using helper
            var permission = await _permissionHelper.CanSubmitForApprovalAsync(evt, currentUserId);
            permission.ThrowIfDenied();

            // Validate event has required data
            ValidateEventForSubmission(evt);

            // Update event status to Pending
            evt.StatusId = PENDING_STATUS_ID;
            await _unitOfWork.Events.UpdateAsync(evt);

            // Log the submission
            await LogEventActionAsync(eventId, currentUserId, "SubmittedForApproval", dto.Note);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        /// <summary>
        /// Approve event - changes status from Pending to Approved
        /// </summary>
        public async Task<EventDto> ApproveEventAsync(int eventId, ApproveEventDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            // Check permission
            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            // Update event status to Approved
            evt.StatusId = APPROVED_STATUS_ID;

            // Update budget if specified
            if (dto.ApprovedBudget.HasValue)
            {
                evt.EstimatedCost = dto.ApprovedBudget.Value;
            }

            await _unitOfWork.Events.UpdateAsync(evt);

            // Create approval record
            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = dto.IsConditional ? "Conditionally Approved" : "Approved",
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.EventApprovals.AddAsync(approval);

            // Log the approval
            await LogEventActionAsync(eventId, directorId, "Approved", dto.Comment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        /// <summary>
        /// Reject event - changes status from Pending to Rejected
        /// </summary>
        public async Task<EventDto> RejectEventAsync(int eventId, RejectEventDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            // Check permission
            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            // Update event status to Rejected
            evt.StatusId = REJECTED_STATUS_ID;
            await _unitOfWork.Events.UpdateAsync(evt);

            // Create rejection record with combined reasons
            var reasons = dto.SelectedReasons != null && dto.SelectedReasons.Any()
                ? string.Join("; ", dto.SelectedReasons)
                : "";

            var fullComment = string.IsNullOrEmpty(reasons)
                ? dto.Reason
                : $"{reasons}. Details: {dto.Reason}";

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Rejected",
                Comment = fullComment,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.EventApprovals.AddAsync(approval);

            // Log the rejection
            await LogEventActionAsync(eventId, directorId, "Rejected", fullComment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        /// <summary>
        /// Get all events pending approval for Director dashboard
        /// </summary>
        public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync()
        {
            var allEvents = await _unitOfWork.Events.GetAllAsync();
            var pendingEvents = allEvents
                .Where(e => e.StatusId == PENDING_STATUS_ID && e.IsDeleted != true)
                .OrderBy(e => e.CreatedAt)
                .ToList();

            var result = new List<PendingApprovalDto>();

            foreach (var evt in pendingEvents)
            {
                // Manually load related data since we don't have navigation properties
                var creator = await _unitOfWork.Users.GetByIdAsync(evt.CreatedBy);

                // Count sub-events
                var subEventsCount = (await _unitOfWork.Events.GetAllAsync())
                    .Count(e => e.ParentEventId == evt.EventId);

                // Get submission note from logs
                var logs = await _unitOfWork.EventLogs.GetLogsByEventIdAsync(evt.EventId);
                var submissionLog = logs
                    .Where(l => l.Action == "SubmittedForApproval")
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefault();

                result.Add(new PendingApprovalDto
                {
                    EventId = evt.EventId,
                    EventName = evt.EventName,
                    StartTime = evt.StartTime,
                    EndTime = evt.EndTime,
                    Budget = evt.EstimatedCost,
                    ExpectedAttendees = evt.ExpectedAttendees,
                    CreatedByName = creator?.FullName ?? "Unknown",
                    SubmittedDate = evt.UpdatedAt ?? evt.CreatedAt ?? DateTime.Now,
                    SubEventsCount = subEventsCount,
                    SubmitterNote = submissionLog?.Details
                });
            }

            return result;
        }

        /// Get approval history for a specific event
        /// Manually loads Event and Director data since we don't have navigation properties
        public async Task<List<EventApprovalDto>> GetApprovalHistoryAsync(int eventId)
        {
            var approvals = await _unitOfWork.EventApprovals.GetApprovalsByEventIdAsync(eventId);

            // Manually load related data for each approval
            var result = new List<EventApprovalDto>();

            foreach (var approval in approvals)
            {
                var evt = await _unitOfWork.Events.GetByIdAsync(approval.EventId);
                var director = await _unitOfWork.Users.GetByIdAsync(approval.DirectorId);

                var dto = _mapper.Map<EventApprovalDto>(approval);
                dto.EventName = evt?.EventName ?? "Unknown Event";
                dto.DirectorName = director?.FullName ?? "Unknown Director";

                result.Add(dto);
            }

            return result;
        }

        #region Private Helper Methods

        /// Validate event has all required data before submission
        private void ValidateEventForSubmission(Event evt)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(evt.EventName))
                errors.Add("Event name is required");

            if (string.IsNullOrWhiteSpace(evt.Description))
                errors.Add("Event description is required");

            if (evt.StartTime == default)
                errors.Add("Start time is required");

            if (evt.EndTime == default)
                errors.Add("End time is required");

            if (evt.EndTime <= evt.StartTime)
                errors.Add("End time must be after start time");

            if (evt.ExpectedAttendees <= 0)
                errors.Add("Expected attendees must be greater than 0");

            if (errors.Any())
                throw new InvalidOperationException(
                    $"Cannot submit event for approval: {string.Join(", ", errors)}");
        }

        /// Log event action to EventLogs table for audit trail
        private async Task LogEventActionAsync(int eventId, int userId, string action, string? details)
        {
            var log = new EventLog
            {
                EventId = eventId,
                UserId = userId,
                Action = action,
                Details = details,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.EventLogs.AddAsync(log);
        }

        #endregion
    }
}
