using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventAttendance
{
    public int AttendanceId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public DateTime? CheckinAt { get; set; }

    public DateTime? CheckoutAt { get; set; }

    public string? Method { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
