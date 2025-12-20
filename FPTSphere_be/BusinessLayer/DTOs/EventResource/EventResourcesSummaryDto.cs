using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventResource
{
    public class EventResourcesSummaryDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalResourcesAssigned { get; set; }
        public int TotalQuantityUsed { get; set; }
        public List<EventResourceResponseDto> Resources { get; set; } = new();
    }
}
