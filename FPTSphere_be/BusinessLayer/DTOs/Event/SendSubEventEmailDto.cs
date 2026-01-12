using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Event
{
    /// <summary>
    /// DTO for sending email to sub-event attendees
    /// Updated: Removed QR code and recipientType, added image upload and Excel import
    /// </summary>
    public class SendSubEventEmailDto
    {
        [Required(ErrorMessage = "Email subject is required")]
        [MaxLength(500, ErrorMessage = "Subject cannot exceed 500 characters")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Email body is required")]
        public string Body { get; set; } = null!;

        /// <summary>
        /// Image file to be uploaded to Cloudinary and embedded in email (optional)
        /// </summary>
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }

        /// <summary>
        /// Excel file containing email addresses (only Email column will be extracted)
        /// </summary>
        public Microsoft.AspNetCore.Http.IFormFile? ExcelFile { get; set; }

        /// <summary>
        /// Custom email list (optional, can be used instead of Excel file)
        /// </summary>
        public List<string>? CustomEmailList { get; set; }
    }

    /// <summary>
    /// Response DTO for sending email
    /// </summary>
    public class SendSubEventEmailResponseDto
    {
        public int SubEventId { get; set; }
        public int TotalRecipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string>? FailedEmails { get; set; }
        public List<string>? ImportedEmails { get; set; }
        public string? ImageUrl { get; set; }
    }
}
