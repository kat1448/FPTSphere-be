using System;

namespace BusinessLayer.DTOs.Event
{
    public class ParticipantDto
    {
        public int RawId { get; set; }
        public int? AttendanceId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Status { get; set; } = null!; // Checked In, Checked Out, Invited
        public string ParticipantType { get; set; } = null!; // User / Guest
        public int? UserId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class ParticipantListResponseDto
    {
        public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
        public int TotalCount { get; set; }
        public int CheckedInCount { get; set; }
        public int CheckedOutCount { get; set; }
        public int InvitedCount { get; set; }
        public int GuestCount { get; set; }
        public int UserCount { get; set; }
    }

    public class SyncAttendanceRequestDto
    {
        public int EventId { get; set; }
        public int? SubEventId { get; set; }
        public string? GoogleSheetId { get; set; }
        public string? GoogleFormUrl { get; set; }
    }

    public class UploadExcelRequestDto
    {
        public int EventId { get; set; }
        public int? SubEventId { get; set; }
    }

    /// <summary>
    /// Unified request DTO for syncing attendance data from either Excel file or Google Sheet
    /// </summary>
    public class SyncAttendanceUnifiedRequestDto
    {
        public int EventId { get; set; }
        public int? SubEventId { get; set; }
        /// <summary>
        /// Google Sheet ID (extracted from Google Sheet URL or Google Form's linked sheet)
        /// Example: From URL "https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit"
        /// The Sheet ID is: "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms"
        /// </summary>
        public string? GoogleSheetId { get; set; }
    }

    /// <summary>
    /// DTO for sync attendance endpoint with file upload support
    /// Used to fix Swagger documentation generation for multipart/form-data
    /// </summary>
    public class SyncAttendanceFormDto
    {
        public int EventId { get; set; }
        public int? SubEventId { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? File { get; set; }
        public string? GoogleSheetId { get; set; }
    }

    public class SyncAttendanceResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int NewRecordsCount { get; set; }
        public int UpdatedRecordsCount { get; set; }
        public int TotalRecordsCount { get; set; }
        public DateTime SyncedAt { get; set; }
    }
}

