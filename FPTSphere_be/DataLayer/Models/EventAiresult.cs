using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventAiresult
{
    public int ResultId { get; set; }

    public int EventId { get; set; }

    public DateTime AnalysisDate { get; set; }

    public decimal SentimentScore { get; set; }

    public string? KeyInsights { get; set; }

    public virtual Event Event { get; set; } = null!;
}
