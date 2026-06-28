using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for the Portfolio ecosystem.
/// Integrates ASP.NET Core Identity tables with domain entities.
/// </summary>
public class PortfolioDbContext : IdentityDbContext<ApplicationUser>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Required for Identity tables

        // ─── UserProfile ───────────────────────────────────────────
        builder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Bio).HasMaxLength(2000);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.GitHubUrl).HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
            entity.Property(e => e.IdentityUserId).IsRequired().HasMaxLength(450);

            entity.HasIndex(e => e.IdentityUserId).IsUnique();

            entity.HasMany(e => e.Projects)
                .WithOne(p => p.UserProfile)
                .HasForeignKey(p => p.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Project ───────────────────────────────────────────────
        builder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.TechnologyStack).HasMaxLength(500);
            entity.Property(e => e.RepositoryUrl).HasMaxLength(500);
            entity.Property(e => e.LiveUrl).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(e => e.UserProfileId);
            entity.HasIndex(e => e.Status);

            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── TaskItem ──────────────────────────────────────────────
        builder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Ignore the computed property — it cannot be mapped to a column
            entity.Ignore(e => e.TotalTimeSpent);

            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);

            entity.HasMany(e => e.WorkSessions)
                .WithOne(ws => ws.TaskItem)
                .HasForeignKey(ws => ws.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── WorkSession ───────────────────────────────────────────
        builder.Entity<WorkSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Ignore the computed Duration property
            entity.Ignore(e => e.Duration);

            entity.HasIndex(e => e.TaskItemId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTime);
        });

        // ─── ApplicationUser extensions ────────────────────────────
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
        });
    }
}
