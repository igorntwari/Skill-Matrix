namespace SkillMatchPro.Domain.ValueObjects;

public class AvailabilitySlot
{
    public DateTime Date { get; set; }
    public int AvailableHours { get; set; }

    public AvailabilitySlot(DateTime date, int availableHours)
    {
        Date = date;
        AvailableHours = availableHours;
    }
}