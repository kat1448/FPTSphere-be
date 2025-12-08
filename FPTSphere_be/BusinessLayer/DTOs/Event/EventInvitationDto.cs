using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    public class EventInvitationDto
    {
        public int InvitationId { get; set; }
        public int EventId { get; set; }
        public string ClassCode { get; set; } = null!;
        public int SentBy { get; set; }
        public DateTime? SentAt { get; set; }

        // Có thể dùng để hiển thị ai gửi
        public string? SentByName { get; set; }
        public string? SentByEmail { get; set; }

        // Bắt buộc / tự nguyện (chỉ để FE hiểu, dựa trên IsMandatory của request)
        public bool IsMandatory { get; set; }
    }
}
