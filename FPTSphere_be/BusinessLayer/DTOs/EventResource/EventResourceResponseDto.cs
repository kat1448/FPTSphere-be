using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventResource
{
    public class EventResourceResponseDto
    {
        public int EventResourceId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string? ResourceType { get; set; }
        public int AvailableQuantity { get; set; }
        public int QuantityUsed { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}

