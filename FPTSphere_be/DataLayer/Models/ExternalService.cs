using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class ExternalService
{
    public int ServiceId { get; set; }

    public int EventId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string? ResourceType { get; set; }

    public decimal? Cost { get; set; }

    public string? Note { get; set; }

    public virtual Event Event { get; set; } = null!;
}
