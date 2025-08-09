using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Infrastructure.Data;
using HotChocolate.Authorization;
using HotChocolate.Data;

namespace SkillMatchPro.API.GraphQL;


public class Query
{
    public string Hello() => "Hello from SkillMatch Pro!";

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public async Task<List<Employee>> GetEmployees([Service] ApplicationDbContext context)
    {
        return await context.Employees.ToListAsync();
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = new[] { "Admin" })]
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

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<User>> GetAllUsers([Service] ApplicationDbContext context)
    {
        return await context.Users
            .Include(u => u.Employee)
            .ToListAsync();
    }


[UseProjection]
[UseFiltering]
[UseSorting]
[Authorize]
public IQueryable<Employee> GetFilteredEmployees([Service] ApplicationDbContext context)
{
    return context.Employees
        .Include(e => e.EmployeeSkills)
        .ThenInclude(es => es.Skill);
}

[UseProjection]
[UseFiltering]
[UseSorting]
[Authorize]
public IQueryable<Skill> GetFilteredSkills([Service] ApplicationDbContext context)
{
    return context.Skills;
}

[Authorize]
public async Task<List<Employee>> SearchEmployees(
    string searchTerm,
    [Service] ApplicationDbContext context)
{
    var normalizedSearch = searchTerm.ToLower();

    return await context.Employees
        .Include(e => e.EmployeeSkills)
        .ThenInclude(es => es.Skill)
        .Where(e =>
            e.FirstName.ToLower().Contains(normalizedSearch) ||
            e.LastName.ToLower().Contains(normalizedSearch) ||
            e.Email.ToLower().Contains(normalizedSearch) ||
            e.Department.ToLower().Contains(normalizedSearch) ||
            e.Title.ToLower().Contains(normalizedSearch))
        .ToListAsync();
}
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Employee> GetPaginatedEmployees([Service] ApplicationDbContext context)
    {
        return context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill);
    }

    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Skill> GetPaginatedSkills([Service] ApplicationDbContext context)
    {
        return context.Skills;
    }

    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ManagerOrAbove")]
    public IQueryable<Employee> GetEmployeeDirectory([Service] ApplicationDbContext context)
    {
        return context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill);
    }
}
