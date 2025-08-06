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
}
