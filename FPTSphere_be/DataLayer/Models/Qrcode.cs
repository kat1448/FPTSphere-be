using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Qrcode
{
    public int QrId { get; set; }

    public int? ReportId { get; set; }

    public string? QrCodeData { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Report? Report { get; set; }
}
