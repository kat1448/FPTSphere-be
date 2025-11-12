using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ==================== MAIN EVENT METHODS ====================

        public async Task<PagedResult<EventDto>> GetEventsAsync(int page, int pageSize, EventFilterDto? filter, string sortBy, bool sortDescending)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();
            var filtered = allEvents.AsEnumerable();

            if (filter != null)
            {
                if (filter.StatusId.HasValue) filtered = filtered.Where(e => e.StatusId == filter.StatusId.Value);
                if (filter.StartDate.HasValue) filtered = filtered.Where(e => e.StartTime >= filter.StartDate.Value);
                if (filter.EndDate.HasValue) filtered = filtered.Where(e => e.EndTime <= filter.EndDate.Value);
                if (filter.LocationId.HasValue) filtered = filtered.Where(e => e.LocationId == filter.LocationId.Value);
                if (filter.ExternalLocationId.HasValue) filtered = filtered.Where(e => e.ExternalLocationId == filter.ExternalLocationId.Value);
                if (filter.CreatedBy.HasValue) filtered = filtered.Where(e => e.CreatedBy == filter.CreatedBy.Value);
                if (filter.MinAttendees.HasValue) filtered = filtered.Where(e => e.ExpectedAttendees >= filter.MinAttendees.Value);
                if (filter.MaxAttendees.HasValue) filtered = filtered.Where(e => e.ExpectedAttendees <= filter.MaxAttendees.Value);
                if (filter.MinCost.HasValue) filtered = filtered.Where(e => e.EstimatedCost >= filter.MinCost.Value);
                if (filter.MaxCost.HasValue) filtered = filtered.Where(e => e.EstimatedCost <= filter.MaxCost.Value);
                if (!filter.IncludeDeleted) filtered = filtered.Where(e => e.IsDeleted != true);
            }
            else
            {
                filtered = filtered.Where(e => e.IsDeleted != true);
            }

            var total = filtered.Count();

            filtered = sortBy.ToLower() switch
            {
                "name" => sortDescending ? filtered.OrderByDescending(e => e.EventName) : filtered.OrderBy(e => e.EventName),
                "starttime" => sortDescending ? filtered.OrderByDescending(e => e.StartTime) : filtered.OrderBy(e => e.StartTime),
                "endtime" => sortDescending ? filtered.OrderByDescending(e => e.EndTime) : filtered.OrderBy(e => e.EndTime),
                "status" => sortDescending ? filtered.OrderByDescending(e => e.StatusId) : filtered.OrderBy(e => e.StatusId),
                "attendees" => sortDescending ? filtered.OrderByDescending(e => e.ExpectedAttendees) : filtered.OrderBy(e => e.ExpectedAttendees),
                "cost" => sortDescending ? filtered.OrderByDescending(e => e.EstimatedCost) : filtered.OrderBy(e => e.EstimatedCost),
                _ => sortDescending ? filtered.OrderByDescending(e => e.CreatedAt) : filtered.OrderBy(e => e.CreatedAt)
            };

            var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            return new PagedResult<EventDto>
            {
                Data = _mapper.Map<List<EventDto>>(items),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null || ev.IsDeleted == true) return null;
            return _mapper.Map<EventDto>(ev);
        }

        public async Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId)
        {
            var validation = await ValidateEventAsync(dto);
            if (!validation.success) throw new InvalidOperationException(validation.message);

            var ev = _mapper.Map<Event>(dto);
            ev.CreatedBy = currentUserId;
            ev.StatusId = 1;
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

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (ev.CreatedBy != currentUserId && currentUser?.Role?.RoleName != "Admin")
                throw new UnauthorizedAccessException("No permission");

            if (ev.StatusId != 1)
                throw new InvalidOperationException("Can only update Draft events");

            var validation = await ValidateEventAsync(_mapper.Map<CreateEventDto>(dto));
            if (!validation.success) throw new InvalidOperationException(validation.message);

            ev.EventName = dto.EventName;
            ev.Description = dto.Description;
            ev.BannerUrl = dto.BannerUrl;
            ev.StartTime = dto.StartTime;
            ev.EndTime = dto.EndTime;
            ev.ExpectedAttendees = dto.ExpectedAttendees;
            ev.EstimatedCost = dto.EstimatedCost;
            ev.LocationId = dto.LocationId;
            ev.ExternalLocationId = dto.ExternalLocationId;
            ev.TemplateId = dto.TemplateId;
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId));
        }

        public async Task<bool> DeleteAsync(int id, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null || ev.IsDeleted == true) return false;

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (ev.CreatedBy != currentUserId && currentUser?.Role?.RoleName != "Admin")
                throw new UnauthorizedAccessException("No permission");

            if (ev.StatusId != 1)
                throw new InvalidOperationException("Can only delete Draft events");

            ev.IsDeleted = true;
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task<(bool success, string message)> ValidateEventAsync(CreateEventDto dto)
        {
            if (dto.EndTime <= dto.StartTime) return (false, "End time must be after start time");
            if (dto.StartTime < DateTime.Now.AddHours(-1)) return (false, "Start time cannot be in the past");
            if (dto.LocationId.HasValue && dto.ExternalLocationId.HasValue) return (false, "Cannot have both locations");
            if (!dto.LocationId.HasValue && !dto.ExternalLocationId.HasValue) return (false, "Must specify a location");

            if (dto.LocationId.HasValue)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(dto.LocationId.Value);
                if (location == null || location.IsActive != true) return (false, "Invalid internal location");
                if (dto.ExpectedAttendees.HasValue && location.Capacity.HasValue)
                    if (dto.ExpectedAttendees.Value > location.Capacity.Value)
                        return (false, $"Attendees ({dto.ExpectedAttendees}) exceeds capacity ({location.Capacity})");
            }

            if (dto.ExternalLocationId.HasValue)
            {
                var extLoc = await _unitOfWork.ExternalLocations.GetByIdAsync(dto.ExternalLocationId.Value);
                if (extLoc == null) return (false, "Invalid external location");
            }

            if (dto.ExpectedAttendees.HasValue && dto.ExpectedAttendees.Value < 1) return (false, "Attendees must be >= 1");
            if (dto.EstimatedCost.HasValue && dto.EstimatedCost.Value < 0) return (false, "Cost cannot be negative");

            return (true, string.Empty);
        }

        // ==================== SUB-EVENT METHODS ====================

        public async Task<List<SubEventDto>> GetSubEventsAsync(int parentEventId)
        {
            var parentEvent = await _unitOfWork.Events.GetByIdAsync(parentEventId);
            if (parentEvent == null)
                throw new InvalidOperationException("Parent event not found");

            var subEvents = await _unitOfWork.Events.GetSubEventsByParentIdAsync(parentEventId);
            var result = _mapper.Map<List<SubEventDto>>(subEvents);

            foreach (var dto in result)
            {
                dto.ParentEventName = parentEvent.EventName;
            }

            return result;
        }

        public async Task<SubEventDto> CreateSubEventAsync(int parentEventId, CreateSubEventDto dto, int currentUserId)
        {
            // 1. Validate parent event exists
            var parentEvent = await _unitOfWork.Events.GetByIdAsync(parentEventId);
            if (parentEvent == null)
                throw new InvalidOperationException("Parent event not found");

            // 2. Check permissions
            if (parentEvent.CreatedBy != currentUserId)
            {
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                    throw new UnauthorizedAccessException("Only event creator or Admin can add sub-events");
            }

            // 3. Only Draft events can have sub-events added
            if (parentEvent.StatusId != 1)
                throw new InvalidOperationException("Can only add sub-events to Draft events");

            // 4. Validate sub-event data
            var validation = await ValidateSubEventAsync(dto, parentEvent);
            if (!validation.success)
                throw new InvalidOperationException(validation.message);

            // 5. Create sub-event using AutoMapper
            var subEvent = _mapper.Map<Event>(dto);
            subEvent.ParentEventId = parentEventId;
            subEvent.CreatedBy = currentUserId;
            subEvent.StatusId = 1; // Draft
            subEvent.CreatedAt = DateTime.Now;
            subEvent.IsDeleted = false;

            await _unitOfWork.Events.AddAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            // 6. Return with details using AutoMapper
            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEvent.EventId);
            var subEventDto = _mapper.Map<SubEventDto>(result);
            subEventDto.ParentEventName = parentEvent.EventName;

            return subEventDto;
        }

        public async Task<SubEventDto?> UpdateSubEventAsync(int subEventId, UpdateSubEventDto dto, int currentUserId)
        {
            // 1. Get sub-event
            var subEvent = await _unitOfWork.Events.GetByIdAsync(subEventId);
            if (subEvent == null || subEvent.ParentEventId == null)
                return null;

            // 2. Check permissions
            if (subEvent.CreatedBy != currentUserId)
            {
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                    throw new UnauthorizedAccessException("Only event creator or Admin can update sub-events");
            }

            // 3. Only Draft can be updated
            if (subEvent.StatusId != 1)
                throw new InvalidOperationException("Can only update Draft sub-events");

            // 4. Get parent event for validation
            var parentEvent = await _unitOfWork.Events.GetByIdAsync(subEvent.ParentEventId.Value);
            if (parentEvent == null)
                throw new InvalidOperationException("Parent event not found");

            // 5. Validate
            var createDto = _mapper.Map<CreateSubEventDto>(dto);
            var validation = await ValidateSubEventAsync(createDto, parentEvent);
            if (!validation.success)
                throw new InvalidOperationException(validation.message);

            // 6. Update using AutoMapper
            _mapper.Map(dto, subEvent);
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            // 7. Return with details using AutoMapper
            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEventId);
            var subEventDto = _mapper.Map<SubEventDto>(result);
            subEventDto.ParentEventName = parentEvent.EventName;

            return subEventDto;
        }

        public async Task<bool> DeleteSubEventAsync(int subEventId, int currentUserId)
        {
            // 1. Get sub-event
            var subEvent = await _unitOfWork.Events.GetByIdAsync(subEventId);
            if (subEvent == null || subEvent.ParentEventId == null || subEvent.IsDeleted == true)
                return false;

            // 2. Check permissions
            if (subEvent.CreatedBy != currentUserId)
            {
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                    throw new UnauthorizedAccessException("Only event creator or Admin can delete sub-events");
            }

            // 3. Only Draft can be deleted
            if (subEvent.StatusId != 1)
                throw new InvalidOperationException("Can only delete Draft sub-events");

            // 4. Soft delete
            subEvent.IsDeleted = true;
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Validate sub-event data
        /// </summary>
        private async Task<(bool success, string message)> ValidateSubEventAsync(CreateSubEventDto dto, Event parentEvent)
        {
            // 1. Time validation
            if (dto.EndTime <= dto.StartTime)
                return (false, "End time must be after start time");

            if (dto.StartTime < DateTime.Now.AddHours(-1))
                return (false, "Start time cannot be in the past");

            // 2. Sub-event must be within parent event time range
            if (dto.StartTime < parentEvent.StartTime)
                return (false, $"Sub-event start time cannot be before parent event start time ({parentEvent.StartTime:dd/MM/yyyy HH:mm})");

            if (dto.EndTime > parentEvent.EndTime)
                return (false, $"Sub-event end time cannot be after parent event end time ({parentEvent.EndTime:dd/MM/yyyy HH:mm})");

            // 3. Location validation (same as main event)
            if (dto.LocationId.HasValue && dto.ExternalLocationId.HasValue)
                return (false, "Cannot have both internal and external location");

            if (!dto.LocationId.HasValue && !dto.ExternalLocationId.HasValue)
                return (false, "Must specify either internal or external location");

            // 4. Verify internal location
            if (dto.LocationId.HasValue)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(dto.LocationId.Value);
                if (location == null || location.IsActive != true)
                    return (false, "Invalid internal location");
            }

            // 5. Verify external location
            if (dto.ExternalLocationId.HasValue)
            {
                var extLocation = await _unitOfWork.ExternalLocations.GetByIdAsync(dto.ExternalLocationId.Value);
                if (extLocation == null)
                    return (false, "Invalid external location");
            }

            return (true, string.Empty);
        }
    }
}