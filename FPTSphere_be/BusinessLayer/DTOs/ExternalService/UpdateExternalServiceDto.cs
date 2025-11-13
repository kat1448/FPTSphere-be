using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalServices
{
    public class UpdateExternalServiceDto
    {
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Provider name must be between 2 and 255 characters")]
        public string? ProviderName { get; set; }

        [StringLength(100, ErrorMessage = "Resource type cannot exceed 100 characters")]
        public string? ResourceType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal? Cost { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}
