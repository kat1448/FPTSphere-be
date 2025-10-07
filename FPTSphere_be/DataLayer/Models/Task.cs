using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int? EventId { get; set; }

    public string? TaskName { get; set; }

    public string? Description { get; set; }

    public int? AssignedTo { get; set; }

    public string? Status { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Event? Event { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
