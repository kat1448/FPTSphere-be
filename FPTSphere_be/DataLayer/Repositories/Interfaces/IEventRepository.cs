using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        /// <summary>
        /// Get event by ID with all related details (no sub-events)
        /// </summary>
        Task<Event?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// ⭐ NEW: Get event by ID with details AND sub-events in single query
        /// Optimized for public event detail pages
        /// </summary>
        Task<Event?> GetByIdWithSubEventsAsync(int id);

        /// <summary>
        /// Get all events with related details
        /// </summary>
        Task<List<Event>> GetAllWithDetailsAsync();

        /// <summary>
        /// Get sub-events by parent event ID
        /// </summary>
        Task<List<Event>> GetSubEventsByParentIdAsync(int parentEventId);
        IQueryable<Event> Query();
    }
}