namespace SkillMatchPro.Domain.ValueObjects;

public class SeniorityProfile
{
    public string Level { get; }
    public int CodingHoursPerWeek { get; }
    public int MentoringHoursPerWeek { get; }
    public int ReviewHoursPerWeek { get; }
    public int MeetingHoursPerWeek { get; }
    public decimal ProductivityMultiplier { get; }

    private SeniorityProfile(
        string level,
        int codingHours,
        int mentoringHours,
        int reviewHours,
        int meetingHours,
        decimal productivityMultiplier)
    {
        Level = level;
        CodingHoursPerWeek = codingHours;
        MentoringHoursPerWeek = mentoringHours;
        ReviewHoursPerWeek = reviewHours;
        MeetingHoursPerWeek = meetingHours;
        ProductivityMultiplier = productivityMultiplier;
    }

    public static SeniorityProfile Junior => new(
        "Junior",
        codingHours: 30,
        mentoringHours: 0,
        reviewHours: 2,
        meetingHours: 4,
        productivityMultiplier: 0.6m
    );

    public static SeniorityProfile Intermediate => new(
        "Intermediate",
        codingHours: 28,
        mentoringHours: 2,
        reviewHours: 3,
        meetingHours: 4,
        productivityMultiplier: 0.9m
    );

    public static SeniorityProfile Senior => new(
        "Senior",
        codingHours: 20,
        mentoringHours: 6,
        reviewHours: 5,
        meetingHours: 5,
        productivityMultiplier: 1.2m
    );

    public static SeniorityProfile Lead => new(
        "Lead",
        codingHours: 15,
        mentoringHours: 8,
        reviewHours: 6,
        meetingHours: 7,
        productivityMultiplier: 1.5m
    );

    public int GetTotalAllocatedHours() =>
        CodingHoursPerWeek + MentoringHoursPerWeek + ReviewHoursPerWeek + MeetingHoursPerWeek;
}