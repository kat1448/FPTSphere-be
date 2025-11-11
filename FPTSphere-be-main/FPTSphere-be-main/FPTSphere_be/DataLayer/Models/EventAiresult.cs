using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

[Table("EventAIResults")]
public partial class EventAiresult
{
    [Key]
    [Column("ai_result_id")]
    public int AiResultId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("sentiment_score", TypeName = "decimal(3, 2)")]
    public decimal? SentimentScore { get; set; }

    [Column("summary_text")]
    public string? SummaryText { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventAiresults")]
    public virtual Event Event { get; set; } = null!;
}
