using System;
using System.Collections.Generic;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Data;

public partial class EventDbContext : DbContext
{
    public EventDbContext()
    {
    }

    public EventDbContext(DbContextOptions<EventDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceToken> AttendanceTokens { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventAiresult> EventAiresults { get; set; }

    public virtual DbSet<EventApproval> EventApprovals { get; set; }

    public virtual DbSet<EventAttendance> EventAttendances { get; set; }

    public virtual DbSet<EventInvitation> EventInvitations { get; set; }

    public virtual DbSet<EventLog> EventLogs { get; set; }

    public virtual DbSet<EventResource> EventResources { get; set; }

    public virtual DbSet<EventStatus> EventStatuses { get; set; }

    public virtual DbSet<EventTask> EventTasks { get; set; }

    public virtual DbSet<ExternalLocation> ExternalLocations { get; set; }

    public virtual DbSet<ExternalService> ExternalServices { get; set; }

    public virtual DbSet<FeedbackQuestion> FeedbackQuestions { get; set; }

    public virtual DbSet<FeedbackResponse> FeedbackResponses { get; set; }

    public virtual DbSet<FeedbackTemplate> FeedbackTemplates { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<StudentFeedbackHeader> StudentFeedbackHeaders { get; set; }

    public virtual DbSet<SystemRole> SystemRoles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Attendan__CB3C9E176B20B9C5");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.Nonce)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("nonce");
            entity.Property(e => e.UsedCount)
                .HasDefaultValue(0)
                .HasColumnName("used_count");

            entity.HasOne(d => d.Event).WithMany(p => p.AttendanceTokens)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__event__6754599E");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F72727B3FD7B");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.BannerUrl)
                .HasMaxLength(255)
                .HasColumnName("banner_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .HasColumnName("event_name");
            entity.Property(e => e.ExternalLocationId).HasColumnName("external_location_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ParentEventId).HasColumnName("parent_event_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__created___48CFD27E");

            entity.HasOne(d => d.ExternalLocation).WithMany(p => p.Events)
                .HasForeignKey(d => d.ExternalLocationId)
                .HasConstraintName("FK__Events__external__47DBAE45");

            entity.HasOne(d => d.Location).WithMany(p => p.Events)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Events__location__46E78A0C");

            entity.HasOne(d => d.ParentEvent).WithMany(p => p.InverseParentEvent)
                .HasForeignKey(d => d.ParentEventId)
                .HasConstraintName("FK__Events__parent_e__4AB81AF0");

            entity.HasOne(d => d.Status).WithMany(p => p.Events)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__status_i__49C3F6B7");

            entity.HasOne(d => d.Template).WithMany(p => p.Events)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_Events_FeedbackTemplates");
        });

        modelBuilder.Entity<EventAiresult>(entity =>
        {
            entity.HasKey(e => e.AiResultId).HasName("PK__EventAIR__1C12045BEBA7B1F9");

            entity.ToTable("EventAIResults");

            entity.Property(e => e.AiResultId).HasColumnName("ai_result_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.SentimentScore)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("sentiment_score");
            entity.Property(e => e.SummaryText).HasColumnName("summary_text");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAiresults)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAIRe__event__7A672E12");
        });

        modelBuilder.Entity<EventApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__EventApp__C94AE61A8B780E58");

            entity.Property(e => e.ApprovalId).HasColumnName("approval_id");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasColumnName("approval_status");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DirectorId).HasColumnName("director_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");

            entity.HasOne(d => d.Director).WithMany(p => p.EventApprovals)
                .HasForeignKey(d => d.DirectorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAppr__direc__5070F446");

            entity.HasOne(d => d.Event).WithMany(p => p.EventApprovals)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAppr__event__4F7CD00D");
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__EventAtt__20D6A968C1F127CA");

            entity.ToTable("EventAttendance");

            entity.HasIndex(e => new { e.EventId, e.UserId }, "UQ_EventAttendance").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.CheckinAt)
                .HasColumnType("datetime")
                .HasColumnName("checkin_at");
            entity.Property(e => e.CheckoutAt)
                .HasColumnType("datetime")
                .HasColumnName("checkout_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .HasDefaultValue("QR")
                .HasColumnName("method");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAttendances)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAtte__event__628FA481");

