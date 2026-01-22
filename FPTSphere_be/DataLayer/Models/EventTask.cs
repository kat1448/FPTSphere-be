using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventTask
{
    [Key]
    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("assigned_to")]
    public int AssignedTo { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column("start_date", TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column("due_date", TypeName = "datetime")]
    public DateTime DueDate { get; set; }

    [Column("completed_at", TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    [Column("report")]
    public string? Report { get; set; }

    [Column("assign_by")]
    public int? AssignBy { get; set; }

    [Column("is_template")]
    public bool? IsTemplate { get; set; }

    [Column("parent_task_id")]
    public int? ParentTaskId { get; set; }

    [ForeignKey("AssignedTo")]
    [InverseProperty("EventTasks")]
    public virtual User AssignedToNavigation { get; set; } = null!;

    [ForeignKey("AssignBy")]
    [InverseProperty("EventTasksAssignedBy")]
    public virtual User? AssignByNavigation { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventTasks")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("ParentTaskId")]
    [InverseProperty("SubTasks")]
    public virtual EventTask? ParentTask { get; set; }

    [InverseProperty("ParentTask")]
    public virtual ICollection<EventTask> SubTasks { get; set; } = new List<EventTask>();
}
