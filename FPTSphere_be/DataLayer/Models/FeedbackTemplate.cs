using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class FeedbackTemplate
{
    public int TemplateId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public virtual ICollection<StudentFeedback> StudentFeedbacks { get; set; } = new List<StudentFeedback>();
}
