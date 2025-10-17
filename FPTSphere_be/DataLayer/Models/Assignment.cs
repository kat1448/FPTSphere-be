using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public string RoleName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
