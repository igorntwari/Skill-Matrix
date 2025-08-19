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

        if (employee == null)
        {
            Console.WriteLine($"  CONFLICT: Employee {employeeId} not found!");
            return true;
        }

        Console.WriteLine($"  Checking allocation for {employee.FirstName} {employee.LastName}:");
        Console.WriteLine($"    - Required: {requiredPercentage}%");
        Console.WriteLine($"    - Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        Console.WriteLine($"    - Total assignments: {employee.ProjectAssignments.Count}");
        Console.WriteLine($"    - Active assignments: {employee.ProjectAssignments.Count(pa => pa.IsActive)}");

        // Check each day in the range
        var currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            var dailyAllocation = employee.ProjectAssignments
                .Where(pa => pa.IsActive &&
                    pa.StartDate.Date <= currentDate &&
                    pa.EndDate.Date >= currentDate)
                .Sum(pa => pa.AllocationPercentage);

            if (dailyAllocation > 0)
            {
                Console.WriteLine($"    - {currentDate:yyyy-MM-dd}: {dailyAllocation}% allocated");
            }

            if (dailyAllocation + requiredPercentage > 100)
            {
                Console.WriteLine($"  CONFLICT: {dailyAllocation}% + {requiredPercentage}% > 100%");
                return true;
            }

            currentDate = currentDate.AddDays(1);
        }

        Console.WriteLine($"  NO CONFLICT: Employee is available");
        return false;
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