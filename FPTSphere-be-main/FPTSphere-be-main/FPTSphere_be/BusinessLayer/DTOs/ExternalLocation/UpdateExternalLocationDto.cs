using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.ExternalLocation
{
    public class UpdateExternalLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(255)]
        public string Address { get; set; } = null!;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(50)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? ContactPhone { get; set; }

        [Range(0, 999999999, ErrorMessage = "Cost must be positive")]
        public decimal? Cost { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }

}
