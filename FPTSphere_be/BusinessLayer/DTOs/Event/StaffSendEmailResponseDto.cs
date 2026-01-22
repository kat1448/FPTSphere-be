using System.Collections.Generic;

namespace BusinessLayer.DTOs.Event
{
    public class StaffSendEmailResponseDto
    {
        public int TotalRecipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string>? FailedEmails { get; set; }
        public List<string>? SentEmails { get; set; }
    }
}

