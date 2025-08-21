namespace SkillMatchPro.Domain.ValueObjects;

public class HourAllocation
{
    public Guid ProjectId { get; }
    public Guid EmployeeId { get; }
    public int HoursPerWeek { get; }
    public decimal EffortMultiplier { get; }
    public string AllocationNote { get; }

    public HourAllocation(
        Guid projectId,
        Guid employeeId,
        int hoursPerWeek,
        decimal effortMultiplier = 1.0m,
        string allocationNote = "")
    {
        if (hoursPerWeek < 4 || hoursPerWeek > 40)
            throw new ArgumentException("Hours per week must be between 4-40");

        if (effortMultiplier < 0.5m || effortMultiplier > 2.0m)
            throw new ArgumentException("Effort multiplier must be between 0.5-2.0");

        ProjectId = projectId;
        EmployeeId = employeeId;
        HoursPerWeek = hoursPerWeek;
        EffortMultiplier = effortMultiplier;
        AllocationNote = allocationNote;
    }

    // Effective hours considering effort multiplier
    public decimal GetEffectiveHours() => HoursPerWeek * EffortMultiplier;
}