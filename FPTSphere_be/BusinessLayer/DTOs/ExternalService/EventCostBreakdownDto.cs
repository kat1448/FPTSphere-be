using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalService
{
    public class EventCostBreakdownDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal? ExternalLocationCost { get; set; }
        public decimal ExternalServicesCost { get; set; }
        public decimal TotalCost { get; set; }
        public List<ServiceCostItemDto> Services { get; set; } = new();
    }

    /// <summary>
    /// Individual service cost item in breakdown
    /// </summary>
    public class ServiceCostItemDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string? ResourceType { get; set; }
        public decimal Cost { get; set; }
    }
}
