using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class StudentFeedbackHeader
{
    public int FeedbackId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<FeedbackResponse> FeedbackResponses { get; set; } = new List<FeedbackResponse>();

    public virtual User User { get; set; } = null!;
}
