using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectRequirement> ProjectRequirements { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure EmployeeSkill as a many-to-many relationship
        modelBuilder.Entity<EmployeeSkill>(entity =>
        {
            // This creates a composite key (EmployeeId + SkillId)
            entity.HasKey(es => new { es.EmployeeId, es.SkillId });
        });

        // This creates a composite key (EmployeeId + SkillId)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<User>(u => u.EmployeeId);
        });
    modelBuilder.Entity<Employee>()
        .HasQueryFilter(e => !e.IsDeleted);

    modelBuilder.Entity<Skill>()
        .HasQueryFilter(s => !s.IsDeleted);

        modelBuilder.Entity<EmployeeAvailability>(entity =>
        {
            entity.HasKey(ea => ea.Id);
            entity.HasIndex(ea => new { ea.EmployeeId, ea.Date }).IsUnique();

            entity.HasOne(ea => ea.Employee)
                .WithMany(e => e.Availabilities)
                .HasForeignKey(ea => ea.EmployeeId);
        });

        modelBuilder.Entity<ProjectAssignment>(entity =>
        {
            entity.HasKey(pa => pa.Id);

            entity.HasOne(pa => pa.Project)
                .WithMany(p => p.Assignments)
                .HasForeignKey(pa => pa.ProjectId);

            entity.HasOne(pa => pa.Employee)
                .WithMany(e => e.ProjectAssignments)
                .HasForeignKey(pa => pa.EmployeeId);

            entity.HasIndex(pa => new { pa.EmployeeId, pa.ProjectId, pa.IsActive });
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => new { n.UserId, n.IsRead });

            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);
        });
        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ProjectRequirement configuration
        modelBuilder.Entity<ProjectRequirement>(entity =>
        {
            entity.HasKey(pr => pr.Id);

            entity.HasOne(pr => pr.Project)
                .WithMany(p => p.Requirements)
                .HasForeignKey(pr => pr.ProjectId);

            entity.HasOne(pr => pr.Skill)
                .WithMany()
                .HasForeignKey(pr => pr.SkillId);
        });

    }
}