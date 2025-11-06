using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    public class CreateEventDto
    {
        public string EventName { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ManagerUserId { get; set; }  // FK to User
        public int? ParentEventId { get; set; } // Optional sub-event
    }
}
