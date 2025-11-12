using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventStatus
{
    public class UpdateEventStatusDto
    {
        [Required(ErrorMessage = "Status name is required")]
        [MaxLength(50)]
        public string StatusName { get; set; } = null!;
    }
}
