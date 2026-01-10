using System;

namespace BusinessLayer.DTOs.EventTask
{
    public class EventTaskDto
    {
        public int TaskId { get; set; }
        public int EventId { get; set; }
        public int AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Report { get; set; }
        public int? AssignBy { get; set; }
        public string? AssignByName { get; set; }
        public bool? IsTemplate { get; set; }
        public int? ParentTaskId { get; set; }
    }
}
