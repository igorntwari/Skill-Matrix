using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Infrastructure.Data;

namespace SkillMatchPro.Infrastructure.Services;

public class AllocationService : IAllocationService
{
    private readonly ApplicationDbContext _context;

    public AllocationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckAllocationConflict(Guid employeeId, int requiredPercentage,
        DateTime startDate, DateTime endDate)
    {
        var employee = await _context.Employees
            .Include(e => e.ProjectAssignments)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null) return true; // Conflict - employee doesn't exist

        // Check each day in the range
        var currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            var dailyAllocation = employee.ProjectAssignments
                .Where(pa => pa.IsActive &&
                    pa.StartDate <= currentDate &&
                    pa.EndDate >= currentDate)
                .Sum(pa => pa.AllocationPercentage);

            if (dailyAllocation + requiredPercentage > 100)
                return true; // Conflict found

            currentDate = currentDate.AddDays(1);
        }

        return false; // No conflict
    }

    public async Task<List<Employee>> FindAvailableEmployees(Guid skillId,
        ProficiencyLevel minProficiency, int requiredPercentage,
        DateTime startDate, DateTime endDate)
    {
        var employeesWithSkill = await _context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .Include(e => e.ProjectAssignments)
            .Where(e => e.EmployeeSkills.Any(es =>
                es.SkillId == skillId &&
                es.Proficiency >= minProficiency))
            .ToListAsync();

        var availableEmployees = new List<Employee>();

        foreach (var employee in employeesWithSkill)
        {
            var hasConflict = await CheckAllocationConflict(
                employee.Id, requiredPercentage, startDate, endDate);

            if (!hasConflict)
                availableEmployees.Add(employee);
        }

        return availableEmployees;
    }
}