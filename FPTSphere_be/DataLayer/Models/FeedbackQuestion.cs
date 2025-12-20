using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class FeedbackQuestion
{
    [Key]
    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("template_id")]
    public int TemplateId { get; set; }

    [Column("question_text")]
    [StringLength(500)]
    public string QuestionText { get; set; } = null!;

    [Column("question_type")]
    [StringLength(50)]
    public string QuestionType { get; set; } = null!;

    [Column("options")]
    [StringLength(500)]
    public string? Options { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<FeedbackResponse> FeedbackResponses { get; set; } = new List<FeedbackResponse>();

    [ForeignKey("TemplateId")]
    [InverseProperty("FeedbackQuestions")]
    public virtual FeedbackTemplate Template { get; set; } = null!;
}
