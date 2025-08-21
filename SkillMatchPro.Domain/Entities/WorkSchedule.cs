namespace SkillMatchPro.Domain.Entities;

public class WorkSchedule
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public int StandardHoursPerWeek { get; private set; }
    public int BufferHoursPerWeek { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    // Navigation
    public Employee Employee { get; private set; } = null!;

    private WorkSchedule() { }

    public WorkSchedule(Guid employeeId, int standardHoursPerWeek, int bufferHoursPerWeek)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        StandardHoursPerWeek = standardHoursPerWeek;
        BufferHoursPerWeek = bufferHoursPerWeek;
        EffectiveFrom = DateTime.UtcNow;

        Validate();
    }

    private void Validate()
    {
        if (StandardHoursPerWeek < 20 || StandardHoursPerWeek > 40)
            throw new ArgumentException("Standard hours must be between 20-40 per week");

        if (BufferHoursPerWeek < 0 || BufferHoursPerWeek > 8)
            throw new ArgumentException("Buffer hours must be between 0-8 per week");

        if (StandardHoursPerWeek - BufferHoursPerWeek < 20)
            throw new ArgumentException("Available hours after buffer must be at least 20");
    }

    public int GetAvailableHours() => StandardHoursPerWeek - BufferHoursPerWeek;
}