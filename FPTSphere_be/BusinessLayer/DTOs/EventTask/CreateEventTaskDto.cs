using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.EventTask
{
    public class CreateEventTaskDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int AssignedTo { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Not Started"; // Default: Chưa bắt đầu

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool? IsTemplate { get; set; } = false; // Default: false

        public int? ParentTaskId { get; set; }
    }
}

