namespace SkillMatchPro.Domain.Entities;

public class ProjectAssignment
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Role { get; private set; }
    public int AllocationPercentage { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    // Audit
    public DateTime AssignedAt { get; private set; }
    public string AssignedBy { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Employee Employee { get; private set; } = null!;

    private ProjectAssignment()
    {
        Role = string.Empty;
        AssignedBy = string.Empty;
    }

    public ProjectAssignment(Guid projectId, Guid employeeId, string role,
        int allocationPercentage, DateTime startDate, DateTime endDate, string assignedBy)
    {
        if (allocationPercentage <= 0 || allocationPercentage > 100)
            throw new ArgumentException("Allocation must be between 1 and 100");
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        Id = Guid.NewGuid();
        ProjectId = projectId;
        EmployeeId = employeeId;
        Role = role;
        AllocationPercentage = allocationPercentage;
        StartDate = startDate;
        EndDate = endDate;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        IsActive = true;
    }

    public void UpdateAllocation(int newPercentage, string updatedBy)
    {
        if (newPercentage <= 0 || newPercentage > 100)
            throw new ArgumentException("Allocation must be between 1 and 100");

        AllocationPercentage = newPercentage;
        AssignedBy = updatedBy;
    }

    public void EndAssignment()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
    }
}