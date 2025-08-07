namespace SkillMatchPro.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Employee? Employee { get; private set; }

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public User(string email, string passwordHash, string role = "Employee")
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
}