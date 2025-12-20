using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

[Table("StudentFeedbackHeader")]
public partial class StudentFeedbackHeader
{
    [Key]
    [Column("feedback_id")]
    public int FeedbackId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("submitted_at", TypeName = "datetime")]
    public DateTime? SubmittedAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("StudentFeedbackHeaders")]
    public virtual Event Event { get; set; } = null!;

    [InverseProperty("Feedback")]
    public virtual ICollection<FeedbackResponse> FeedbackResponses { get; set; } = new List<FeedbackResponse>();

    [ForeignKey("UserId")]
    [InverseProperty("StudentFeedbackHeaders")]
    public virtual User User { get; set; } = null!;
}
