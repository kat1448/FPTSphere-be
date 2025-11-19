using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.DTOs.Event;
using DataLayer.Models;

namespace BusinessLayer.Helpers
{
    /// <summary>
    /// ⭐ Event Filter & Sorting Helper
    /// Centralized filtering and sorting logic for events
    /// </summary>
    public class EventFilterHelper
    {
        #region Filtering Methods

        /// <summary>
        /// Apply all filters to event collection
        /// </summary>
        public IEnumerable<Event> ApplyFilters(IEnumerable<Event> events, EventFilterDto? filter)
        {
            if (events == null)
                return Enumerable.Empty<Event>();

            var filtered = events;

            // If no filter specified, just exclude deleted events
            if (filter == null)
                return filtered.Where(e => e.IsDeleted != true);

            // Apply each filter condition
            if (filter.StatusId.HasValue)
                filtered = filtered.Where(e => e.StatusId == filter.StatusId.Value);

            if (filter.StartDate.HasValue)
                filtered = filtered.Where(e => e.StartTime >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                filtered = filtered.Where(e => e.EndTime <= filter.EndDate.Value);

            if (filter.LocationId.HasValue)
                filtered = filtered.Where(e => e.LocationId == filter.LocationId.Value);

            if (filter.ExternalLocationId.HasValue)
                filtered = filtered.Where(e => e.ExternalLocationId == filter.ExternalLocationId.Value);

            if (filter.CreatedBy.HasValue)
                filtered = filtered.Where(e => e.CreatedBy == filter.CreatedBy.Value);

            if (filter.MinAttendees.HasValue)
                filtered = filtered.Where(e => e.ExpectedAttendees.HasValue &&
                                              e.ExpectedAttendees.Value >= filter.MinAttendees.Value);

            if (filter.MaxAttendees.HasValue)
                filtered = filtered.Where(e => e.ExpectedAttendees.HasValue &&
                                              e.ExpectedAttendees.Value <= filter.MaxAttendees.Value);

            if (filter.MinCost.HasValue)
                filtered = filtered.Where(e => e.EstimatedCost.HasValue &&
                                              e.EstimatedCost.Value >= filter.MinCost.Value);

            if (filter.MaxCost.HasValue)
                filtered = filtered.Where(e => e.EstimatedCost.HasValue &&
                                              e.EstimatedCost.Value <= filter.MaxCost.Value);

            // Deleted filter (default: exclude deleted)
            if (!filter.IncludeDeleted)
                filtered = filtered.Where(e => e.IsDeleted != true);

            return filtered;
        }

        /// <summary>
        /// Filter events by date range
        /// </summary>
        public IEnumerable<Event> FilterByDateRange(
            IEnumerable<Event> events,
            DateTime? startDate,
            DateTime? endDate)
        {
            var filtered = events;

            if (startDate.HasValue)
                filtered = filtered.Where(e => e.StartTime >= startDate.Value);

            if (endDate.HasValue)
                filtered = filtered.Where(e => e.EndTime <= endDate.Value);

            return filtered;
        }

        /// <summary>
        /// Filter events by status
        /// </summary>
        public IEnumerable<Event> FilterByStatus(IEnumerable<Event> events, int statusId)
        {
            return events.Where(e => e.StatusId == statusId);
        }

        /// <summary>
        /// Filter events by creator
        /// </summary>
        public IEnumerable<Event> FilterByCreator(IEnumerable<Event> events, int createdBy)
        {
            return events.Where(e => e.CreatedBy == createdBy);
        }

        /// <summary>
        /// Filter public events (approved, not deleted, not in past)
        /// </summary>
        public IEnumerable<Event> FilterPublicEvents(IEnumerable<Event> events)
        {
            var now = DateTime.Now;
            return events
                .Where(e => e.ParentEventId == null)        // Main events only
                .Where(e => e.IsDeleted != true)            // Not deleted
                .Where(e => e.StatusId == 3)                // Approved
                .Where(e => e.EndTime >= now);              // Not in past
        }

        /// <summary>
        /// Filter ongoing events
        /// </summary>
        public IEnumerable<Event> FilterOngoingEvents(IEnumerable<Event> events)
        {
            var now = DateTime.Now;
            return events.Where(e => e.StartTime <= now && e.EndTime >= now);
        }

        /// <summary>
        /// Filter upcoming events
        /// </summary>
        public IEnumerable<Event> FilterUpcomingEvents(IEnumerable<Event> events)
        {
            var now = DateTime.Now;
            return events.Where(e => e.StartTime > now);
        }

        /// <summary>
        /// Filter past events
        /// </summary>
        public IEnumerable<Event> FilterPastEvents(IEnumerable<Event> events)
        {
            var now = DateTime.Now;
            return events.Where(e => e.EndTime < now);
        }

        #endregion

        #region Sorting Methods

        /// <summary>
        /// Apply sorting to event collection
        /// </summary>
        public IOrderedEnumerable<Event> ApplySorting(
            IEnumerable<Event> events,
            string sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "createdAt";

            return sortBy.ToLower() switch
            {
                "name" or "eventname" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.EventName)
                        : events.OrderBy(e => e.EventName),

                "starttime" or "start" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.StartTime)
                        : events.OrderBy(e => e.StartTime),

                "endtime" or "end" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.EndTime)
                        : events.OrderBy(e => e.EndTime),

                "status" or "statusid" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.StatusId)
                        : events.OrderBy(e => e.StatusId),

