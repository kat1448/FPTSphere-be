using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? GoogleId { get; set; }

    public int RoleId { get; set; }

    public string? ClassCode { get; set; }

    public bool? IsAuthorized { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EventApproval> EventApprovals { get; set; } = new List<EventApproval>();

    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

    public virtual ICollection<EventTask> EventTasks { get; set; } = new List<EventTask>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual SystemRole Role { get; set; } = null!;

    public virtual ICollection<StudentFeedbackHeader> StudentFeedbackHeaders { get; set; } = new List<StudentFeedbackHeader>();
}
