using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventResource
{
    public class AssignResourceToEventDto
    {
        [Required(ErrorMessage = "Resource ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid resource ID")]
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "Quantity used is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int QuantityUsed { get; set; }
    }

}
