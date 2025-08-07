using System.Data;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Employee? Employee { get; private set; }

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = UserRole.Employee;
    }

    public User(string email, string passwordHash, UserRole role = UserRole.Employee)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void LinkToEmployee(Guid employeeId)
    {
        EmployeeId = employeeId;
    }

    public void ChangeRole(UserRole newRole, UserRole changerRole)
    {
        // Business rule: Only admins can create other admins
        if (newRole == UserRole.Admin && changerRole != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only administrators can create other administrators");
        }

        // Business rule: Can't demote yourself
        if (Role == UserRole.Admin && newRole != UserRole.Admin)
        {
            throw new InvalidOperationException("Administrators cannot demote themselves");
        }

        Role = newRole;
    }
}