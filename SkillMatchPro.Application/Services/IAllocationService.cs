using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Application.Services;

public interface IAllocationService
{
    Task<bool> CheckAllocationConflict(Guid employeeId, int requiredPercentage,
        DateTime startDate, DateTime endDate);
    Task<List<Employee>> FindAvailableEmployees(Guid skillId,
        ProficiencyLevel minProficiency, int requiredPercentage,
        DateTime startDate, DateTime endDate);
}