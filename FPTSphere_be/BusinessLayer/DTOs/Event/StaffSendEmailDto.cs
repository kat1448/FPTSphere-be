using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Event
{
    public class StaffSendEmailDto
    {
        [Required(ErrorMessage = "Email list is required")]
        public List<string> EmailList { get; set; } = new List<string>();

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; } = string.Empty;
    }
}

