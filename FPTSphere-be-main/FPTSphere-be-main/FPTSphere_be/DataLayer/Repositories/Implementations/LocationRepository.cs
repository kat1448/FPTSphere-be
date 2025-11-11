using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(EventDbContext context) : base(context) { }

        public async Task<List<Location>> GetActiveLocationsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive == true)
                .OrderBy(l => l.Building)
                .ThenBy(l => l.RoomNumber)
                .ToListAsync();
        }

        public async Task<List<Location>> GetByBuildingAsync(string building)
        {
            return await _dbSet
                .Where(l => l.Building == building)
                .OrderBy(l => l.RoomNumber)
                .ToListAsync();
        }

        public async Task<List<Location>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(l => l.Name.ToLower().Contains(term) ||
                           (l.Building != null && l.Building.ToLower().Contains(term)) ||
                           (l.RoomNumber != null && l.RoomNumber.ToLower().Contains(term)))
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(l => l.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(l => l.LocationId != excludeId.Value);
            return await query.AnyAsync();
        }
        public async Task<List<Location>> GetAvailableLocationsAsync(
    DateTime startTime,
    DateTime endTime,
    int? minCapacity = null,
    string? building = null)
        {
            // Step 1: Get base query with filters
            var query = _dbSet
                .Include(l => l.Events.Where(e => e.IsDeleted != true))
                .Where(l => l.IsActive == true);

            // Step 2: Filter by capacity
            if (minCapacity.HasValue)
            {
                query = query.Where(l => l.Capacity >= minCapacity.Value);
            }

            // Step 3: Filter by building
            if (!string.IsNullOrWhiteSpace(building))
            {
                query = query.Where(l => l.Building == building);
            }

            // Step 4: Get all matching locations with their events
            var locations = await query.ToListAsync();

            // Step 5: Filter out locations with conflicting events (in memory)
            var availableLocations = locations.Where(location =>
            {
                // Check if ANY event conflicts with the requested time
                bool hasConflict = location.Events.Any(evt =>
                {
                    // Conflict formula: (startA < endB) AND (endA > startB)
                    bool conflicts = evt.StartTime < endTime && evt.EndTime > startTime;
                    return conflicts;
                });

                // Return location only if NO conflict
                return !hasConflict;
            }).ToList();

            return availableLocations;
        }
    }
}
