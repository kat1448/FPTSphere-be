using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventApproval
{
    public class PendingApprovalDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? Budget { get; set; }
        public int ExpectedAttendees { get; set; }
        public string CreatedByName { get; set; } = null!;
        public DateTime SubmittedDate { get; set; }
        public int SubEventsCount { get; set; }
        public string? SubmitterNote { get; set; }
    }
}
