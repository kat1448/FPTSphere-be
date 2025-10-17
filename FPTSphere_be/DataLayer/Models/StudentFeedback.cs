using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class StudentFeedback
{
    public int FeedbackRecordId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public int TemplateId { get; set; }

    public string? ResponseValue { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual FeedbackTemplate Template { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
