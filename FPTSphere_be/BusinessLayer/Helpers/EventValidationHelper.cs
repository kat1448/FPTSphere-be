using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Constants;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Helpers
{
    /// <summary>
    /// ⭐ Event Validation Helper
    /// Centralized validation logic for events and sub-events
    /// Can be reused across multiple services
    /// </summary>
    public class EventValidationHelper
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventValidationHelper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Public Validation Methods

        /// <summary>
        /// Validate complete event data (for both main events and sub-events)
        /// </summary>
        public async Task<ValidationResult> ValidateEventDataAsync(
            string eventName,
            DateTime startTime,
            DateTime endTime,
            int? locationId,
            int? externalLocationId,
            int? expectedAttendees,
            DateTime? parentStartTime = null,
            DateTime? parentEndTime = null,
            int? parentEventId = null,   
            int? currentEventId = null  
        )
        {
            // 1. Basic field validation
            if (string.IsNullOrWhiteSpace(eventName))
                return ValidationResult.Fail("Event name is required");

            // 2. Time validation
            var timeValidation = ValidateTimeRange(startTime, endTime, parentStartTime, parentEndTime);
            if (!timeValidation.IsSuccess)
                return timeValidation;

            // 3. Location existence + capacity validation
            var locationValidation = await ValidateLocationAsync(locationId, externalLocationId, expectedAttendees);
            if (!locationValidation.IsSuccess)
                return locationValidation;

            // 4. Location availability (conflict with other events)
            //    - Main event: chỉ truyền currentEventId (nếu update) để bỏ qua chính nó
            //    - Sub-event: truyền parentEventId để bỏ qua parent (không coi parent là conflict)
            if (locationId.HasValue)
            {
                var availability = await ValidateLocationAvailabilityAsync(
                    locationId.Value,
                    startTime,
                    endTime,
                    ignoreEventId: currentEventId,
                    ignoreParentEventId: parentEventId
                );

                if (!availability.IsSuccess)
                    return availability;
            }

            return ValidationResult.Success();
        }
        /// Validate time ranges with optional parent event constraints
        public ValidationResult ValidateTimeRange(
            DateTime startTime,
            DateTime endTime,
            DateTime? parentStartTime = null,
            DateTime? parentEndTime = null)
        {
            // Basic time validation
            if (endTime <= startTime)
                return ValidationResult.Fail("End time must be after start time");

            if (startTime < DateTime.Now.AddHours(-1))
                return ValidationResult.Fail("Start time cannot be in the past");

            // Duration check (optional - max 30 days for an event)
            var duration = endTime - startTime;
            if (duration.TotalDays > 30)
                return ValidationResult.Fail("Event duration cannot exceed 30 days");

            // Sub-event validation (must be within parent timeframe)
            if (parentStartTime.HasValue && parentEndTime.HasValue)
            {
                if (startTime < parentStartTime.Value)
                    return ValidationResult.Fail(
                        $"Sub-event must start after parent event ({parentStartTime.Value:g})");

                if (endTime > parentEndTime.Value)
                    return ValidationResult.Fail(
                        $"Sub-event must end before parent event ({parentEndTime.Value:g})");

                // Sub-event should not be longer than parent
                var parentDuration = parentEndTime.Value - parentStartTime.Value;
                if (duration > parentDuration)
                    return ValidationResult.Fail("Sub-event duration cannot exceed parent event duration");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate location (internal or external) with capacity check
        /// </summary>
        public async Task<ValidationResult> ValidateLocationAsync(
            int? locationId,
            int? externalLocationId,
            int? expectedAttendees)
        {
            // Must have exactly one location
            if (locationId.HasValue && externalLocationId.HasValue)
                return ValidationResult.Fail("Cannot specify both internal and external location");

            if (!locationId.HasValue && !externalLocationId.HasValue)
                return ValidationResult.Fail("Must specify either internal or external location");

            // Validate internal location
            if (locationId.HasValue)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(locationId.Value);

                if (location == null)
                    return ValidationResult.Fail("Internal location not found");

                if (location.IsActive != true)
                    return ValidationResult.Fail("Internal location is not active");

                // Check capacity
                if (expectedAttendees.HasValue && location.Capacity.HasValue)
                {
                    if (expectedAttendees.Value > location.Capacity.Value)
                        return ValidationResult.Fail(
                            $"Expected attendees ({expectedAttendees}) exceeds location capacity ({location.Capacity})");
                }
            }

            // Validate external location
            if (externalLocationId.HasValue)
            {
                var extLocation = await _unitOfWork.ExternalLocations.GetByIdAsync(externalLocationId.Value);

                if (extLocation == null)
                    return ValidationResult.Fail("External location not found");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate event cost (if specified)
        /// </summary>
        public ValidationResult ValidateEventCost(decimal? estimatedCost)
        {
            if (!estimatedCost.HasValue)
                return ValidationResult.Success();

            if (estimatedCost.Value < 0)
                return ValidationResult.Fail("Estimated cost cannot be negative");

            if (estimatedCost.Value > 10_000_000_000) // 10 billion
                return ValidationResult.Fail("Estimated cost exceeds maximum allowed value");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate expected attendees count
        /// </summary>
        public ValidationResult ValidateAttendeeCount(int? expectedAttendees)
        {
            if (!expectedAttendees.HasValue)
                return ValidationResult.Success();

            if (expectedAttendees.Value <= 0)
                return ValidationResult.Fail("Expected attendees must be greater than 0");

            if (expectedAttendees.Value > 100_000)
                return ValidationResult.Fail("Expected attendees exceeds maximum capacity (100,000)");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate banner URL format
        /// </summary>
        public ValidationResult ValidateBannerUrl(string? bannerUrl)
        {
            if (string.IsNullOrWhiteSpace(bannerUrl))
                return ValidationResult.Success();

            // Basic URL validation
            if (!Uri.TryCreate(bannerUrl, UriKind.Absolute, out var uri))
                return ValidationResult.Fail("Banner URL is not a valid URL");

            // Check if it's HTTP/HTTPS
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return ValidationResult.Fail("Banner URL must be HTTP or HTTPS");

            return ValidationResult.Success();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if location is free in given time range.
        /// You can optionally ignore:
        ///  - ignoreEventId: chính event đang update
        ///  - ignoreParentEventId: parent event khi tạo/đổi sub-event
        /// </summary>
        public async Task<ValidationResult> ValidateLocationAvailabilityAsync(
            int locationId,
            DateTime startTime,
            DateTime endTime,
            int? ignoreEventId = null,
            int? ignoreParentEventId = null)
        {
            // ✅ CHỈ Pending/Approved/InProgress mới block phòng
            var blockingStatusIds = new[]
            {
        EventStatusIds.PendingApproval,  // 2
        EventStatusIds.Approved,          // 3
        EventStatusIds.InProgress         // 4
    };

            var conflictingEvents = await _unitOfWork.Events.FindAllAsync(e =>
                e.LocationId == locationId &&
                e.IsDeleted != true &&
                blockingStatusIds.Contains(e.StatusId) &&  // ✅ CHỈ check 3 status này
                (!ignoreEventId.HasValue || e.EventId != ignoreEventId.Value) &&
                (!ignoreParentEventId.HasValue || e.EventId != ignoreParentEventId.Value) &&
                e.StartTime < endTime && e.EndTime > startTime
            );

            if (conflictingEvents.Any())
            {
                var conflict = conflictingEvents.First();
                var location = await _unitOfWork.Locations.GetByIdAsync(locationId);

                // ✅ Thêm status name vào error message
                var statusName = conflict.StatusId switch
                {
                    EventStatusIds.PendingApproval => "Đang chờ duyệt",
                    EventStatusIds.Approved => "Đã duyệt",
                    EventStatusIds.InProgress => "Đang diễn ra",
                    _ => $"Status {conflict.StatusId}"
                };

                return ValidationResult.Fail(
                    $"❌ Phòng '{location?.Name}' đã được đặt bởi sự kiện '{conflict.EventName}' " +
                    $"({statusName}) trong khoảng {conflict.StartTime:dd/MM/yyyy HH:mm} - {conflict.EndTime:HH:mm}. " +
                    $"Vui lòng chọn phòng khác hoặc thời gian khác."
                );
            }

            return ValidationResult.Success();
        }



        /// <summary>
        /// Basic time overlap check:
        /// [aStart, aEnd] overlaps [bStart, bEnd] if:
        /// aStart < bEnd && bStart < aEnd
        /// </summary>
        private bool TimeRangeOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart < bEnd && bStart < aEnd;
        }

        /// <summary>
        /// Check if a date is in the past (with tolerance)
        /// </summary>
        private bool IsInPast(DateTime dateTime, int toleranceHours = 1)
        {
            return dateTime < DateTime.Now.AddHours(-toleranceHours);
        }

        #endregion
    }

    /// <summary>
    /// Validation result object for cleaner error handling
    /// </summary>
    public class ValidationResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        private ValidationResult(bool isSuccess, string errorMessage = "")
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true);

        public static ValidationResult Fail(string errorMessage) => new ValidationResult(false, errorMessage);

        // Implicit conversion to tuple for backwards compatibility
        public void Deconstruct(out bool success, out string message)
        {
            success = IsSuccess;
            message = ErrorMessage;
        }

        // Convert to tuple
        public (bool success, string message) ToTuple() => (IsSuccess, ErrorMessage);
    }
}
