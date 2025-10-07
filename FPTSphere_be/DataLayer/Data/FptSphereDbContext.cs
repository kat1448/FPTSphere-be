using System;
using System.Collections.Generic;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Data;

public partial class FptSphereDbContext : DbContext
{
    public FptSphereDbContext()
    {
    }

    public FptSphereDbContext(DbContextOptions<FptSphereDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventRegistration> EventRegistrations { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<OauthLogin> OauthLogins { get; set; }

    public virtual DbSet<Qrcode> Qrcodes { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SubEvent> SubEvents { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=FPTSphere;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__Devices__3B085D8BCE2B03DB");

            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("device_name");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("device_type");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Devices)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Devices__created__48CFD27E");

            entity.HasOne(d => d.Location).WithMany(p => p.Devices)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Devices__locatio__47DBAE45");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F727F1B48434");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EventName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("event_name");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT")
                .HasColumnName("status");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Events__created___4D94879B");
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__EventReg__22A298F6D53C251A");

            entity.Property(e => e.RegistrationId).HasColumnName("registration_id");
            entity.Property(e => e.CheckinTime)
                .HasColumnType("datetime")
                .HasColumnName("checkin_time");
            entity.Property(e => e.CheckoutTime)
                .HasColumnType("datetime")
                .HasColumnName("checkout_time");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Event).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__EventRegi__event__693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__EventRegi__user___6A30C649");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EAC93297F0");

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LocationName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("location_name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Locations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Locations__creat__4316F928");
        });

        modelBuilder.Entity<OauthLogin>(entity =>
        {
            entity.HasKey(e => e.OauthId).HasName("PK__OAuth_Lo__C579A02C31EEFAAB");

            entity.ToTable("OAuth_Login");

            entity.Property(e => e.OauthId).HasColumnName("oauth_id");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("provider");
            entity.Property(e => e.ProviderUid)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("provider_uid");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.OauthLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__OAuth_Log__user___3E52440B");
        });

        modelBuilder.Entity<Qrcode>(entity =>
        {
            entity.HasKey(e => e.QrId).HasName("PK__QRCodes__6CD5101B09A194F2");

            entity.ToTable("QRCodes");

            entity.Property(e => e.QrId).HasColumnName("qr_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.QrCodeData).HasColumnName("qr_code_data");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.Qrcodes)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK__QRCodes__report___66603565");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__779B7C585863F789");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Event).WithMany(p => p.Reports)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Reports__event_i__619B8048");

            entity.HasOne(d => d.Staff).WithMany(p => p.Reports)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Reports__staff_i__628FA481");
        });

        modelBuilder.Entity<SubEvent>(entity =>
        {
            entity.HasKey(e => e.SubeventId).HasName("PK__SubEvent__D9E570EE68D1FCC5");

            entity.Property(e => e.SubeventId).HasColumnName("subevent_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.SubeventName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("subevent_name");

            entity.HasOne(d => d.Event).WithMany(p => p.SubEvents)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__SubEvents__event__5070F446");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__0492148D64C3B41A");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("TODO")
                .HasColumnName("status");
            entity.Property(e => e.TaskName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("task_name");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK__Tasks__assigned___5629CD9C");

            entity.HasOne(d => d.Event).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Tasks__event_id__5535A963");

            entity.HasMany(d => d.Devices).WithMany(p => p.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskDevice",
                    r => r.HasOne<Device>().WithMany()
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskDevic__devic__59FA5E80"),
                    l => l.HasOne<Task>().WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskDevic__task___59063A47"),
                    j =>
                    {
                        j.HasKey("TaskId", "DeviceId").HasName("PK__TaskDevi__A7229155D412DC3E");
                        j.ToTable("TaskDevices");
                        j.IndexerProperty<int>("TaskId").HasColumnName("task_id");
                        j.IndexerProperty<int>("DeviceId").HasColumnName("device_id");
                    });

            entity.HasMany(d => d.Locations).WithMany(p => p.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskLocation",
                    r => r.HasOne<Location>().WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskLocat__locat__5DCAEF64"),
                    l => l.HasOne<Task>().WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskLocat__task___5CD6CB2B"),
                    j =>
                    {
                        j.HasKey("TaskId", "LocationId").HasName("PK__TaskLoca__B3E39793E514F2D0");
                        j.ToTable("TaskLocations");
                        j.IndexerProperty<int>("TaskId").HasColumnName("task_id");
                        j.IndexerProperty<int>("LocationId").HasColumnName("location_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F6351D8DD");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E61645C6EB2FC").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
