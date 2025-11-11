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
    }
}
