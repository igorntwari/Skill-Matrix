using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Application.Services;

public interface ITeamOptimizationService
{
    Task<TeamOptimizationResult> OptimizeTeam(
        Guid projectId,
        OptimizationConstraints constraints);

    Task<List<HourlyAvailability>> GetTeamAvailability(
        List<Guid> employeeIds,
        DateTime startDate,
        DateTime endDate);
}

public class OptimizationConstraints
{
    public decimal? MaxBudgetPerWeek { get; set; }
    public int? MaxTeamSize { get; set; }
    public int? MinSeniorMembers { get; set; }
    public bool RequireBackupCoverage { get; set; } = true;
    public int MinBufferHoursPerPerson { get; set; } = 4;
    public List<Guid> RequiredEmployees { get; set; } = new();
    public List<Guid> ExcludedEmployees { get; set; } = new();
}

public class TeamOptimizationResult
{
    public List<TeamOption> Options { get; set; } = new();
    public TeamAllocationSummary Summary { get; set; } = null!;
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, string> Recommendations { get; set; } = new();
}

public class TeamAllocationSummary
{
    public int TotalHoursRequired { get; set; }
    public int TotalHoursAllocated { get; set; }
    public decimal AverageUtilization { get; set; }
    public Dictionary<string, int> HoursBySkill { get; set; } = new();
    public Dictionary<string, int> HoursBySeniority { get; set; } = new();
}

public class HourlyAvailability
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalHoursPerWeek { get; set; }
    public int AllocatedHours { get; set; }
    public int AvailableHours { get; set; }
    public List<ProjectHours> CurrentProjects { get; set; } = new();
}

public class ProjectHours
{
    public string ProjectName { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}