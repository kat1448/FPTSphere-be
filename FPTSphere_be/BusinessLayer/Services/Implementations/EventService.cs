using AutoMapper;
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Helpers;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EventValidationHelper _validationHelper;
        private readonly EventPermissionHelper _permissionHelper;
        private readonly EventFilterHelper _filterHelper;
        private const int DRAFT_STATUS_ID = 1;
        private const int PENDING_STATUS_ID = 2;
        private const int APPROVED_STATUS_ID = 3;
        private const int IN_PROGRESS_STATUS_ID = 4;
        private const int COMPLETED_STATUS_ID = 5;
        private const int CANCELLED_STATUS_ID = 6;

        public EventService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            EventValidationHelper validationHelper,
            EventPermissionHelper permissionHelper,
            EventFilterHelper filterHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validationHelper = validationHelper;
            _permissionHelper = permissionHelper;
            _filterHelper = filterHelper;
        }


        public async Task<PagedResult<EventDto>> GetEventsAsync(
            int page, int pageSize, EventFilterDto? filter, string sortBy, bool sortDescending)
        {
            // Normalize pagination
            (page, pageSize) = _filterHelper.NormalizePagination(page, pageSize);

            // Get all events
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();

            // Apply filters and sorting using helper
            var filtered = _filterHelper.ApplyFilters(allEvents, filter);
            var sorted = _filterHelper.ApplySorting(filtered, sortBy, sortDescending);

            // Paginate
            var total = sorted.Count();
            var items = _filterHelper.ApplyPagination(sorted, page, pageSize).ToList();

            return new PagedResult<EventDto>
            {
                Data = _mapper.Map<List<EventDto>>(items),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = _filterHelper.CalculateTotalPages(total, pageSize)
            };
        }

        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            return (ev == null || ev.IsDeleted == true) ? null : _mapper.Map<EventDto>(ev);
        }

        public async Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId)
        {
            // Validate using helper
            var validation = await _validationHelper.ValidateEventDataAsync(
                dto.EventName, dto.StartTime, dto.EndTime,
                dto.LocationId, dto.ExternalLocationId, dto.ExpectedAttendees);

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Create event
            var ev = _mapper.Map<Event>(dto);
            ev.CreatedBy = currentUserId;
            ev.StatusId = DRAFT_STATUS_ID;
            ev.CreatedAt = DateTime.Now;
            ev.IsDeleted = false;

            await _unitOfWork.Events.AddAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId));
        }

        public async Task<EventDto?> UpdateAsync(int id, UpdateEventDto dto, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null) return null;

            // Check permission using helper
            var permission = await _permissionHelper.CanModifyEventAsync(ev, currentUserId);
            permission.ThrowIfDenied();

            // Validate data using helper
            var validation = await _validationHelper.ValidateEventDataAsync(
                dto.EventName, dto.StartTime, dto.EndTime,
                dto.LocationId, dto.ExternalLocationId, dto.ExpectedAttendees);

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Update properties
            _mapper.Map(dto, ev);
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId));
        }

        public async Task<bool> DeleteAsync(int id, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null || ev.IsDeleted == true) return false;

            // Check permission using helper
            var permission = await _permissionHelper.CanDeleteEventAsync(ev, currentUserId);
            permission.ThrowIfDenied();

            // Soft delete
            ev.IsDeleted = true;
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<SubEventDto>> GetSubEventsAsync(int parentEventId)
        {
            var parent = await _unitOfWork.Events.GetByIdAsync(parentEventId);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            var subEvents = await _unitOfWork.Events.GetSubEventsByParentIdAsync(parentEventId);
            return _mapper.Map<List<SubEventDto>>(subEvents);
        }

        public async Task<SubEventDto> CreateSubEventAsync(
            int parentEventId, CreateSubEventDto dto, int currentUserId)
        {
            // Get parent
            var parent = await _unitOfWork.Events.GetByIdWithDetailsAsync(parentEventId);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            if (parent.ParentEventId != null)
                throw new InvalidOperationException("Cannot create sub-event under another sub-event");

            // Validate with parent constraints using helper
            var validation = await _validationHelper.ValidateEventDataAsync(
                dto.EventName, dto.StartTime, dto.EndTime,
                dto.LocationId, dto.ExternalLocationId, null,
                parent.StartTime, parent.EndTime);

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Create sub-event
            var subEvent = _mapper.Map<Event>(dto);
            subEvent.ParentEventId = parentEventId;
            subEvent.CreatedBy = currentUserId;
            subEvent.StatusId = DRAFT_STATUS_ID;
            subEvent.CreatedAt = DateTime.Now;
            subEvent.IsDeleted = false;

            await _unitOfWork.Events.AddAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEvent.EventId);
            return _mapper.Map<SubEventDto>(result);
        }

        public async Task<SubEventDto?> UpdateSubEventAsync(
            int subEventId, UpdateSubEventDto dto, int currentUserId)
        {
            var subEvent = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEventId);
            if (subEvent == null || subEvent.ParentEventId == null) return null;

            // Get parent
            var parent = await _unitOfWork.Events.GetByIdAsync(subEvent.ParentEventId.Value);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            // Check permission using helper
            var permission = await _permissionHelper.CanModifyEventAsync(subEvent, currentUserId);
            permission.ThrowIfDenied();

            // Validate with parent constraints using helper
            var validation = await _validationHelper.ValidateEventDataAsync(
                dto.EventName, dto.StartTime, dto.EndTime,
                dto.LocationId, dto.ExternalLocationId, null,
                parent.StartTime, parent.EndTime);

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            // Update
            _mapper.Map(dto, subEvent);
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEvent.EventId);
            return _mapper.Map<SubEventDto>(result);
        }

        public async Task<bool> DeleteSubEventAsync(int subEventId, int currentUserId)
        {
            var subEvent = await _unitOfWork.Events.GetByIdAsync(subEventId);
            if (subEvent == null || subEvent.IsDeleted == true || subEvent.ParentEventId == null)
                return false;

            // Check permission using helper
            var permission = await _permissionHelper.CanDeleteEventAsync(subEvent, currentUserId);
            permission.ThrowIfDenied();

            // Soft delete
            subEvent.IsDeleted = true;
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<PublicEventDto>> GetPublicEventsAsync()
        {
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();

            // Filter public events using helper
            var publicEvents = _filterHelper.FilterPublicEvents(allEvents);
            var sorted = _filterHelper.SortByStartTime(publicEvents);

            return _mapper.Map<List<PublicEventDto>>(sorted.ToList());
        }

        public async Task<PublicEventDto?> GetPublicEventByIdAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);

            // Validate if event is public
            if (!IsPublicEvent(ev))
                return null;

            return _mapper.Map<PublicEventDto>(ev);
        }

        public async Task<PublicEventDto?> GetPublicEventWithSubEventsAsync(int id)
        {
            // Single query loads event + sub-events
            var ev = await _unitOfWork.Events.GetByIdWithSubEventsAsync(id);

            // Validate if event is public
            if (!IsPublicEvent(ev))
                return null;

            // Map to DTO
            var result = _mapper.Map<PublicEventDto>(ev);

            // Filter and map sub-events
            if (ev.InverseParentEvent != null)
            {
                var approvedSubEvents = ev.InverseParentEvent
                    .Where(se => se.IsDeleted != true && se.StatusId == APPROVED_STATUS_ID)
                    .OrderBy(se => se.StartTime);

                result.SubEvents = _mapper.Map<List<PublicSubEventDto>>(approvedSubEvents.ToList());
            }

            return result;
        }


        /// <summary>
        /// Check if event is publicly viewable
        /// </summary>
        private bool IsPublicEvent(Event? ev)
        {
            if (ev == null ||
                ev.ParentEventId != null ||  // Not a sub-event
                ev.IsDeleted == true ||
                ev.StatusId != APPROVED_STATUS_ID)
                return false;

            return ev.EndTime >= DateTime.Now;
        }

        #region Director aprove/reject

        // Get pending approval
        public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync()
        {
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();
            var pendingEvents = allEvents
                .Where(e => e.StatusId == PENDING_STATUS_ID && e.IsDeleted != true)
                .OrderBy(e => e.CreatedAt)
                .ToList();

            var result = new List<PendingApprovalDto>();

            foreach (var evt in pendingEvents)
            {
                var subEventsCount = allEvents.Count(e => e.ParentEventId == evt.EventId);

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
                    CreatedByName = evt.CreatedByNavigation?.FullName ?? "Unknown",
                    SubmittedDate = evt.UpdatedAt ?? evt.CreatedAt ?? DateTime.Now,
                    SubEventsCount = subEventsCount,
                    SubmitterNote = submissionLog?.Details
                });
            }

            return result;
        }

        // Approve event
        public async Task<EventDto> ApproveEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            evt.StatusId = APPROVED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Approved",
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Approved", dto.Comment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        // Reject event
        public async Task<EventDto> RejectEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            evt.StatusId = CANCELLED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Rejected",
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Rejected", dto.Comment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        // Get approval history for a specific event
        public async Task<List<EventApprovalDto>> GetApprovalHistoryAsync(int eventId)
        {
            var approvals = await _unitOfWork.EventApprovals.GetApprovalsByEventIdAsync(eventId);
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

        // Log event
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