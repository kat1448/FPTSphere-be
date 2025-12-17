using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class FeedbackTemplate
{
    public int TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<FeedbackQuestion> FeedbackQuestions { get; set; } = new List<FeedbackQuestion>();
}
