using System;
using BusinessLayer.DTOs.Event;

namespace BusinessLayer.DTOs.Event
{
    public class RegisteredEventFullDto
    {
        public int AttendanceId { get; set; }
        public int EventId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string Method { get; set; }
        public EventDto Event { get; set; } // Chứa đầy đủ thông tin event, bao gồm các quan hệ khác
    }
}
