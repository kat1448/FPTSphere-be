using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "Event name is required")]
        [MaxLength(255, ErrorMessage = "Event name cannot exceed 255 characters")]
        public string EventName { get; set; } = null!;

        [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        [MaxLength(255, ErrorMessage = "Banner URL cannot exceed 255 characters")]
        [Url(ErrorMessage = "Banner URL must be a valid URL")]
        public string? BannerUrl { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        // ⭐ NEW FIELDS WITH VALIDATION
        [Range(1, 100000, ErrorMessage = "Expected attendees must be between 1 and 100,000")]
        public int? ExpectedAttendees { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Estimated cost must be between 0 and 9,999,999,999,999.99")]
        public decimal? EstimatedCost { get; set; }

        public int? LocationId { get; set; }

        public int? ExternalLocationId { get; set; }

        public int? TemplateId { get; set; }
    }

}
