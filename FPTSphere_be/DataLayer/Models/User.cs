using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

[Index("Email", Name = "UQ__Users__AB6E61642F713FB0", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("google_id")]
    [StringLength(255)]
    public string? GoogleId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("class_code")]
    [StringLength(50)]
    public string? ClassCode { get; set; }

    [Column("is_authorized")]
    public bool? IsAuthorized { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Director")]
    public virtual ICollection<EventApproval> EventApprovals { get; set; } = new List<EventApproval>();

    [InverseProperty("User")]
    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    [InverseProperty("SentByNavigation")]
    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    [InverseProperty("User")]
    public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

    [InverseProperty("AssignedToNavigation")]
    public virtual ICollection<EventTask> EventTasks { get; set; } = new List<EventTask>();

    [InverseProperty("AssignByNavigation")]
    public virtual ICollection<EventTask> EventTasksAssignedBy { get; set; } = new List<EventTask>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual SystemRole Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<StudentFeedbackHeader> StudentFeedbackHeaders { get; set; } = new List<StudentFeedbackHeader>();
}
