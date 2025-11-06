using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventAiresult
{
    public int AiResultId { get; set; }

    public int EventId { get; set; }

    public decimal? SentimentScore { get; set; }

    public string? SummaryText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;
}
