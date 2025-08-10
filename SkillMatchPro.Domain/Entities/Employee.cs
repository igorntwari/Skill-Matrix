using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Domain.ValueObjects;

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
        public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

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

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    // Add these to Employee entity:

    private readonly List<ProjectAssignment> _projectAssignments = new();
    public IReadOnlyCollection<ProjectAssignment> ProjectAssignments => _projectAssignments.AsReadOnly();

    private readonly List<EmployeeAvailability> _availabilities = new();
    public IReadOnlyCollection<EmployeeAvailability> Availabilities => _availabilities.AsReadOnly();

    // Add these methods:
    public int GetCurrentAllocationPercentage()
    {
        var today = DateTime.UtcNow.Date;
        return _projectAssignments
            .Where(pa => pa.IsActive && pa.StartDate <= today && pa.EndDate >= today)
            .Sum(pa => pa.AllocationPercentage);
    }

    public bool IsAvailableForAllocation(int requiredPercentage)
    {
        var currentAllocation = GetCurrentAllocationPercentage();
        return (currentAllocation + requiredPercentage) <= 100;
    }

    public List<AvailabilitySlot> GetAvailabilityForDateRange(DateTime startDate, DateTime endDate)
    {
        var availability = new List<AvailabilitySlot>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            var specificAvailability = _availabilities
                .FirstOrDefault(a => a.Date == currentDate);

            if (specificAvailability != null)
            {
                availability.Add(new AvailabilitySlot(currentDate, specificAvailability.AvailableHours));
            }
            else
            {
                availability.Add(new AvailabilitySlot(currentDate, 8));
            }

            currentDate = currentDate.AddDays(1);
        }

        return availability;
    }

}