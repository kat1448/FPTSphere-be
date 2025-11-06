using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class FeedbackQuestion
{
    public int QuestionId { get; set; }

    public int TemplateId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public string? Options { get; set; }

    public virtual ICollection<FeedbackResponse> FeedbackResponses { get; set; } = new List<FeedbackResponse>();

    public virtual FeedbackTemplate Template { get; set; } = null!;
}
