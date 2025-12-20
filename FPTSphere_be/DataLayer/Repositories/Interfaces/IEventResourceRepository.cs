using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventResourceRepository : IRepository<EventResource>
    {
        /// <summary>
        /// Get all resources assigned to an event
        /// </summary>
        Task<IEnumerable<EventResource>> GetResourcesByEventIdAsync(int eventId);

        /// <summary>
        /// Get specific resource assignment for an event
        /// </summary>
        Task<EventResource?> GetEventResourceAsync(int eventId, int resourceId);

        /// <summary>
        /// Check if resource is already assigned to event
        /// </summary>
        Task<bool> IsResourceAssignedAsync(int eventId, int resourceId);

        /// <summary>
        /// Remove resource from event
        /// </summary>
        Task RemoveResourceFromEventAsync(int eventId, int resourceId);

        /// <summary>
        /// Get all events using a specific resource
        /// </summary>
        Task<IEnumerable<EventResource>> GetEventsByResourceIdAsync(int resourceId);

        /// <summary>
        /// Get total quantity used for a resource across all events
        /// </summary>
        Task<int> GetTotalQuantityUsedForResourceAsync(int resourceId);
        Task<int> GetTotalQuantityUsedForResourceInRangeAsync(
     int resourceId,
     DateTime startTime,
     DateTime endTime,
     int[] blockingStatusIds,
     int? ignoreEventId = null);



    }
}
