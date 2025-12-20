using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalService
{
    public class ExternalServiceListDto
    {
        public int ServiceId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ResourceType { get; set; }
        public decimal? Cost { get; set; }
    }
}
