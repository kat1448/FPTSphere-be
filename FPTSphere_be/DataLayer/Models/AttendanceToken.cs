using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class AttendanceToken
{
    public int TokenId { get; set; }

    public int EventId { get; set; }

    public Guid Nonce { get; set; }

    public DateTime ExpiresAt { get; set; }

    public int? UsedCount { get; set; }

    public virtual Event Event { get; set; } = null!;
}
