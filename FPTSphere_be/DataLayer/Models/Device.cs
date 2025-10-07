using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Device
{
    public int DeviceId { get; set; }

    public string? DeviceName { get; set; }

    public string? DeviceType { get; set; }

    public string? Status { get; set; }

    public int? LocationId { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Location? Location { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
