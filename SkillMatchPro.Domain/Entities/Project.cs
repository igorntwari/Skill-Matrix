using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Department { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Priority { get; private set; } // 1-5

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    private readonly List<ProjectRequirement> _requirements = new();
    public IReadOnlyCollection<ProjectRequirement> Requirements => _requirements.AsReadOnly();

    private readonly List<ProjectAssignment> _assignments = new();
    public IReadOnlyCollection<ProjectAssignment> Assignments => _assignments.AsReadOnly();

    private Project()
    {
        Name = string.Empty;
        Description = string.Empty;
        Department = string.Empty;
        CreatedBy = string.Empty;
    }

    public Project(string name, string description, string department,
        DateTime startDate, DateTime endDate, string createdBy, int priority = 3)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required");
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");
        if (priority < 1 || priority > 5)
            throw new ArgumentException("Priority must be between 1 and 5");

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Department = department;
        StartDate = startDate;
        EndDate = endDate;
        Priority = priority;
        Status = ProjectStatus.Planning;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void AddRequirement(Guid skillId, ProficiencyLevel minProficiency, int requiredCount)
    {
        if (_requirements.Any(r => r.SkillId == skillId))
            throw new InvalidOperationException("Skill requirement already exists for this project");

        _requirements.Add(new ProjectRequirement(Id, skillId, minProficiency, requiredCount));
    }

    public void UpdateStatus(ProjectStatus newStatus, string updatedBy)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }
}