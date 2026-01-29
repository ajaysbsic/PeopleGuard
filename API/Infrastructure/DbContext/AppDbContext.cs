using EmployeeInvestigationSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.DbContext;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Investigation> Investigations { get; set; }
    public DbSet<InvestigationRemark> InvestigationRemarks { get; set; }
    public DbSet<InvestigationAttachment> InvestigationAttachments { get; set; }
    public DbSet<CaseHistory> CaseHistory { get; set; }
    public DbSet<WarningLetter> WarningLetters { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<QrToken> QrTokens { get; set; }
    public DbSet<QrSubmission> QrSubmissions { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveAttachment> LeaveAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Factory).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.Department, e.Factory });
        });

        modelBuilder.Entity<Investigation>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).IsRequired();
            entity.HasOne(i => i.Employee)
                .WithMany()
                .HasForeignKey(i => i.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(i => i.EmployeeId);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.IsDeleted);
        });

        modelBuilder.Entity<InvestigationRemark>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Remark).IsRequired();
            entity.HasOne(r => r.Investigation)
                .WithMany(i => i.Remarks)
                .HasForeignKey(r => r.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(r => r.InvestigationId);
        });

        modelBuilder.Entity<InvestigationAttachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.FilePath).IsRequired();
            entity.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
            entity.HasOne(a => a.Investigation)
                .WithMany(i => i.Attachments)
                .HasForeignKey(a => a.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(a => a.InvestigationId);
        });

        modelBuilder.Entity<WarningLetter>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.LetterContent).IsRequired();
            entity.Property(w => w.HtmlContent).IsRequired();
            entity.Property(w => w.Template).IsRequired().HasMaxLength(50);
            entity.Property(w => w.PdfPath).IsRequired().HasMaxLength(500);
            entity.HasOne(w => w.Investigation)
                .WithMany(i => i.WarningLetters)
                .HasForeignKey(w => w.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(w => w.Employee)
                .WithMany()
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(w => w.InvestigationId);
            entity.HasIndex(w => w.EmployeeId);
        });

        modelBuilder.Entity<CaseHistory>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Description).IsRequired().HasMaxLength(1000);
            entity.Property(h => h.OldValue).HasMaxLength(200);
            entity.Property(h => h.NewValue).HasMaxLength(200);
            entity.HasOne(h => h.Investigation)
                .WithMany(i => i.History)
                .HasForeignKey(h => h.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(h => h.InvestigationId);
            entity.HasIndex(h => h.CreatedAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserName).IsRequired().HasMaxLength(256);
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(128);
            entity.Property(a => a.EntityId).IsRequired().HasMaxLength(256);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Endpoint).IsRequired().HasMaxLength(256);
            entity.Property(a => a.HttpMethod).IsRequired().HasMaxLength(10);
            entity.Property(a => a.IpAddress).IsRequired().HasMaxLength(45);
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => a.UserId);
            entity.HasIndex(a => a.EntityId);
            entity.HasIndex(a => a.Action);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).IsRequired().HasMaxLength(512);
            entity.Property(r => r.ReplacedByToken).HasMaxLength(512);
            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(r => r.Token);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.ExpiresAtUtc);
        });

        modelBuilder.Entity<QrToken>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Token).IsRequired().HasMaxLength(100);
            entity.Property(q => q.TargetType).IsRequired().HasMaxLength(50);
            entity.Property(q => q.TargetId).IsRequired().HasMaxLength(100);
            entity.Property(q => q.Label).HasMaxLength(200);
            entity.HasIndex(q => q.Token).IsUnique();
            entity.HasIndex(q => q.ExpiresAt);
        });

        modelBuilder.Entity<QrToken>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Token).IsRequired().HasMaxLength(256);
            entity.Property(q => q.TargetType).IsRequired().HasMaxLength(50);
            entity.Property(q => q.TargetId).IsRequired().HasMaxLength(256);
            entity.HasIndex(q => q.Token).IsUnique();
            entity.HasIndex(q => q.ExpiresAt);
            entity.HasOne(q => q.Creator)
                .WithMany()
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QrSubmission>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Category).IsRequired().HasMaxLength(100);
            entity.Property(q => q.Message).IsRequired();
            entity.HasOne(q => q.Token)
                .WithMany()
                .HasForeignKey(q => q.TokenId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(q => q.Investigation)
                .WithMany()
                .HasForeignKey(q => q.RelatedInvestigationId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(q => q.TokenId);
            entity.HasIndex(q => q.CreatedAt);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.EmployeeId).IsRequired().HasMaxLength(100);
            entity.Property(l => l.EmployeeName).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Reason).HasMaxLength(2000);
            entity.Property(l => l.CreatedByName).IsRequired().HasMaxLength(256);
            entity.Property(l => l.ReviewRemark).HasMaxLength(2000);
            entity.Property(l => l.ReviewedByName).HasMaxLength(256);

            entity.Property(l => l.StartDate)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v))
                .HasColumnType("date");

            entity.Property(l => l.EndDate)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v))
                .HasColumnType("date");

            entity.HasMany(l => l.Attachments)
                .WithOne(a => a.LeaveRequest)
                .HasForeignKey(a => a.LeaveRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.Type);
            entity.HasIndex(l => l.StartDate);
            entity.HasIndex(l => l.CreatedAt);
        });

        modelBuilder.Entity<LeaveAttachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileId).IsRequired().HasMaxLength(128);
            entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.Url).IsRequired().HasMaxLength(512);
            entity.HasIndex(a => a.LeaveRequestId);
        });
    }
}
