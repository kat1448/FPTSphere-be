using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalService
{
    public class ExternalServiceDto
    {
        public int ServiceId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string? ResourceType { get; set; }
        public decimal? Cost { get; set; }
        public string? Note { get; set; }
    }
}
