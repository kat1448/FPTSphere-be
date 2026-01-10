using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.EventTask
{
    public class UpdateEventTaskDto
    {
        public int? AssignedTo { get; set; }

        [StringLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public bool? IsTemplate { get; set; }

        public int? ParentTaskId { get; set; }
    }
}

