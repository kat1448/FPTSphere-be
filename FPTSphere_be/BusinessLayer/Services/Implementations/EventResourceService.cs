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
            // 1. Validate event exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {eventId} not found");

            // 2. Validate resource exists
            var resource = await _unitOfWork.Resources.GetByIdAsync(dto.ResourceId);
            if (resource == null)
                throw new KeyNotFoundException($"Resource with ID {dto.ResourceId} not found");

            // 3. Check if resource is active
            if (resource.IsActive != true)
                throw new InvalidOperationException($"Resource '{resource.Name}' is not active and cannot be assigned");

            // 4. Check if resource already assigned to this event
            var eventResourceRepo = _unitOfWork.EventResources;
            var isAssigned = await eventResourceRepo.IsResourceAssignedAsync(eventId, dto.ResourceId);
            if (isAssigned)
                throw new InvalidOperationException($"Resource '{resource.Name}' is already assigned to this event. Use update instead.");

            // 5. Check availability (total quantity - already used)
            var totalUsed = await eventResourceRepo.GetTotalQuantityUsedForResourceAsync(dto.ResourceId);
            var available = resource.Quantity - totalUsed;

            if (dto.QuantityUsed > available)
            {
                throw new InvalidOperationException(
                    $"Insufficient quantity. Requested: {dto.QuantityUsed}, Available: {available} " +
                    $"(Total: {resource.Quantity}, Already used: {totalUsed})");
            }

            // 6. Create assignment
            var eventResource = new EventResource
            {
                EventId = eventId,
                ResourceId = dto.ResourceId,
                QuantityUsed = dto.QuantityUsed
            };

            await eventResourceRepo.AddAsync(eventResource);
            await _unitOfWork.SaveChangesAsync();

            // 7. Return full details
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
            // 1. Get existing assignment
            var eventResourceRepo = _unitOfWork.EventResources;
            var eventResource = await eventResourceRepo.GetEventResourceAsync(eventId, resourceId);

            if (eventResource == null)
                throw new KeyNotFoundException(
                    $"Resource assignment not found for event {eventId} and resource {resourceId}");

            // 2. Validate new quantity doesn't exceed availability
            if (eventResource.Resource != null)
            {
                // Get total used by OTHER events
                var totalUsedByOthers = await eventResourceRepo.GetTotalQuantityUsedForResourceAsync(resourceId)
                                        - eventResource.QuantityUsed; // Exclude current

                var available = eventResource.Resource.Quantity - totalUsedByOthers;

                if (dto.QuantityUsed > available)
                {
                    throw new InvalidOperationException(
                        $"Insufficient quantity. Requested: {dto.QuantityUsed}, Available: {available} " +
                        $"(Total: {eventResource.Resource.Quantity}, Used by other events: {totalUsedByOthers})");
                }
            }

            // 3. Update quantity
            eventResource.QuantityUsed = dto.QuantityUsed;

            await eventResourceRepo.UpdateAsync(eventResource);
            await _unitOfWork.SaveChangesAsync();

            // 4. Return updated details
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

        public async Task<int> GetResourceAvailabilityAsync(int resourceId)
        {
            // Validate resource exists
            var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
            if (resource == null)
                throw new KeyNotFoundException($"Resource with ID {resourceId} not found");

            // Calculate availability
            var eventResourceRepo = _unitOfWork.EventResources;
            var totalUsed = await eventResourceRepo.GetTotalQuantityUsedForResourceAsync(resourceId);

            return resource.Quantity - totalUsed;
        }
    }
}