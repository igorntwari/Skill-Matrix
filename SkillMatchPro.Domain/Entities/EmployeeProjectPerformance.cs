namespace SkillMatchPro.Domain.Entities;

public class EmployeeProjectPerformance
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid ProjectId { get; private set; }
    public DateTime EvaluatedAt { get; private set; }

    // Delivery metrics
    public int TasksAssigned { get; private set; }
    public int TasksCompleted { get; private set; }
    public int TasksDeliveredOnTime { get; private set; }

    // Quality metrics
    public int BugsReported { get; private set; }
    public int CodeReviewIssues { get; private set; }
    public decimal QualityScore { get; private set; } // 0-100

    // Efficiency metrics
    public int EstimatedHours { get; private set; }
    public int ActualHours { get; private set; }

    // Manager feedback
    public int ManagerRating { get; private set; } // 1-5
    public string? ManagerComments { get; private set; }

    // Navigation
    public Employee Employee { get; private set; } = null!;
    public Project Project { get; private set; } = null!;

    private EmployeeProjectPerformance() { }

    public EmployeeProjectPerformance(
        Guid employeeId,
        Guid projectId,
        int tasksAssigned,
        int tasksCompleted,
        int tasksDeliveredOnTime,
        int estimatedHours,
        int actualHours)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        ProjectId = projectId;
        TasksAssigned = tasksAssigned;
        TasksCompleted = tasksCompleted;
        TasksDeliveredOnTime = tasksDeliveredOnTime;
        EstimatedHours = estimatedHours;
        ActualHours = actualHours;
        EvaluatedAt = DateTime.UtcNow;

        CalculateQualityScore();
    }

    public void SetQualityMetrics(int bugsReported, int codeReviewIssues)
    {
        BugsReported = bugsReported;
        CodeReviewIssues = codeReviewIssues;
        CalculateQualityScore();
    }

    public void SetManagerFeedback(int rating, string? comments)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        ManagerRating = rating;
        ManagerComments = comments;
    }

    private void CalculateQualityScore()
    {
        // Simple quality calculation
        var bugPenalty = Math.Min(30, BugsReported * 5); // Max 30 point penalty
        var reviewPenalty = Math.Min(20, CodeReviewIssues * 2); // Max 20 point penalty

        QualityScore = Math.Max(0, 100 - bugPenalty - reviewPenalty);
    }

    public decimal GetDeliveryRate()
    {
        if (TasksAssigned == 0) return 100;
        return (decimal)TasksDeliveredOnTime / TasksAssigned * 100;
    }

    public decimal GetCompletionRate()
    {
        if (TasksAssigned == 0) return 100;
        return (decimal)TasksCompleted / TasksAssigned * 100;
    }

    public decimal GetEstimationAccuracy()
    {
        if (EstimatedHours == 0) return 0;
        var variance = Math.Abs(ActualHours - EstimatedHours) / (decimal)EstimatedHours;
        return Math.Max(0, 100 - (variance * 100));
    }
}