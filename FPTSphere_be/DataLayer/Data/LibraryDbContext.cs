using System;
using System.Collections.Generic;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Data;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventAiresult> EventAiresults { get; set; }

    public virtual DbSet<EventInvitation> EventInvitations { get; set; }

    public virtual DbSet<EventTask> EventTasks { get; set; }

    public virtual DbSet<ExternalResource> ExternalResources { get; set; }

    public virtual DbSet<FeedbackTemplate> FeedbackTemplates { get; set; }

    public virtual DbSet<InternalResourcesUsage> InternalResourcesUsages { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<StudentFeedback> StudentFeedbacks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=EventManagementSystem;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__DA8918149EECF0FA");

            entity.HasIndex(e => new { e.EventId, e.UserId, e.RoleName }, "UQ_Assignment_Role").IsUnique();

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Assignment_Event");

            entity.HasOne(d => d.User).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignment_User");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F727E7FCC86D");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .HasColumnName("event_name");
            entity.Property(e => e.EventType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("event_type");
            entity.Property(e => e.ManagerUserId).HasColumnName("manager_user_id");
            entity.Property(e => e.ParentEventId).HasColumnName("parent_event_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");

            entity.HasOne(d => d.ManagerUser).WithMany(p => p.Events)
                .HasForeignKey(d => d.ManagerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Manager");

            entity.HasOne(d => d.ParentEvent).WithMany(p => p.InverseParentEvent)
                .HasForeignKey(d => d.ParentEventId)
                .HasConstraintName("FK_Events_Parent");
        });

        modelBuilder.Entity<EventAiresult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__EventAIR__AFB3C3161327AB5F");

            entity.ToTable("EventAIResults");

            entity.HasIndex(e => e.EventId, "UQ__EventAIR__2370F726AF972A90").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.AnalysisDate)
                .HasColumnType("datetime")
                .HasColumnName("analysis_date");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.KeyInsights).HasColumnName("key_insights");
            entity.Property(e => e.SentimentScore)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("sentiment_score");

            entity.HasOne(d => d.Event).WithOne(p => p.EventAiresult)
                .HasForeignKey<EventAiresult>(d => d.EventId)
                .HasConstraintName("FK_AIResults_Event");
        });

        modelBuilder.Entity<EventInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__EventInv__94B74D7CDDADF41A");

            entity.HasIndex(e => new { e.EventId, e.UserId }, "UQ_Invitation_User").IsUnique();

            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.CheckInTime)
                .HasColumnType("datetime")
                .HasColumnName("check_in_time");
            entity.Property(e => e.CheckOutTime)
                .HasColumnType("datetime")
                .HasColumnName("check_out_time");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventInvitations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Invitation_Event");

            entity.HasOne(d => d.User).WithMany(p => p.EventInvitations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_User");
        });

        modelBuilder.Entity<EventTask>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__0492148DE5616F0D");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssignedToUserId).HasColumnName("assigned_to_user_id");
            entity.Property(e => e.CompletionDate)
                .HasColumnType("datetime")
                .HasColumnName("completion_date");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TaskDescription).HasColumnName("task_description");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.EventTasks)
                .HasForeignKey(d => d.AssignedToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_User");

            entity.HasOne(d => d.Event).WithMany(p => p.EventTasks)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Task_Event");
        });

        modelBuilder.Entity<ExternalResource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__External__4985FC730CE71BBC");

            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.ActualCost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("actual_cost");
            entity.Property(e => e.ContractFileLink)
                .HasMaxLength(255)
                .HasColumnName("contract_file_link");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ExpectedCost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("expected_cost");
            entity.Property(e => e.ProviderName)
                .HasMaxLength(255)
                .HasColumnName("provider_name");
            entity.Property(e => e.ResourceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("resource_type");

            entity.HasOne(d => d.Event).WithMany(p => p.ExternalResources)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_External_Event");
        });

        modelBuilder.Entity<FeedbackTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Feedback__BE44E0792316C58A");

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("question_type");
        });

        modelBuilder.Entity<InternalResourcesUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__Internal__B6B13A022E3FAF6D");

            entity.ToTable("InternalResourcesUsage");

            entity.Property(e => e.UsageId).HasColumnName("usage_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ResourceName)
                .HasMaxLength(255)
                .HasColumnName("resource_name");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");

            entity.HasOne(d => d.Event).WithMany(p => p.InternalResourcesUsages)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Usage_Event");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EAA5C64584");

            entity.HasIndex(e => e.InternalCode, "UQ__Location__6B8395D63FF6B13C").IsUnique();

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.InternalCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("internal_code");
            entity.Property(e => e.IsBookable).HasColumnName("is_bookable");
            entity.Property(e => e.LocationName)
                .HasMaxLength(255)
                .HasColumnName("location_name");
            entity.Property(e => e.MaintenanceNote)
                .HasMaxLength(255)
                .HasColumnName("maintenance_note");
        });

        modelBuilder.Entity<StudentFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackRecordId).HasName("PK__StudentF__7C3282915E9DA901");

            entity.ToTable("StudentFeedback");

            entity.Property(e => e.FeedbackRecordId).HasColumnName("feedback_record_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ResponseValue).HasColumnName("response_value");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.StudentFeedbacks)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Feedback_Event");

            entity.HasOne(d => d.Template).WithMany(p => p.StudentFeedbacks)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedback_Template");

            entity.HasOne(d => d.User).WithMany(p => p.StudentFeedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedback_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F73E27A77");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164ED8D16C2").IsUnique();

            entity.HasIndex(e => e.GoogleId, "UQ__Users__CCBDE7DC504AEA2B").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ClassCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("class_code");
            entity.Property(e => e.DepartmentMajor)
                .HasMaxLength(100)
                .HasColumnName("department_major");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .HasColumnName("google_id");
            entity.Property(e => e.IsAuthorized).HasColumnName("is_authorized");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("user_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
