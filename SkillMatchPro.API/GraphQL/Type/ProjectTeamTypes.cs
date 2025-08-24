using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.API.GraphQL.Types;

public class ProjectTeamSummary
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalHoursAllocated { get; set; }
    public List<TeamMemberSummary> TeamMembers { get; set; } = new();
    public Dictionary<string, SkillCoverageInfo> SkillCoverage { get; set; } = new();
}

public class TeamMemberSummary
{
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public List<string> Skills { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SkillCoverageInfo
{
    public int RequiredHours { get; set; }
    public int AllocatedHours { get; set; }
    public bool IsCovered { get; set; }
}

public class TeamMemberAssignmentInput
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
}

public class ProjectTeamResult
{
    public List<ProjectAssignment> SuccessfulAssignments { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public int TotalAssigned { get; set; }
    public int TotalRequested { get; set; }
}

public class ProjectAssignmentDetail
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}