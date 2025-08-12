using SkillMatchPro.Domain.ValueObjects;

namespace SkillMatchPro.Domain.Entities;

public class TeamComposition
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public DateTime ComposedAt { get; private set; }
    public string ComposedBy { get; private set; }
    public decimal OverallScore { get; private set; }
    public bool IsOptimal { get; private set; }

    // Team members and their assignments
    private readonly List<TeamMember> _teamMembers = new();
    public IReadOnlyCollection<TeamMember> TeamMembers => _teamMembers.AsReadOnly();

    // Skill coverage tracking
    private readonly List<SkillMatch> _skillMatches = new();
    public IReadOnlyCollection<SkillMatch> SkillMatches => _skillMatches.AsReadOnly();

    private TeamComposition()
    {
        ComposedBy = string.Empty;
    }

    public TeamComposition(Guid projectId, string composedBy)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        ComposedAt = DateTime.UtcNow;
        ComposedBy = composedBy;
        OverallScore = 0;
        IsOptimal = false;
    }

    public void AddTeamMember(Employee employee, Skill skill, int allocationPercentage, MatchScore score)
    {
        // Check if employee already in team
        if (_teamMembers.Any(tm => tm.EmployeeId == employee.Id))
        {
            var existingMember = _teamMembers.First(tm => tm.EmployeeId == employee.Id);
            existingMember.AddSkillAssignment(skill, allocationPercentage);
        }
        else
        {
            var teamMember = new TeamMember(employee.Id, employee.FirstName, employee.LastName);
            teamMember.AddSkillAssignment(skill, allocationPercentage);
            _teamMembers.Add(teamMember);
        }

        RecalculateOverallScore();
    }

    public void AddSkillMatch(SkillMatch skillMatch)
    {
        _skillMatches.Add(skillMatch);
    }

    private void RecalculateOverallScore()
    {
        if (!_skillMatches.Any()) return;

        // Average of all skill match scores
        var totalScore = _skillMatches
            .SelectMany(sm => sm.GetTopCandidates())
            .Average(c => c.Score.TotalScore);

        OverallScore = Math.Round(totalScore, 2);
    }

    public void MarkAsOptimal()
    {
        IsOptimal = true;
    }

    public bool AreAllRequirementsMet()
    {
        return _skillMatches.All(sm => sm.IsFulfilled());
    }
}

// Nested class for team members
public class TeamMember
{
    public Guid EmployeeId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public int TotalAllocation { get; private set; }

    private readonly List<SkillAssignment> _skillAssignments = new();
    public IReadOnlyCollection<SkillAssignment> SkillAssignments => _skillAssignments.AsReadOnly();

    public TeamMember(Guid employeeId, string firstName, string lastName)
    {
        EmployeeId = employeeId;
        FirstName = firstName;
        LastName = lastName;
        TotalAllocation = 0;
    }

    public void AddSkillAssignment(Skill skill, int allocationPercentage)
    {
        _skillAssignments.Add(new SkillAssignment(skill.Id, skill.Name, allocationPercentage));
        TotalAllocation += allocationPercentage;

        if (TotalAllocation > 100)
            throw new InvalidOperationException($"Total allocation for {FirstName} {LastName} exceeds 100%");
    }
}

public class SkillAssignment
{
    public Guid SkillId { get; }
    public string SkillName { get; }
    public int AllocationPercentage { get; }

    public SkillAssignment(Guid skillId, string skillName, int allocationPercentage)
    {
        SkillId = skillId;
        SkillName = skillName;
        AllocationPercentage = allocationPercentage;
    }
}