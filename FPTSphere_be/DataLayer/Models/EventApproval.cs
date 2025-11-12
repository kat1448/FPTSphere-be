using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventApproval
{
    [Key]
    [Column("approval_id")]
    public int ApprovalId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("director_id")]
    public int DirectorId { get; set; }

    [Column("approval_status")]
    [StringLength(50)]
    public string ApprovalStatus { get; set; } = null!;

    [Column("comment")]
    [StringLength(500)]
    public string? Comment { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DirectorId")]
    [InverseProperty("EventApprovals")]
    public virtual User Director { get; set; } = null!;

    [ForeignKey("EventId")]
    [InverseProperty("EventApprovals")]
    public virtual Event Event { get; set; } = null!;
}
