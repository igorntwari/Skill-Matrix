using SkillMatchPro.Domain.Enums;
namespace SkillMatchPro.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Department { get; private set; }
    public string Title { get; private set; }
    public DateTime HiredDate { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<EmployeeSkill> _employeeSkills = new();
    public IReadOnlyCollection<EmployeeSkill> EmployeeSkills => _employeeSkills.AsReadOnly();

    private Employee() { }

    public Employee(string firstName, string lastName, string email, string department, string title)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email.ToLowerInvariant();
        Department = department;
        Title = title;
        HiredDate = DateTime.UtcNow;
        IsActive = true;
    }

    public void AddSkill(Skill skill, ProficiencyLevel proficiency)
    {
        if (_employeeSkills.Any(es => es.SkillId == skill.Id))
            throw new InvalidOperationException($"Employee already has skill: {skill.Name}");

        _employeeSkills.Add(new EmployeeSkill(Id, skill.Id, proficiency));
    }
}