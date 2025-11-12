using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Location
{
    public class LocationDto
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = null!;
        public int? Capacity { get; set; }
        public string? Building { get; set; }
        public string? RoomNumber { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}
