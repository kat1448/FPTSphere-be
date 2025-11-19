using System;
using System.Collections.Generic;

namespace BusinessLayer.DTOs.Event
{

    public class PublicEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? LocationName { get; set; }
        public string? LocationAddress { get; set; }

        public string StatusName { get; set; } = null!;

        public bool IsOngoing { get; set; }
        public bool IsUpcoming { get; set; }

        public int? ExpectedAttendees { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PublicSubEventDto>? SubEvents { get; set; }

        public int SubEventCount => SubEvents?.Count ?? 0;
        public bool HasSubEvents => SubEvents?.Count > 0;
    }

    public class PublicSubEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? LocationName { get; set; }
        public string? Building { get; set; }
        public string? RoomNumber { get; set; }
        public int? ExpectedAttendees { get; set; }
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        public string DateDisplay => StartTime.ToString("dddd, MMMM dd, yyyy");
        public string DayName => StartTime.ToString("dddd");
    }
}