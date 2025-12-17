using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class ExternalLocation
{
    public int ExternalLocationId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? ContactPhone { get; set; }

    public decimal? Cost { get; set; }

    public string? Note { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
