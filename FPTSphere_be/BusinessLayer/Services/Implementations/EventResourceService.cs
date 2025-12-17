using AutoMapper;
using BusinessLayer.DTOs.EventResource;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class EventResourceService : IEventResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventResourceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<EventResourceResponseDto> AssignResourceToEventAsync(int eventId, AssignResourceToEventDto dto)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId)
                ?? throw new KeyNotFoundException($"Event with ID {eventId} not found");

            var resource = await _unitOfWork.Resources.GetByIdAsync(dto.ResourceId)
                ?? throw new KeyNotFoundException($"Resource with ID {dto.ResourceId} not found");

            if (resource.IsActive != true)
                throw new InvalidOperationException($"Resource '{resource.Name}' is not active and cannot be assigned");

            var eventResourceRepo = _unitOfWork.EventResources;

            var isAssigned = await eventResourceRepo.IsResourceAssignedAsync(eventId, dto.ResourceId);
            if (isAssigned)
                throw new InvalidOperationException($"Resource '{resource.Name}' is already assigned to this event. Use update instead.");

            // ✅ NEW: availability theo time + status
            var blockingStatusIds = new[] { 2, 3, 4 }; // Pending, Approved, InProgress
            var usedInRange = await eventResourceRepo.GetTotalQuantityUsedForResourceInRangeAsync(
                dto.ResourceId,
                eventEntity.StartTime,
                eventEntity.EndTime,
                blockingStatusIds,
                ignoreEventId: eventId // nếu event đang update/assign lại
            );

            var available = resource.Quantity - usedInRange;
            if (dto.QuantityUsed > available)
                throw new InvalidOperationException(
                    $"Thiết bị '{resource.Name}' không đủ số lượng trong khung giờ này. Requested: {dto.QuantityUsed}, Available: {available}");

            // create assignment
            var eventResource = new EventResource
            {
                EventId = eventId,
                ResourceId = dto.ResourceId,
                QuantityUsed = dto.QuantityUsed
            };

            await eventResourceRepo.AddAsync(eventResource);
            await _unitOfWork.SaveChangesAsync();

            var created = await eventResourceRepo.GetEventResourceAsync(eventId, dto.ResourceId);
            return _mapper.Map<EventResourceResponseDto>(created!);
        }

        public async Task<EventResourcesSummaryDto> GetEventResourcesAsync(int eventId)
        {
            // Validate event exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {eventId} not found");

            // Get all resources for event
            var eventResourceRepo = _unitOfWork.EventResources;
            var resources = await eventResourceRepo.GetResourcesByEventIdAsync(eventId);

            // Map to DTOs
            var resourceDtos = _mapper.Map<List<EventResourceResponseDto>>(resources);

            // Calculate summary
            return new EventResourcesSummaryDto
            {
                EventId = eventId,
                EventName = eventEntity.EventName,
                TotalResourcesAssigned = resourceDtos.Count,
                TotalQuantityUsed = resourceDtos.Sum(r => r.QuantityUsed),
                Resources = resourceDtos
            };
        }

        public async Task<EventResourceResponseDto?> GetEventResourceAsync(int eventId, int resourceId)
        {
            var eventResourceRepo = _unitOfWork.EventResources;
            var eventResource = await eventResourceRepo.GetEventResourceAsync(eventId, resourceId);

            return eventResource == null ? null : _mapper.Map<EventResourceResponseDto>(eventResource);
        }

        public async Task<EventResourceResponseDto> UpdateEventResourceAsync(
     int eventId,
     int resourceId,
     UpdateEventResourceDto dto)
        {
            var eventResourceRepo = _unitOfWork.EventResources;

            var eventResource = await eventResourceRepo.GetEventResourceAsync(eventId, resourceId)
                ?? throw new KeyNotFoundException("Resource assignment not found");

            var evt = await _unitOfWork.Events.GetByIdAsync(eventId)
                ?? throw new InvalidOperationException("Event not found");

            if (dto.QuantityUsed <= 0)
                throw new InvalidOperationException("Quantity must be greater than 0");

            var blockingStatusIds = new[] { 2, 3, 4 }; // Pending, Approved, InProgress

            // ✅ TÍNH ĐÚNG availability (theo time + status)
            var usedInRangeByOthers =
                await eventResourceRepo.GetTotalQuantityUsedForResourceInRangeAsync(
                    resourceId,
                    evt.StartTime,
                    evt.EndTime,
                    blockingStatusIds,
                    ignoreEventId: eventId
                );

            var available = eventResource.Resource!.Quantity - usedInRangeByOthers;

            if (dto.QuantityUsed > available)
                throw new InvalidOperationException(
                    $"Not enough resource. Requested: {dto.QuantityUsed}, Available: {available}");

            eventResource.QuantityUsed = dto.QuantityUsed;

            await eventResourceRepo.UpdateAsync(eventResource);
            await _unitOfWork.SaveChangesAsync();

            var updated = await eventResourceRepo.GetEventResourceAsync(eventId, resourceId);
            return _mapper.Map<EventResourceResponseDto>(updated!);
        }


        public async Task<bool> RemoveResourceFromEventAsync(int eventId, int resourceId)
        {
            var eventResourceRepo = _unitOfWork.EventResources;

            // Check if assignment exists
            var exists = await eventResourceRepo.IsResourceAssignedAsync(eventId, resourceId);
            if (!exists)
                throw new KeyNotFoundException(
                    $"Resource assignment not found for event {eventId} and resource {resourceId}");

            // Remove assignment
            await eventResourceRepo.RemoveResourceFromEventAsync(eventId, resourceId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<EventResourceResponseDto>> GetEventsUsingResourceAsync(int resourceId)
        {
            // Validate resource exists
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (resource == null)
                throw new KeyNotFoundException($"Resource with ID {resourceId} not found");

            // Get all events using this resource
            var eventResourceRepo = _unitOfWork.EventResources;
            var eventResources = await eventResourceRepo.GetEventsByResourceIdAsync(resourceId);

            return _mapper.Map<IEnumerable<EventResourceResponseDto>>(eventResources);
        }

        public async Task<int> GetResourceAvailabilityAsync(
            int resourceId,
            DateTime startTime,
            DateTime endTime)
        {
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId)
                ?? throw new KeyNotFoundException($"Resource {resourceId} not found");

            var blockingStatusIds = new[] { 2, 3, 4 }; // Pending, Approved, InProgress

            var usedInRange = await _unitOfWork.EventResources
                .GetTotalQuantityUsedForResourceInRangeAsync(
                    resourceId,
                    startTime,
                    endTime,
                    blockingStatusIds
                );

            var available = resource.Quantity - usedInRange;

            return Math.Max(0, available);
        }


    }
}