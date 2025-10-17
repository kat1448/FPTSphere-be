using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public int Capacity { get; set; }

    public string InternalCode { get; set; } = null!;

    public bool IsBookable { get; set; }

    public string? MaintenanceNote { get; set; }
}
