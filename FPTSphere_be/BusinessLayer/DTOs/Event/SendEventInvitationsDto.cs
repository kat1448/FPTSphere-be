using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    /// <summary>
    /// DTO gửi thư mời tham gia sự kiện
    /// </summary>
    public class SendEventInvitationsDto
    {
        /// <summary>
        /// Các class_code được mời (thường là bắt buộc)
        /// </summary>
        public List<string> ClassCodes { get; set; } = new();

        /// <summary>
        /// Mời người trong hệ thống theo UserId (giảng viên / sinh viên đã có account)
        /// </summary>
        public List<int> InternalUserIds { get; set; } = new();

        /// <summary>
        /// Mời người ngoài hệ thống (chỉ có email / tên)
        /// </summary>
        public List<ExternalInviteDto> ExternalInvites { get; set; } = new();

        /// <summary>
        /// true = bắt buộc tham gia (như buổi học)
        /// false = tự nguyện / khuyến khích tham gia
        /// (áp dụng cho tất cả invite trong request này)
        /// </summary>
        [Required]
        public bool IsMandatory { get; set; }
    }

    public class ExternalInviteDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        public string? FullName { get; set; }
    }
}
