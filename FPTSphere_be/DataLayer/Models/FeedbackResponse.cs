using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class FeedbackResponse
{
    public int ResponseId { get; set; }

    public int FeedbackId { get; set; }

    public int QuestionId { get; set; }

    public string? AnswerText { get; set; }

    public virtual StudentFeedbackHeader Feedback { get; set; } = null!;

    public virtual FeedbackQuestion Question { get; set; } = null!;
}
