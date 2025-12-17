using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.EventResource;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventResourceService
    {
        /// <summary>
        /// Assign a resource to an event (including sub-events)
        /// </summary>
        Task<EventResourceResponseDto> AssignResourceToEventAsync(int eventId, AssignResourceToEventDto dto);

        /// <summary>
        /// Get all resources assigned to an event with summary
        /// </summary>
        Task<EventResourcesSummaryDto> GetEventResourcesAsync(int eventId);

        /// <summary>
        /// Get specific resource assignment details
        /// </summary>
        Task<EventResourceResponseDto?> GetEventResourceAsync(int eventId, int resourceId);

        /// <summary>
        /// Update resource assignment (quantity)
        /// </summary>
        Task<EventResourceResponseDto> UpdateEventResourceAsync(int eventId, int resourceId, UpdateEventResourceDto dto);

        /// <summary>
        /// Remove resource from event
        /// </summary>
        Task<bool> RemoveResourceFromEventAsync(int eventId, int resourceId);

        /// <summary>
        /// Get all events using a specific resource
        /// </summary>
        Task<IEnumerable<EventResourceResponseDto>> GetEventsUsingResourceAsync(int resourceId);

        /// <summary>
        /// Get resource availability (total - used)
        /// </summary>
        Task<int> GetResourceAvailabilityAsync(int resourceId,
    DateTime startTime,
    DateTime endTime);

    }
}
