using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventApproval
{
    public class EventApprovalDto
    {
        public int ApprovalId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public int DirectorId { get; set; }
        public string DirectorName { get; set; } = null!;
        public string ApprovalStatus { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
