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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=BOSS-KHANH;Database=EventManagementSystem;User Id=sa;Password=vip0976109385;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Attendan__CB3C9E17128C6162");

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
                .HasConstraintName("FK__Attendanc__event__6FE99F9F");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F7272207AEB8");

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
            entity.Property(e => e.EstimatedCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("estimated_cost");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .HasColumnName("event_name");
            entity.Property(e => e.ExpectedAttendees).HasColumnName("expected_attendees");
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
                .HasConstraintName("FK__Events__created___7B5B524B");

            entity.HasOne(d => d.ExternalLocation).WithMany(p => p.Events)
                .HasForeignKey(d => d.ExternalLocationId)
                .HasConstraintName("FK__Events__external__7C4F7684");

            entity.HasOne(d => d.Location).WithMany(p => p.Events)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Events__location__7D439ABD");

            entity.HasOne(d => d.ParentEvent).WithMany(p => p.InverseParentEvent)
                .HasForeignKey(d => d.ParentEventId)
                .HasConstraintName("FK__Events__parent_e__7E37BEF6");

            entity.HasOne(d => d.Status).WithMany(p => p.Events)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__status_i__7F2BE32F");

            entity.HasOne(d => d.Template).WithMany(p => p.Events)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_Events_FeedbackTemplates");
        });

        modelBuilder.Entity<EventAiresult>(entity =>
        {
            entity.HasKey(e => e.AiResultId).HasName("PK__EventAIR__1C12045BE8C67A4E");

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
                .HasConstraintName("FK__EventAIRe__event__70DDC3D8");
        });

        modelBuilder.Entity<EventApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__EventApp__C94AE61A7C3463C7");

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
                .HasConstraintName("FK__EventAppr__direc__71D1E811");

            entity.HasOne(d => d.Event).WithMany(p => p.EventApprovals)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAppr__event__72C60C4A");
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__EventAtt__20D6A968773C49D9");

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
                .HasConstraintName("FK__EventAtte__event__73BA3083");

            entity.HasOne(d => d.User).WithMany(p => p.EventAttendances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAtte__user___74AE54BC");
        });

        modelBuilder.Entity<EventInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__EventInv__94B74D7CFBDA458C");

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
                .HasConstraintName("FK__EventInvi__event__75A278F5");

            entity.HasOne(d => d.SentByNavigation).WithMany(p => p.EventInvitations)
                .HasForeignKey(d => d.SentBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventInvi__sent___76969D2E");
        });

        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__EventLog__9E2397E01010126F");

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
                .HasConstraintName("FK__EventLogs__event__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.EventLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventLogs__user___787EE5A0");
        });

        modelBuilder.Entity<EventResource>(entity =>
        {
            entity.HasKey(e => e.EventResourceId).HasName("PK__EventRes__D41E1877BE6392E3");

            entity.Property(e => e.EventResourceId).HasColumnName("event_resource_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.QuantityUsed).HasColumnName("quantity_used");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventResources)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__event__797309D9");

            entity.HasOne(d => d.Resource).WithMany(p => p.EventResources)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__resou__7A672E12");
        });

        modelBuilder.Entity<EventStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__EventSta__3683B53102E9153C");

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<EventTask>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__EventTas__0492148D5552673D");

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
            entity.Property(e => e.IsTemplate).HasColumnName("is_template");
            entity.Property(e => e.ParentTaskId).HasColumnName("parent_task_id");
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
                .HasConstraintName("FK__EventTask__assig__01142BA1");

            entity.HasOne(d => d.Event).WithMany(p => p.EventTasks)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventTask__event__02084FDA");

            entity.HasOne(d => d.ParentTask).WithMany(p => p.InverseParentTask)
                .HasForeignKey(d => d.ParentTaskId)
                .HasConstraintName("FK_EventTasks_Parent");
        });

        modelBuilder.Entity<ExternalLocation>(entity =>
        {
            entity.HasKey(e => e.ExternalLocationId).HasName("PK__External__2857FF41F4830AAF");

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
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
        });

        modelBuilder.Entity<ExternalService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__External__3E0DB8AF754CFFA1");

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
                .HasConstraintName("FK__ExternalS__event__02FC7413");
        });

        modelBuilder.Entity<FeedbackQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Feedback__2EC21549E368F77D");

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
                .HasConstraintName("FK__FeedbackQ__templ__03F0984C");
        });

        modelBuilder.Entity<FeedbackResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__Feedback__EBECD89679F1E1EE");

            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__feedb__04E4BC85");

            entity.HasOne(d => d.Question).WithMany(p => p.FeedbackResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__quest__05D8E0BE");
        });

        modelBuilder.Entity<FeedbackTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Feedback__BE44E079E3E7AB00");

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
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EACE128234");

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Building)
                .HasMaxLength(100)
                .HasColumnName("building");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
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
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FF98D8C62");

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
                .HasConstraintName("FK__Notificat__user___06CD04F7");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Resource__4985FC7369C135D4");

            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
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
            entity.HasKey(e => e.FeedbackId).HasName("PK__StudentF__7A6B2B8CFBC5A42B");

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
                .HasConstraintName("FK__StudentFe__event__07C12930");

            entity.HasOne(d => d.User).WithMany(p => p.StudentFeedbackHeaders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentFe__user___08B54D69");
        });

        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__SystemRo__760965CCCBFCA0FB");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F9AFCB77C");

            entity.HasIndex(e => e.GoogleId, "UQ_Users_GoogleId")
                .IsUnique()
                .HasFilter("([google_id] IS NOT NULL)");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E61642F713FB0").IsUnique();

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
                .HasConstraintName("FK__Users__role_id__09A971A2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
