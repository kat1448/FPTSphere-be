using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Resource
{
    public int ResourceId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public int Quantity { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<EventResource> EventResources { get; set; } = new List<EventResource>();
}
