using SkillMatchPro.Application.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.ValueObjects;

namespace SkillMatchPro.API.GraphQL.Types;

public class OptimizationConstraintsInput
{
    public decimal? MaxBudgetPerWeek { get; set; }
    public int? MaxTeamSize { get; set; }
    public int? MinSeniorMembers { get; set; }
    public bool RequireBackupCoverage { get; set; } = true;
    public int MinBufferHoursPerPerson { get; set; } = 4;
    public List<Guid> RequiredEmployees { get; set; } = new();
    public List<Guid> ExcludedEmployees { get; set; } = new();
}

public class TeamOptionType
{
    public Guid Id { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public decimal TotalCostPerWeek { get; set; }
    public decimal QualityScore { get; set; }
    public decimal RiskScore { get; set; }
    public bool MeetsAllRequirements { get; set; }
    public int TotalHoursPerWeek { get; set; }
    public decimal EffectiveHoursPerWeek { get; set; }
    public List<HourAllocationDetail> Allocations { get; set; } = new();
    public Dictionary<string, string> TradeOffs { get; set; } = new();
}

public class HourAllocationDetail
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public decimal EffortMultiplier { get; set; }
    public decimal EffectiveHours { get; set; }
    public string AllocationNote { get; set; } = string.Empty;
    public decimal WeeklyCost { get; set; }
}

public class TeamOptimizationResultType
{
    public List<TeamOptionType> Options { get; set; } = new();
    public TeamAllocationSummaryType Summary { get; set; } = null!;
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, string> Recommendations { get; set; } = new();
}

public class TeamAllocationSummaryType
{
    public int TotalHoursRequired { get; set; }
    public int TotalHoursAllocated { get; set; }
    public decimal AverageUtilization { get; set; }
    public Dictionary<string, int> HoursBySkill { get; set; } = new();
    public Dictionary<string, int> HoursBySeniority { get; set; } = new();
}

public class HourlyAvailabilityType
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeTitle { get; set; } = string.Empty;
    public int TotalHoursPerWeek { get; set; }
    public int AllocatedHours { get; set; }
    public int AvailableHours { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public List<ProjectHoursType> CurrentProjects { get; set; } = new();
}

public class ProjectHoursType
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}