using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventApproval
{
    public class ApproveEventDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Approved budget must be positive")]
        public decimal? ApprovedBudget { get; set; }

        public bool IsConditional { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}
