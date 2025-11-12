using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalLocation
{
    public class ExternalLocationDto
    {
        public int ExternalLocationId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public decimal? Cost { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
    }
}
