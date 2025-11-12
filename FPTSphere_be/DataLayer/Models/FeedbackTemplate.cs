using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class FeedbackTemplate
{
    [Key]
    [Column("template_id")]
    public int TemplateId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    [InverseProperty("Template")]
    public virtual ICollection<FeedbackQuestion> FeedbackQuestions { get; set; } = new List<FeedbackQuestion>();
}
