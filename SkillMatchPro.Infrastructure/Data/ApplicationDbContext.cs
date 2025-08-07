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
    }
}