            entity.HasOne(d => d.User).WithMany(p => p.EventAttendances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAtte__user___6383C8BA");
        });

        modelBuilder.Entity<EventInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__EventInv__94B74D7C3E21C3D4");

            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.ClassCode)
                .HasMaxLength(50)
                .HasColumnName("class_code");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.SentBy).HasColumnName("sent_by");

            entity.HasOne(d => d.Event).WithMany(p => p.EventInvitations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventInvi__event__59063A47");

            entity.HasOne(d => d.SentByNavigation).WithMany(p => p.EventInvitations)
                .HasForeignKey(d => d.SentBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventInvi__sent___59FA5E80");
        });

        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__EventLog__9E2397E0AFA79590");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Details)
                .HasMaxLength(500)
                .HasColumnName("details");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventLogs)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventLogs__event__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.EventLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventLogs__user___5535A963");
        });

        modelBuilder.Entity<EventResource>(entity =>
        {
            entity.HasKey(e => e.EventResourceId).HasName("PK__EventRes__D41E187719DAC984");

            entity.Property(e => e.EventResourceId).HasColumnName("event_resource_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.QuantityUsed).HasColumnName("quantity_used");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventResources)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__event__01142BA1");

            entity.HasOne(d => d.Resource).WithMany(p => p.EventResources)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__resou__02084FDA");
        });

        modelBuilder.Entity<EventStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__EventSta__3683B53162090AA9");

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<EventTask>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__EventTas__0492148DE22B1F7D");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Report).HasColumnName("report");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.EventTasks)
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventTask__assig__5EBF139D");

            entity.HasOne(d => d.Event).WithMany(p => p.EventTasks)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventTask__event__5DCAEF64");
        });

        modelBuilder.Entity<ExternalLocation>(entity =>
        {
            entity.HasKey(e => e.ExternalLocationId).HasName("PK__External__2857FF41EBC3546B");

            entity.Property(e => e.ExternalLocationId).HasColumnName("external_location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(50)
                .HasColumnName("contact_phone");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
        });

        modelBuilder.Entity<ExternalService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__External__3E0DB8AF04F72F62");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.ProviderName)
                .HasMaxLength(255)
                .HasColumnName("provider_name");
            entity.Property(e => e.ResourceType)
                .HasMaxLength(100)
                .HasColumnName("resource_type");

            entity.HasOne(d => d.Event).WithMany(p => p.ExternalServices)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExternalS__event__04E4BC85");
        });

        modelBuilder.Entity<FeedbackQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Feedback__2EC215498F29ADD8");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Options)
                .HasMaxLength(500)
                .HasColumnName("options");
            entity.Property(e => e.QuestionText)
                .HasMaxLength(500)
                .HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasColumnName("question_type");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");

            entity.HasOne(d => d.Template).WithMany(p => p.FeedbackQuestions)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackQ__templ__6EF57B66");
        });

        modelBuilder.Entity<FeedbackResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__Feedback__EBECD896BC211F13");

            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__feedb__76969D2E");

            entity.HasOne(d => d.Question).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__quest__778AC167");
        });

        modelBuilder.Entity<FeedbackTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Feedback__BE44E079AEA099AA");

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EAAFC97CE2");

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Building)
                .HasMaxLength(100)
                .HasColumnName("building");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(50)
                .HasColumnName("room_number");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F221825BB");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__user___07C12930");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Resource__4985FC7321D051D1");

            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
        });

        modelBuilder.Entity<StudentFeedbackHeader>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__StudentF__7A6B2B8CAC767E30");

            entity.ToTable("StudentFeedbackHeader");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("submitted_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.StudentFeedbackHeaders)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentFe__event__71D1E811");

            entity.HasOne(d => d.User).WithMany(p => p.StudentFeedbackHeaders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentFe__user___72C60C4A");
        });

        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__SystemRo__760965CC5F2DC963");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F811078B8");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E61642B4B2A88").IsUnique();

            entity.HasIndex(e => e.GoogleId, "UQ__Users__CCBDE7DC689CC043").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ClassCode)
                .HasMaxLength(50)
                .HasColumnName("class_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .HasColumnName("google_id");
            entity.Property(e => e.IsAuthorized)
                .HasDefaultValue(true)
                .HasColumnName("is_authorized");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__role_id__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
