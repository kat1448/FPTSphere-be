using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventResource
{
    public class UpdateEventResourceDto
    {
        [Required(ErrorMessage = "Quantity used is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int QuantityUsed { get; set; }
    }
}
