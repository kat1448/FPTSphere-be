using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Resource
{
    public class ResourceDto
    {
        public int ResourceId { get; set; }
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public int Quantity { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateResourceDto
    {
        [Required(ErrorMessage = "Resource name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? Type { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateResourceDto
    {
        [Required(ErrorMessage = "Resource name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? Type { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
