using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class FeedbackResponse
{
    [Key]
    [Column("response_id")]
    public int ResponseId { get; set; }

    [Column("feedback_id")]
    public int FeedbackId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [ForeignKey("FeedbackId")]
    [InverseProperty("FeedbackResponses")]
    public virtual StudentFeedbackHeader Feedback { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("FeedbackResponses")]
    public virtual FeedbackQuestion Question { get; set; } = null!;
}
