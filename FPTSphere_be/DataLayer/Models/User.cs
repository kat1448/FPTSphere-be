using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class User
{
    public int UserId { get; set; }

    public string GoogleId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public bool IsAuthorized { get; set; }

    public string? DepartmentMajor { get; set; }

    public string? ClassCode { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<StudentFeedback> StudentFeedbacks { get; set; } = new List<StudentFeedback>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
