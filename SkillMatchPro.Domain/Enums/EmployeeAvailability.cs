namespace SkillMatchPro.Domain.Entities;

public class EmployeeAvailability
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime Date { get; private set; }
    public int AvailableHours { get; private set; } // 0-8 hours per day
    public string? Notes { get; private set; }

    // Navigation
    public Employee Employee { get; private set; } = null!;

    private EmployeeAvailability() { }

    public EmployeeAvailability(Guid employeeId, DateTime date, int availableHours, string? notes = null)
    {
        if (availableHours < 0 || availableHours > 8)
            throw new ArgumentException("Available hours must be between 0 and 8");

        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Date = date.Date; // Ensure we only store date, not time
        AvailableHours = availableHours;
        Notes = notes;
    }

    public void UpdateAvailability(int newHours, string? notes = null)
    {
        if (newHours < 0 || newHours > 8)
            throw new ArgumentException("Available hours must be between 0 and 8");

        AvailableHours = newHours;
        Notes = notes;
    }
}