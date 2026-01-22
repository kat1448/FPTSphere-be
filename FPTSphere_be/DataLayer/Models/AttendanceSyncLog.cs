using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

/// <summary>
/// Model lưu trữ log đồng bộ dữ liệu từ Google Form/Sheet hoặc Excel file.
/// Mỗi lần sync sẽ tạo các bản ghi log để theo dõi nguồn dữ liệu và thời điểm sync.
/// Dùng để quản lý lịch sử đồng bộ và xác định nguồn dữ liệu cho các lần refresh tiếp theo.
/// </summary>
[Table("AttendanceSyncLogs")]
public partial class AttendanceSyncLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("sub_event_id")]
    public int? SubEventId { get; set; }

    [Column("participant_key")]
    [StringLength(255)]
    public string ParticipantKey { get; set; } = null!; // Email / MSSV / Phone

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("check_type")]
    [StringLength(20)]
    public string CheckType { get; set; } = null!; // CHECK_IN / CHECK_OUT

    [Column("form_submitted_at", TypeName = "datetime")]
    public DateTime FormSubmittedAt { get; set; }

    [Column("sheet_row_index")]
    public int SheetRowIndex { get; set; }

    [Column("synced_at", TypeName = "datetime")]
    public DateTime SyncedAt { get; set; }

    [Column("source")]
    [StringLength(50)]
    public string Source { get; set; } = "GOOGLE_FORM"; // GOOGLE_FORM / EXCEL_FILE

    [Column("google_sheet_id")]
    [StringLength(500)]
    public string? GoogleSheetId { get; set; }

    [Column("excel_file_url")]
    [StringLength(500)]
    public string? ExcelFileUrl { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("AttendanceSyncLogs")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("SubEventId")]
    [InverseProperty("AttendanceSyncLogsAsSubEvent")]
    public virtual Event? SubEvent { get; set; }
}

