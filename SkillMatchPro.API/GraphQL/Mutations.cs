using System;
using Microsoft.EntityFrameworkCore;
using SkillMatchPro.API.GraphQL.Inputs;
using SkillMatchPro.API.GraphQL.Types;
using SkillMatchPro.API.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Infrastructure.Data;

namespace SkillMatchPro.API.GraphQL;

public class Mutations
{
    public async Task<Employee> CreateEmployee(
        CreateEmployeeInput input,
        [Service] ApplicationDbContext context)
    {
        // Check for duplicate email
        var emailExists = await context.Employees
            .AnyAsync(e => e.Email.ToLower() == input.Email.ToLower());

        if (emailExists)
        {
            throw new GraphQLException("An employee with this email already exists.");
        }

        // Create new employee using domain constructor
        var employee = new Employee(
            input.FirstName,
            input.LastName,
            input.Email,
            input.Department,
            input.Title
        );

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return employee;
    }

    public async Task<Skill> CreateSkill(
        CreateSkillInput input,
        [Service] ApplicationDbContext context)
    {
        // Check for duplicate skill name
        var skillExists = await context.Skills
            .AnyAsync(s => s.Name.ToLower() == input.Name.ToLower());

        if (skillExists)
        {
            throw new GraphQLException("A skill with this name already exists.");
        }

        // Create new skill
        var skill = new Skill(
            input.Name,
            input.Category,
            input.Description
        );

        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        return skill;
    }

    public async Task<EmployeeSkill> AssignSkillToEmployee(
       AssignSkillInput input,  // Use the input we created
       [Service] ApplicationDbContext context)  // Fixed typo: contex → context
    {
        // 1. GET the employee (not just check if exists)
        var employee = await context.Employees
            .Include(e => e.EmployeeSkills)  // Include skills to check duplicates
            .FirstOrDefaultAsync(e => e.Id == input.EmployeeId);  // Use ID, not email

        if (employee == null)  // If null, employee doesn't exist
        {
            throw new GraphQLException("Employee not found.");
        }

        // 2. GET the skill
        var skill = await context.Skills
            .FirstOrDefaultAsync(s => s.Id == input.SkillId);  // Use ID, not name

        if (skill == null)
        {
            throw new GraphQLException("Skill not found.");
        }

        // 3. Check if employee already has this skill
        var alreadyHasSkill = employee.EmployeeSkills
            .Any(es => es.SkillId == input.SkillId);

        if (alreadyHasSkill)
        {
            throw new GraphQLException("Employee already has this skill.");
        }

        // 4. Add skill using domain method
        employee.AddSkill(skill, input.Proficiency);

        // 5. Save to database
        await context.SaveChangesAsync();

        // 6. Return the newly created relationship
        return employee.EmployeeSkills
            .First(es => es.SkillId == input.SkillId);
    }

    public async Task<AuthPayload> Register(
    RegisterInput input,
    [Service] ApplicationDbContext context,
    [Service] JwtService jwtService)
    {
        try
        {
            // Check if user already exists
            var userExists = await context.Users
                .AnyAsync(u => u.Email.ToLower() == input.Email.ToLower());

            if (userExists)
            {
                throw new GraphQLException("User with this email already exists.");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);

            // Create user
            var user = new User(input.Email, passwordHash);

            // Create employee
            var employee = new Employee(
                input.FirstName,
                input.LastName,
                input.Email,
                input.Department,
                input.Title
            );

            // Link user to employee
            user.LinkToEmployee(employee.Id);

            // Add to database
            context.Users.Add(user);
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            // Generate token
            var token = jwtService.GenerateToken(user);

            return new AuthPayload
            {
                Token = token,
                User = user,
                Employee = employee
            };
        }
        catch (Exception ex)
        {
            // Log the actual error
            Console.WriteLine($"Registration error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new GraphQLException($"Registration failed: {ex.Message}");
        }

    }

    public async Task<AuthPayload> Login(
        LoginInput input,
        [Service] ApplicationDbContext context,
        [Service] JwtService jwtService)
    {
        // Find user
        var user = await context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == input.Email.ToLower());

        if (user == null)
        {
            throw new GraphQLException("Invalid email or password.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        {
            throw new GraphQLException("Invalid email or password.");
        }

        // Generate token
        var token = jwtService.GenerateToken(user);

        return new AuthPayload
        {
            Token = token,
            User = user,
            Employee = user.Employee
        };
    }
}