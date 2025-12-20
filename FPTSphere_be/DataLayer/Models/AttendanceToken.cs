using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class AttendanceToken
{
    [Key]
    [Column("token_id")]
    public int TokenId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("nonce")]
    public Guid Nonce { get; set; }

    [Column("expires_at", TypeName = "datetime")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_count")]
    public int? UsedCount { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("AttendanceTokens")]
    public virtual Event Event { get; set; } = null!;
}
