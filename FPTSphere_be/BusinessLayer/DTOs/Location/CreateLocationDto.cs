using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Location
{
    public class CreateLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000")]
        public int? Capacity { get; set; }

        [MaxLength(100)]
        public string? Building { get; set; }

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
