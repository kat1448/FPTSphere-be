using System;

namespace BusinessLayer.DTOs.Event
{
    public class EventAttendanceDto
    {
        public int AttendanceId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string? Method { get; set; }
    }
}
