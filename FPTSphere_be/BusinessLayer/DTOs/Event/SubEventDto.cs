using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    public class SubEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Location info
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public int? ExternalLocationId { get; set; }
        public string? ExternalLocationName { get; set; }

        // Parent event info
        public int ParentEventId { get; set; }
        public string ParentEventName { get; set; } = null!;

        // Status
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating Sub-Event
    /// </summary>
    public class CreateSubEventDto
    {
        [Required(ErrorMessage = "Sub-event name is required")]
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string EventName { get; set; } = null!;

        [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        [MaxLength(255, ErrorMessage = "Banner URL cannot exceed 255 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string? BannerUrl { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        // Must have ONE of these
        public int? LocationId { get; set; }
        public int? ExternalLocationId { get; set; }
    }

    /// <summary>
    /// DTO for updating Sub-Event
    /// </summary>
    public class UpdateSubEventDto
    {
        [Required]
        [MaxLength(255)]
        public string EventName { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Url]
        public string? BannerUrl { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? LocationId { get; set; }
        public int? ExternalLocationId { get; set; }
    }
}
