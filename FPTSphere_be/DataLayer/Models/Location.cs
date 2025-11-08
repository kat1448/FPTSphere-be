using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public int? Capacity { get; set; }

    public string? Building { get; set; }

    public string? RoomNumber { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
