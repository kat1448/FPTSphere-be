using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class Event
{
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("event_name")]
    [StringLength(255)]
    public string EventName { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("banner_url")]
    [StringLength(255)]
    public string? BannerUrl { get; set; }

    [Column("start_time", TypeName = "datetime")]
    public DateTime StartTime { get; set; }

    [Column("end_time", TypeName = "datetime")]
    public DateTime EndTime { get; set; }

    [Column("location_id")]
    public int? LocationId { get; set; }

    [Column("external_location_id")]
    public int? ExternalLocationId { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("parent_event_id")]
    public int? ParentEventId { get; set; }

    [Column("template_id")]
    public int? TemplateId { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }

    [Column("expected_attendees")]
    public int? ExpectedAttendees { get; set; }

    [Column("estimated_cost", TypeName = "decimal(15, 2)")]
    public decimal? EstimatedCost { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("type_id")]
    public int? TypeId { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<AttendanceToken> AttendanceTokens { get; set; } = new List<AttendanceToken>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Events")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("Event")]
    public virtual ICollection<EventAiresult> EventAiresults { get; set; } = new List<EventAiresult>();

    [InverseProperty("Event")]
    public virtual ICollection<EventApproval> EventApprovals { get; set; } = new List<EventApproval>();

    [InverseProperty("Event")]
    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    [InverseProperty("Event")]
    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    [InverseProperty("Event")]
    public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

    [InverseProperty("Event")]
    public virtual ICollection<EventResource> EventResources { get; set; } = new List<EventResource>();

    [InverseProperty("Event")]
    public virtual ICollection<EventTask> EventTasks { get; set; } = new List<EventTask>();

    [ForeignKey("ExternalLocationId")]
    [InverseProperty("Events")]
    public virtual ExternalLocation? ExternalLocation { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<ExternalService> ExternalServices { get; set; } = new List<ExternalService>();

    [InverseProperty("ParentEvent")]
    public virtual ICollection<Event> InverseParentEvent { get; set; } = new List<Event>();

    [ForeignKey("LocationId")]
    [InverseProperty("Events")]
    public virtual Location? Location { get; set; }

    [ForeignKey("ParentEventId")]
    [InverseProperty("InverseParentEvent")]
    public virtual Event? ParentEvent { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Events")]
    public virtual EventStatus Status { get; set; } = null!;

    [InverseProperty("Event")]
    public virtual ICollection<StudentFeedbackHeader> StudentFeedbackHeaders { get; set; } = new List<StudentFeedbackHeader>();

    [ForeignKey("TemplateId")]
    [InverseProperty("Events")]
    public virtual FeedbackTemplate? Template { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Events")]
    public virtual EventCategory? Category { get; set; }

    [ForeignKey("TypeId")]
    [InverseProperty("Events")]
    public virtual EventType? Type { get; set; }
}
