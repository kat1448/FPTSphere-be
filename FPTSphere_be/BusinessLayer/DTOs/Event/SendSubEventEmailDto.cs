using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Event
{
    /// <summary>
    /// DTO for sending email to sub-event attendees
    /// </summary>
    public class SendSubEventEmailDto
    {
        [Required(ErrorMessage = "Email subject is required")]
        [MaxLength(500, ErrorMessage = "Subject cannot exceed 500 characters")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Email body is required")]
        public string Body { get; set; } = null!;

        /// <summary>
        /// Recipient type: "AllAttendees", "CheckedInOnly", "NotCheckedIn", "CustomList"
        /// - AllAttendees: All registered attendees for this sub-event
        /// - CheckedInOnly: Only attendees who have checked in
        /// - NotCheckedIn: Only attendees who have not checked in
        /// - CustomList: Custom list of emails (requires CustomEmailList)
        /// </summary>
        [Required(ErrorMessage = "Recipient type is required")]
        public string RecipientType { get; set; } = null!;

        /// <summary>
        /// Custom email list (REQUIRED when RecipientType is "CustomList", ignored otherwise)
        /// Must contain at least one valid email address
        /// </summary>
        public List<string>? CustomEmailList { get; set; }

        /// <summary>
        /// QR Code image as base64 string (optional, if QR code is already generated)
        /// </summary>
        public string? QrCodeBase64 { get; set; }

        /// <summary>
        /// QR Code URL (optional, if QR code URL is provided)
        /// </summary>
        public string? QrCodeUrl { get; set; }
    }

    /// <summary>
    /// Response DTO for sending email
    /// </summary>
    public class SendSubEventEmailResponseDto
    {
        public int SubEventId { get; set; }
        public string RecipientType { get; set; } = null!;
        public int TotalRecipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string>? FailedEmails { get; set; }
    }
}

