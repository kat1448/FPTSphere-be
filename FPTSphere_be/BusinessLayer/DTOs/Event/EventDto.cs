using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.EventStatus;
using BusinessLayer.DTOs.ExternalLocation;
using BusinessLayer.DTOs.Location;

namespace BusinessLayer.DTOs.Event
{
    public class EventDto
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = null!;

        public string? Description { get; set; }

        public string? BannerUrl { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? ExpectedAttendees { get; set; }

        public decimal? EstimatedCost { get; set; }
        public int? LocationId { get; set; }
        public LocationDto? Location { get; set; }

        public int? ExternalLocationId { get; set; }
        public ExternalLocationDto? ExternalLocation { get; set; }

        public int CreatedBy { get; set; }
        public UserResponse? Creator { get; set; }

        public int StatusId { get; set; }
        public EventStatusDto? Status { get; set; }

        public int? ParentEventId { get; set; }

        public int? TemplateId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
