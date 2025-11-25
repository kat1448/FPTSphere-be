using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventApproval
{
    public class RejectEventDto
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = null!;

        public List<string>? SelectedReasons { get; set; }
    }
}
