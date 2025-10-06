using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SPMS.Models;

public partial class SpmsContext : DbContext
{
    private readonly IConfiguration _configuration;

    public SpmsContext(DbContextOptions<SpmsContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationStatus> ApplicationStatuses { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PermitType> PermitTypes { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workflow> Workflows { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use the connection string from configuration (e.g., appsettings.json)
            var connectionString = _configuration.GetConnectionString("SPMS");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4F792B0A98CE");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_Applications_Audit");
                    tb.HasTrigger("trg_Applications_Update_LastUpdatedAt");
                });

            entity.HasIndex(e => e.ApplicantId, "IX_Applications_ApplicantID");

            entity.HasIndex(e => e.ApprovedBy, "IX_Applications_ApprovedBy");

            entity.HasIndex(e => e.PermitTypeId, "IX_Applications_PermitTypeID");

            entity.HasIndex(e => e.StatusId, "IX_Applications_StatusID");

            entity.HasIndex(e => e.ReferenceNumber, "UQ__Applicat__C5ADBE4DDB4EEB9F").IsUnique();

            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.ApplicantId).HasColumnName("ApplicantID");
            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PermitTypeId).HasColumnName("PermitTypeID");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(50)
                .HasDefaultValueSql("(concat('APP-',format(sysdatetime(),'yyyyMMdd'),'-',right('00000'+CONVERT([nvarchar](10),NEXT VALUE FOR [dbo].[Seq_AppRef]),(5))))");
            entity.Property(e => e.State).HasMaxLength(200);
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1L)
                .HasColumnName("StatusID");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Town).HasMaxLength(200);

            entity.HasOne(d => d.Applicant).WithMany(p => p.ApplicationApplicants)
                .HasForeignKey(d => d.ApplicantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Applications_Applicant");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ApplicationApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_Applications_Approver");

            entity.HasOne(d => d.PermitType).WithMany(p => p.Applications)
                .HasForeignKey(d => d.PermitTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Applications_PermitType");

            entity.HasOne(d => d.Status).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Applications_Status");
        });

        modelBuilder.Entity<ApplicationStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Applicat__C8EE2043905EEF38");

            entity.HasIndex(e => e.Name, "UQ__Applicat__737584F635458E8E").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsFinal).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F23B8DE414A61");

            entity.HasIndex(e => e.ApplicationId, "IX_AuditLogs_ApplicationID");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserID");

            entity.Property(e => e.AuditId).HasColumnName("AuditID");
            entity.Property(e => e.Action).HasMaxLength(200);
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Application).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_AuditLogs_Applications");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK__Countrie__10D160BF661A6153");

            entity.HasIndex(e => e.Name, "UQ__Countrie__737584F62E4E7852").IsUnique();

            entity.HasIndex(e => e.Iso, "UQ__Countrie__C4979A230743892A").IsUnique();

            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Iso)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ISO");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF6FF799411A");

            entity.HasIndex(e => e.ApplicationId, "IX_Documents_ApplicationID");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Application).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documents_Applications");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32F79F1C15");

            entity.HasIndex(e => e.ApplicationId, "IX_Notifications_ApplicationID");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserID");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Application).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_Notifications_Applications");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<PermitType>(entity =>
        {
            entity.HasKey(e => e.PermitTypeId).HasName("PK__PermitTy__80E8C4CF396240D2");

            entity.ToTable(tb => tb.HasTrigger("trg_PermitTypes_Update_ModifiedAt"));

            entity.HasIndex(e => e.Name, "UQ__PermitTy__737584F6814B084F").IsUnique();

            entity.Property(e => e.PermitTypeId).HasColumnName("PermitTypeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PK__States__C3BA3B5A43B7EA9F");

            entity.HasIndex(e => e.CountryIso, "IX_States_CountryISO");

            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.CountryIso)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CountryISO");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.CountryIsoNavigation).WithMany(p => p.States)
                .HasPrincipalKey(p => p.Iso)
                .HasForeignKey(d => d.CountryIso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_States_Countries");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC8AD5972B");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_Update_ModifiedAt"));

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341E4185B9").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(100)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Town).HasMaxLength(200);
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.WorkflowId).HasName("PK__Workflow__5704A64A4EAEFACE");

            entity.ToTable("Workflow");

            entity.HasIndex(e => e.PermitTypeId, "IX_Workflow_PermitTypeID");

            entity.Property(e => e.WorkflowId).HasColumnName("WorkflowID");
            entity.Property(e => e.IsFinalStep).HasDefaultValue(false);
            entity.Property(e => e.PermitTypeId).HasColumnName("PermitTypeID");
            entity.Property(e => e.RoleAssigned).HasMaxLength(50);

            entity.HasOne(d => d.PermitType).WithMany(p => p.Workflows)
                .HasForeignKey(d => d.PermitTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Workflow_PermitTypes");
        });
        modelBuilder.HasSequence("Seq_AppRef");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
