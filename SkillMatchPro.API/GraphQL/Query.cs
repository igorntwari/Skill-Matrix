using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Infrastructure.Data;

namespace SkillMatchPro.API.GraphQL;

public class Query
{
    public string Hello() => "Hello from SkillMatch Pro!";

    public async Task<List<Employee>> GetEmployees([Service] ApplicationDbContext context)
    {
        return await context.Employees.ToListAsync();
    }
    public async Task<List<Skill>> GetSkills([Service] ApplicationDbContext context)
    {
        return await context.Skills.ToListAsync();
    }
    public async Task<List<Employee>> GetEmployeesWithSkills([Service] ApplicationDbContext context)
    {
        return await context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .ToListAsync();
    }
}
