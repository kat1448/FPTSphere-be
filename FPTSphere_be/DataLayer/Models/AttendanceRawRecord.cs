using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

/// <summary>
/// Model lưu trữ dữ liệu từ Google Form/Sheet hoặc Excel file.
/// Dữ liệu này được map với Users trong hệ thống (nếu có) hoặc lưu dưới dạng Guest.
/// Dùng để hiển thị danh sách participants và quản lý trạng thái check-in/check-out.
/// </summary>
[Table("AttendanceRawRecords")]
public partial class AttendanceRawRecord
{
    [Key]
    [Column("raw_id")]
    public int RawId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("sub_event_id")]
    public int? SubEventId { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string? FullName { get; set; }

    [Column("sheet_row")]
    public int SheetRow { get; set; }

    [Column("check_type")]
    [StringLength(20)]
    public string CheckType { get; set; } = null!; // CHECKIN / CHECKOUT

    [Column("submitted_at", TypeName = "datetime")]
    public DateTime SubmittedAt { get; set; }

    [Column("is_guest")]
    public bool IsGuest { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("synced_at", TypeName = "datetime")]
    public DateTime SyncedAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("AttendanceRawRecords")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("SubEventId")]
    [InverseProperty("AttendanceRawRecordsAsSubEvent")]
    public virtual Event? SubEvent { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AttendanceRawRecords")]
    public virtual User? User { get; set; }
}

