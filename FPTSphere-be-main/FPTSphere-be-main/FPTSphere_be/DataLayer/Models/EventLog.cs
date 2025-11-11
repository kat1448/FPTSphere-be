using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventLog
{
    [Key]
    [Column("log_id")]
    public int LogId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("action")]
    [StringLength(100)]
    public string Action { get; set; } = null!;

    [Column("details")]
    [StringLength(500)]
    public string? Details { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventLogs")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("EventLogs")]
    public virtual User User { get; set; } = null!;
}
