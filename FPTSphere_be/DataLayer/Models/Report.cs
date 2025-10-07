using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int? EventId { get; set; }

    public int? StaffId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event? Event { get; set; }

    public virtual ICollection<Qrcode> Qrcodes { get; set; } = new List<Qrcode>();

    public virtual User? Staff { get; set; }
}
