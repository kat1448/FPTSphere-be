using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs;
using BusinessLayer.Helpers;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

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
        private const int APPROVED_STATUS_ID = 3;

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

    }
}