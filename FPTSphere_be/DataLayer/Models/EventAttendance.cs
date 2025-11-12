using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

[Table("EventAttendance")]
[Index("EventId", "UserId", Name = "UQ_EventAttendance", IsUnique = true)]
public partial class EventAttendance
{
    [Key]
    [Column("attendance_id")]
    public int AttendanceId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("checkin_at", TypeName = "datetime")]
    public DateTime? CheckinAt { get; set; }

    [Column("checkout_at", TypeName = "datetime")]
    public DateTime? CheckoutAt { get; set; }

    [Column("method")]
    [StringLength(50)]
    public string? Method { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventAttendances")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("EventAttendances")]
    public virtual User User { get; set; } = null!;
}