                "attendees" or "expectedattendees" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.ExpectedAttendees ?? 0)
                        : events.OrderBy(e => e.ExpectedAttendees ?? 0),

                "cost" or "estimatedcost" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.EstimatedCost ?? 0)
                        : events.OrderBy(e => e.EstimatedCost ?? 0),

                "createdat" or "created" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.CreatedAt ?? DateTime.MinValue)
                        : events.OrderBy(e => e.CreatedAt ?? DateTime.MinValue),

                "updatedat" or "updated" =>
                    sortDescending
                        ? events.OrderByDescending(e => e.UpdatedAt ?? DateTime.MinValue)
                        : events.OrderBy(e => e.UpdatedAt ?? DateTime.MinValue),

                _ => // Default: sort by CreatedAt descending
                    sortDescending
                        ? events.OrderByDescending(e => e.CreatedAt ?? DateTime.MinValue)
                        : events.OrderBy(e => e.CreatedAt ?? DateTime.MinValue)
            };
        }

        /// <summary>
        /// Sort by start time (default for public events)
        /// </summary>
        public IOrderedEnumerable<Event> SortByStartTime(
            IEnumerable<Event> events,
            bool descending = false)
        {
            return descending
                ? events.OrderByDescending(e => e.StartTime)
                : events.OrderBy(e => e.StartTime);
        }

        /// <summary>
        /// Sort by created date
        /// </summary>
        public IOrderedEnumerable<Event> SortByCreatedDate(
            IEnumerable<Event> events,
            bool descending = true)
        {
            return descending
                ? events.OrderByDescending(e => e.CreatedAt ?? DateTime.MinValue)
                : events.OrderBy(e => e.CreatedAt ?? DateTime.MinValue);
        }

        #endregion

        #region Pagination Methods

        /// <summary>
        /// Normalize pagination parameters
        /// </summary>
        public (int page, int pageSize) NormalizePagination(
            int page,
            int pageSize,
            int maxPageSize = 100)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, maxPageSize);
            return (page, pageSize);
        }

        /// <summary>
        /// Apply pagination to collection
        /// </summary>
        public IEnumerable<T> ApplyPagination<T>(
            IEnumerable<T> items,
            int page,
            int pageSize)
        {
            return items
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Calculate total pages
        /// </summary>
        public int CalculateTotalPages(int totalRecords, int pageSize)
        {
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        #endregion

        #region Search Methods

        /// <summary>
        /// Search events by keyword (name, description)
        /// </summary>
        public IEnumerable<Event> SearchByKeyword(IEnumerable<Event> events, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return events;

            var normalizedKeyword = keyword.ToLower().Trim();

            return events.Where(e =>
                (e.EventName != null && e.EventName.ToLower().Contains(normalizedKeyword)) ||
                (e.Description != null && e.Description.ToLower().Contains(normalizedKeyword))
            );
        }

        /// <summary>
        /// Advanced search with multiple criteria
        /// </summary>
        public IEnumerable<Event> AdvancedSearch(
            IEnumerable<Event> events,
            string? keyword = null,
            int? statusId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? locationId = null,
            int? createdBy = null)
        {
            var result = events;

            if (!string.IsNullOrWhiteSpace(keyword))
                result = SearchByKeyword(result, keyword);

            if (statusId.HasValue)
                result = FilterByStatus(result, statusId.Value);

            if (fromDate.HasValue || toDate.HasValue)
                result = FilterByDateRange(result, fromDate, toDate);

            if (locationId.HasValue)
                result = result.Where(e => e.LocationId == locationId.Value);

            if (createdBy.HasValue)
                result = FilterByCreator(result, createdBy.Value);

            return result;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Group events by month
        /// </summary>
        public Dictionary<string, List<Event>> GroupByMonth(IEnumerable<Event> events)
        {
            return events
                .GroupBy(e => e.StartTime.ToString("yyyy-MM"))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(e => e.StartTime).ToList()
                );
        }

        /// <summary>
        /// Group events by status
        /// </summary>
        public Dictionary<int, List<Event>> GroupByStatus(IEnumerable<Event> events)
        {
            return events
                .GroupBy(e => e.StatusId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
        }

        /// <summary>
        /// Get event statistics
        /// </summary>
        public EventStatistics GetStatistics(IEnumerable<Event> events)
        {
            var eventList = events.ToList();
            var now = DateTime.Now;

            return new EventStatistics
            {
                TotalEvents = eventList.Count,
                DraftEvents = eventList.Count(e => e.StatusId == 1),
                PendingEvents = eventList.Count(e => e.StatusId == 2),
                ApprovedEvents = eventList.Count(e => e.StatusId == 3),
                OngoingEvents = eventList.Count(e => e.StartTime <= now && e.EndTime >= now),
                UpcomingEvents = eventList.Count(e => e.StartTime > now),
                PastEvents = eventList.Count(e => e.EndTime < now),
                TotalExpectedAttendees = eventList.Sum(e => e.ExpectedAttendees ?? 0),
                TotalEstimatedCost = eventList.Sum(e => e.EstimatedCost ?? 0)
            };
        }

        #endregion
    }

    /// <summary>
    /// Event statistics model
    /// </summary>
    public class EventStatistics
    {
        public int TotalEvents { get; set; }
        public int DraftEvents { get; set; }
        public int PendingEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int OngoingEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public int TotalExpectedAttendees { get; set; }
        public decimal TotalEstimatedCost { get; set; }
    }
}