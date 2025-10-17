using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class ExternalResource
{
    public int ResourceId { get; set; }

    public int EventId { get; set; }

    public string ResourceType { get; set; } = null!;

    public string ProviderName { get; set; } = null!;

    public decimal ExpectedCost { get; set; }

    public decimal? ActualCost { get; set; }

    public string? ContractFileLink { get; set; }

    public virtual Event Event { get; set; } = null!;
}
