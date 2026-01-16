using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

        // Category and Type
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }

        // Expected attendees and estimated cost
        public int? ExpectedAttendees { get; set; }
        public decimal? EstimatedCost { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating Sub-Event
    /// Updated: BannerUrl is now IFormFile for file upload support
    /// </summary>
    public class CreateSubEventDto
    {
        [Required(ErrorMessage = "Sub-event name is required")]
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string EventName { get; set; } = null!;

        [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Banner image file (will be uploaded to Cloudinary)
        /// </summary>
        public IFormFile? BannerUrl { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        // Must have ONE of these
        public int? LocationId { get; set; }
        public int? ExternalLocationId { get; set; }

        // Category and Type (optional - will be auto-filled from parent if not provided)
        public int? CategoryId { get; set; }
        public int? TypeId { get; set; }

        [Range(1, 100000, ErrorMessage = "Expected attendees must be between 1 and 100,000")]
        public int? ExpectedAttendees { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Estimated cost must be between 0 and 9,999,999,999,999.99")]
        public decimal? EstimatedCost { get; set; }
    }

    /// <summary>
    /// DTO for updating Sub-Event
    /// Updated: BannerUrl is now IFormFile for file upload support
    /// </summary>
    public class UpdateSubEventDto
    {
        [Required]
        [MaxLength(255)]
        public string EventName { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        /// <summary>
        /// Banner image file (will be uploaded to Cloudinary)
        /// If provided, will replace existing banner
        /// </summary>
        public IFormFile? BannerUrl { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? LocationId { get; set; }
        public int? ExternalLocationId { get; set; }

        // Category and Type (optional)
        public int? CategoryId { get; set; }
        public int? TypeId { get; set; }

        [Range(1, 100000, ErrorMessage = "Expected attendees must be between 1 and 100,000")]
        public int? ExpectedAttendees { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Estimated cost must be between 0 and 9,999,999,999,999.99")]
        public decimal? EstimatedCost { get; set; }
    }
}
