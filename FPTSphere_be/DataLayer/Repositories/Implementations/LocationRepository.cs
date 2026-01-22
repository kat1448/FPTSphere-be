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
            string? building = null,
            int? ignoreEventId = null,
            int? ignoreParentEventId = null)
        {
            // 1️⃣ Các trạng thái CHIẾM PHÒNG
            var blockingStatuses = new[] { 2, 3, 4 };
            // PendingApproval, Approved, InProgress

            // 2️⃣ Query cơ bản
            var query = _dbSet
                .Include(l => l.Events.Where(e =>
                    e.IsDeleted != true &&
                    blockingStatuses.Contains(e.StatusId)
                ))
                .Where(l => l.IsActive == true);

            // 3️⃣ Lọc sức chứa
            if (minCapacity.HasValue)
            {
                query = query.Where(l => l.Capacity >= minCapacity.Value);
            }

            // 4️⃣ Lọc theo building
            if (!string.IsNullOrWhiteSpace(building))
            {
                query = query.Where(l => l.Building == building);
            }

            var locations = await query.ToListAsync();

            // Loại các phòng bị trùng thời gian
            var availableLocations = locations
                .Where(location =>
                    !location.Events.Any(evt =>
                        // Bỏ qua chính event đang sửa hoặc parent event (khi tạo sub-event)
                        (ignoreEventId.HasValue && evt.EventId == ignoreEventId.Value) ||
                        (ignoreParentEventId.HasValue && evt.EventId == ignoreParentEventId.Value)
                            ? false
                            : evt.StartTime < endTime && evt.EndTime > startTime
                    )
                )
                .ToList();

            return availableLocations;
        }



    }
}
