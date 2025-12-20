using System; 
using BusinessLayer.DTOs.Event;

namespace BusinessLayer.DTOs.EventTask
{
    public class EventTaskWithEventDto
    {
        public int TaskId { get; set; }
        public int EventId { get; set; }
        public int AssignedTo { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Report { get; set; }
        public EventDto Event { get; set; } // Thông tin event đầy đủ
    }
}
