namespace SkillMatchPro.Domain.Entities;

public class TeamCollaboration
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid Employee1Id { get; private set; }
    public Guid Employee2Id { get; private set; }
    public DateTime CollaborationDate { get; private set; }

    // Collaboration metrics
    public int CommunicationRating { get; private set; } // 1-5
    public int ConflictsResolved { get; private set; }
    public int ConflictsEscalated { get; private set; }
    public bool WouldWorkTogetherAgain { get; private set; }
    public decimal CollaborationScore { get; private set; } // 0-100

    // Feedback
    public string? Employee1Feedback { get; private set; }
    public string? Employee2Feedback { get; private set; }
    public string? ManagerObservations { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;

    private TeamCollaboration() { }

    public TeamCollaboration(
        Guid projectId,
        Guid employee1Id,
        Guid employee2Id)
    {
        // Ensure consistent ordering (smaller GUID first)
        if (employee1Id.CompareTo(employee2Id) > 0)
        {
            (employee1Id, employee2Id) = (employee2Id, employee1Id);
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Employee1Id = employee1Id;
        Employee2Id = employee2Id;
        CollaborationDate = DateTime.UtcNow;
        CommunicationRating = 3; // Default neutral
        WouldWorkTogetherAgain = true; // Default positive
        CalculateCollaborationScore();
    }

    public void SetCollaborationMetrics(
        int communicationRating,
        int conflictsResolved,
        int conflictsEscalated,
        bool wouldWorkTogetherAgain)
    {
        if (communicationRating < 1 || communicationRating > 5)
            throw new ArgumentException("Communication rating must be between 1 and 5");

        CommunicationRating = communicationRating;
        ConflictsResolved = conflictsResolved;
        ConflictsEscalated = conflictsEscalated;
        WouldWorkTogetherAgain = wouldWorkTogetherAgain;

        CalculateCollaborationScore();
    }

    public void SetFeedback(
        string? employee1Feedback,
        string? employee2Feedback,
        string? managerObservations)
    {
        Employee1Feedback = employee1Feedback;
        Employee2Feedback = employee2Feedback;
        ManagerObservations = managerObservations;
    }

    private void CalculateCollaborationScore()
    {
        var score = 0m;

        // Communication (40%)
        score += (CommunicationRating / 5m) * 40;

        // Conflict resolution (30%)
        var totalConflicts = ConflictsResolved + ConflictsEscalated;
        if (totalConflicts > 0)
        {
            var resolutionRate = ConflictsResolved / (decimal)totalConflicts;
            score += resolutionRate * 30;
        }
        else
        {
            score += 30; // No conflicts is good
        }

        // Would work together again (30%)
        score += WouldWorkTogetherAgain ? 30 : 0;

        CollaborationScore = Math.Round(score, 2);
    }
}