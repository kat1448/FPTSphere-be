using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.DTOs.Event
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Event name is required")]
        [MaxLength(255, ErrorMessage = "Event name cannot exceed 255 characters")]
        public string EventName { get; set; } = null!;

        [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        public IFormFile? BannerUrl { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [Range(1, 100000, ErrorMessage = "Expected attendees must be between 1 and 100,000")]
        public int? ExpectedAttendees { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Estimated cost must be between 0 and 9,999,999,999,999.99")]
        public decimal? EstimatedCost { get; set; }

        // Location - must specify either LocationId OR ExternalLocationId
        public int? LocationId { get; set; }

        public int? ExternalLocationId { get; set; }

        // Feedback template (optional)
        public int? TemplateId { get; set; }

        // Category and Type (optional)
        public int? CategoryId { get; set; }

        public int? TypeId { get; set; }
    }
}
