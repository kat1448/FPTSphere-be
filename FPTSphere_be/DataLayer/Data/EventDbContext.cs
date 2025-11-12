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

            entity.Property(e => e.Nonce).HasDefaultValueSql("(newid())");
            entity.Property(e => e.UsedCount).HasDefaultValue(0);

            entity.HasOne(d => d.Event).WithMany(p => p.AttendanceTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__event__6FE99F9F");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F7272207AEB8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__created___7B5B524B");

            entity.HasOne(d => d.ExternalLocation).WithMany(p => p.Events).HasConstraintName("FK__Events__external__7C4F7684");

            entity.HasOne(d => d.Location).WithMany(p => p.Events).HasConstraintName("FK__Events__location__7D439ABD");

            entity.HasOne(d => d.ParentEvent).WithMany(p => p.InverseParentEvent).HasConstraintName("FK__Events__parent_e__7E37BEF6");

            entity.HasOne(d => d.Status).WithMany(p => p.Events)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Events__status_i__7F2BE32F");

            entity.HasOne(d => d.Template).WithMany(p => p.Events).HasConstraintName("FK_Events_FeedbackTemplates");
        });

        modelBuilder.Entity<EventAiresult>(entity =>
        {
            entity.HasKey(e => e.AiResultId).HasName("PK__EventAIR__1C12045BE8C67A4E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAiresults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAIRe__event__70DDC3D8");
        });

        modelBuilder.Entity<EventApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__EventApp__C94AE61A7C3463C7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Director).WithMany(p => p.EventApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAppr__direc__71D1E811");

            entity.HasOne(d => d.Event).WithMany(p => p.EventApprovals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAppr__event__72C60C4A");
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__EventAtt__20D6A968773C49D9");

            entity.Property(e => e.Method).HasDefaultValue("QR");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAttendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAtte__event__73BA3083");

            entity.HasOne(d => d.User).WithMany(p => p.EventAttendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventAtte__user___74AE54BC");
        });

        modelBuilder.Entity<EventInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__EventInv__94B74D7CFBDA458C");

            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Event).WithMany(p => p.EventInvitations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventInvi__event__75A278F5");

            entity.HasOne(d => d.SentByNavigation).WithMany(p => p.EventInvitations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventInvi__sent___76969D2E");
        });

        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__EventLog__9E2397E01010126F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Event).WithMany(p => p.EventLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventLogs__event__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.EventLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventLogs__user___787EE5A0");
        });

        modelBuilder.Entity<EventResource>(entity =>
        {
            entity.HasKey(e => e.EventResourceId).HasName("PK__EventRes__D41E1877BE6392E3");

            entity.HasOne(d => d.Event).WithMany(p => p.EventResources)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__event__797309D9");

            entity.HasOne(d => d.Resource).WithMany(p => p.EventResources)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventReso__resou__7A672E12");
        });

        modelBuilder.Entity<EventStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__EventSta__3683B53102E9153C");
        });

        modelBuilder.Entity<EventTask>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__EventTas__0492148D5552673D");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.EventTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventTask__assig__01142BA1");

            entity.HasOne(d => d.Event).WithMany(p => p.EventTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventTask__event__02084FDA");
        });

        modelBuilder.Entity<ExternalLocation>(entity =>
        {
            entity.HasKey(e => e.ExternalLocationId).HasName("PK__External__2857FF41F4830AAF");
        });

        modelBuilder.Entity<ExternalService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__External__3E0DB8AF754CFFA1");

            entity.HasOne(d => d.Event).WithMany(p => p.ExternalServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExternalS__event__02FC7413");
        });

        modelBuilder.Entity<FeedbackQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Feedback__2EC21549E368F77D");

            entity.HasOne(d => d.Template).WithMany(p => p.FeedbackQuestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackQ__templ__03F0984C");
        });

        modelBuilder.Entity<FeedbackResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__Feedback__EBECD89679F1E1EE");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackResponses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__feedb__04E4BC85");

            entity.HasOne(d => d.Question).WithMany(p => p.FeedbackResponses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__quest__05D8E0BE");
        });

        modelBuilder.Entity<FeedbackTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Feedback__BE44E079E3E7AB00");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EACE128234");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FF98D8C62");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__user___06CD04F7");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Resource__4985FC7369C135D4");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StudentFeedbackHeader>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__StudentF__7A6B2B8CFBC5A42B");

            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Event).WithMany(p => p.StudentFeedbackHeaders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentFe__event__07C12930");

            entity.HasOne(d => d.User).WithMany(p => p.StudentFeedbackHeaders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentFe__user___08B54D69");
        });

        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__SystemRo__760965CCCBFCA0FB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F9AFCB77C");

            entity.HasIndex(e => e.GoogleId, "UQ_Users_GoogleId")
                .IsUnique()
                .HasFilter("([google_id] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsAuthorized).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__role_id__09A971A2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
