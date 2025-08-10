using Microsoft.EntityFrameworkCore;
using SkillMatchPro.API.GraphQL.Inputs;
using SkillMatchPro.API.GraphQL.Types;
using SkillMatchPro.API.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Infrastructure.Data;
using HotChocolate.Authorization;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using SkillMatchPro.Application.Services;

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

    [Authorize(Policy = "AdminOnly")]
    public async Task<User> AssignRole(
    Guid userId,
    UserRole newRole,
    [Service] ApplicationDbContext context)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new GraphQLException("User not found.");
        }

        // For now, we'll need to add a method to User entity to update role
        // Or use reflection/EF Core to update
        context.Entry(user).Property(u => u.Role).CurrentValue = newRole;

        await context.SaveChangesAsync();

        return user;
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<List<Employee>> GetAllEmployees(
        [Service] ApplicationDbContext context)
    {
        return await context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .ToListAsync();
    }
    //// TEMPORARY - Remove in production!
    //public async Task<AuthPayload> CreateAdminUser(
    //    [Service] ApplicationDbContext context,
    //    [Service] JwtService jwtService)
    //{
    //    // Check if admin already exists
    //    var adminExists = await context.Users
    //        .AnyAsync(u => u.Email == "admin@skillmatchpro.com");

    //    if (adminExists)
    //    {
    //        throw new GraphQLException("Admin user already exists.");
    //    }

    //    // Create admin user
    //    var passwordHash = BCrypt.Net.BCrypt.HashPassword("AdminPass123!");
    //    var adminUser = new User("admin@skillmatchpro.com", passwordHash, UserRole.Admin);

    //    // Create admin employee
    //    var adminEmployee = new Employee(
    //        "System",
    //        "Administrator",
    //        "admin@skillmatchpro.com",
    //        "IT",
    //        "System Administrator"
    //    );

    //    adminUser.LinkToEmployee(adminEmployee.Id);

    //    context.Users.Add(adminUser);
    //    context.Employees.Add(adminEmployee);
    //    await context.SaveChangesAsync();

    //    var token = jwtService.GenerateToken(adminUser);

    //    return new AuthPayload
    //    {
    //        Token = token,
    //        User = adminUser,
    //        Employee = adminEmployee
    //    };
    //}

    // Add this field at the top of Mutations class
    private readonly ILogger<Mutations> _logger;

    // Add constructor
    public Mutations(ILogger<Mutations> logger)
    {
        _logger = logger;
    }

    // Update AssignRole to add logging
    [Authorize(Policy = "AdminOnly")]
    public async Task<User> AssignRole(
        Guid userId,
        UserRole newRole,
        [Service] ApplicationDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        var adminEmail = currentUser?.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogInformation("Admin {AdminEmail} attempting to assign role {NewRole} to user {UserId}",
            adminEmail, newRole, userId);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("Attempted to assign role to non-existent user {UserId}", userId);
            throw new GraphQLException("User not found.");
        }

        var oldRole = user.Role;
        context.Entry(user).Property(u => u.Role).CurrentValue = newRole;
        await context.SaveChangesAsync();

        _logger.LogInformation("Successfully changed user {UserId} role from {OldRole} to {NewRole}",
            userId, oldRole, newRole);

        return user;
    }

    [Authorize]
    public async Task<Employee> UpdateMyProfile(
    string? firstName,
    string? lastName,
    string? title,
    [Service] ApplicationDbContext context,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userEmail))
        {
            throw new UnauthorizedAccessException("User email not found in token");
        }

        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Email.ToLower() == userEmail.ToLower());

        if (employee == null)
        {
            throw new GraphQLException("Employee profile not found");
        }

        if (!string.IsNullOrEmpty(firstName))
            context.Entry(employee).Property(e => e.FirstName).CurrentValue = firstName;

        if (!string.IsNullOrEmpty(lastName))
            context.Entry(employee).Property(e => e.LastName).CurrentValue = lastName;

        if (!string.IsNullOrEmpty(title))
            context.Entry(employee).Property(e => e.Title).CurrentValue = title;

        await context.SaveChangesAsync();

        _logger.LogInformation("User {Email} updated their profile", userEmail);

        return employee;
    }

    [Authorize]
    public async Task<Employee> UpdateEmployee(
    UpdateEmployeeInput input,
    [Service] ApplicationDbContext context,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUserEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;
        var currentUserRole = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Role)?.Value;

        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == input.Id);

        if (employee == null)
        {
            throw new GraphQLException("Employee not found.");
        }

        // Check authorization: Users can edit themselves, Managers/Admins can edit anyone
        if (currentUserRole == "Employee" && employee.Email.ToLower() != currentUserEmail?.ToLower())
        {
            throw new UnauthorizedAccessException("You can only edit your own profile.");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(input.FirstName))
            context.Entry(employee).Property(e => e.FirstName).CurrentValue = input.FirstName;

        if (!string.IsNullOrWhiteSpace(input.LastName))
            context.Entry(employee).Property(e => e.LastName).CurrentValue = input.LastName;

        if (!string.IsNullOrWhiteSpace(input.Department) && currentUserRole != "Employee")
            context.Entry(employee).Property(e => e.Department).CurrentValue = input.Department;

        if (!string.IsNullOrWhiteSpace(input.Title) && currentUserRole != "Employee")
            context.Entry(employee).Property(e => e.Title).CurrentValue = input.Title;

        await context.SaveChangesAsync();

        _logger.LogInformation("Employee {EmployeeId} updated by {User}", input.Id, currentUserEmail);

        return employee;
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<bool> DeleteEmployee(
        Guid id,
        [Service] ApplicationDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUserEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            throw new GraphQLException("Employee not found.");
        }

        employee.SoftDelete(currentUserEmail);
        await context.SaveChangesAsync();

        _logger.LogWarning("Employee {EmployeeId} soft deleted by {User}", id, currentUserEmail);

        return true;
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<bool> DeleteSkill(
        Guid id,
        [Service] ApplicationDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUserEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        var skill = await context.Skills
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill == null)
        {
            throw new GraphQLException("Skill not found.");
        }

        skill.SoftDelete(currentUserEmail);
        await context.SaveChangesAsync();

        _logger.LogWarning("Skill {SkillId} soft deleted by {User}", id, currentUserEmail);

        return true;
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<Project> CreateProject(
     string name,
     string description,
     string department,
     string startDate,
     string endDate,
     int priority,
     [Service] ApplicationDbContext context,
     [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (!DateTime.TryParse(startDate, out var parsedStartDate))
            throw new ArgumentException("Invalid start date format");

        if (!DateTime.TryParse(endDate, out var parsedEndDate))
            throw new ArgumentException("Invalid end date format");

        // Ensure UTC kind so PostgreSQL accepts it
        parsedStartDate = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
        parsedEndDate = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);

        var createdBy = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        var project = new Project(
            name,
            description,
            department,
            parsedStartDate,
            parsedEndDate,
            createdBy,
            priority
        );

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        _logger.LogInformation("Project {ProjectName} created by {User}", name, createdBy);

        return project;
    }


    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<ProjectRequirement> AddProjectRequirement(
     Guid projectId,
     Guid skillId,
     ProficiencyLevel minimumProficiency,
     int requiredCount,
     [Service] ApplicationDbContext context)
    {
        var project = await context.Projects
            .Include(p => p.Requirements)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new GraphQLException("Project not found");

        // Check if skill exists
        var skillExists = await context.Skills.AnyAsync(s => s.Id == skillId);
        if (!skillExists)
            throw new GraphQLException("Skill not found");

        // Create the requirement directly instead of using the method
        var requirement = new ProjectRequirement(projectId, skillId, minimumProficiency, requiredCount);

        context.ProjectRequirements.Add(requirement);
        await context.SaveChangesAsync();

        // Load the requirement with navigation properties
        return await context.ProjectRequirements
            .Include(pr => pr.Skill)
            .FirstAsync(pr => pr.Id == requirement.Id);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<ProjectAssignment> AssignEmployeeToProject(
    Guid projectId,
    Guid employeeId,
    string role,
    int allocationPercentage,
    string startDate,
    string endDate,
    [Service] ApplicationDbContext context,
    [Service] IAllocationService allocationService,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var assignedBy = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        // Parse dates and convert to UTC
        if (!DateTime.TryParse(startDate, out var parsedStartDate))
            throw new ArgumentException("Invalid start date format");
        if (!DateTime.TryParse(endDate, out var parsedEndDate))
            throw new ArgumentException("Invalid end date format");

        // Convert to UTC
        parsedStartDate = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
        parsedEndDate = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);

        // Check for conflicts
        var hasConflict = await allocationService.CheckAllocationConflict(
            employeeId, allocationPercentage, parsedStartDate, parsedEndDate);

        if (hasConflict)
            throw new GraphQLException("Employee has allocation conflicts in the specified period");

        var assignment = new ProjectAssignment(projectId, employeeId, role,
            allocationPercentage, parsedStartDate, parsedEndDate, assignedBy);

        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync();

        _logger.LogInformation("Employee {EmployeeId} assigned to project {ProjectId} by {User}",
            employeeId, projectId, assignedBy);

        return assignment;
    }
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<List<Employee>> GetAvailableEmployeesForSkill(
    Guid skillId,
    ProficiencyLevel minProficiency,
    int allocationPercentage,
    string startDate,  // Changed to string
    string endDate,    // Changed to string
    [Service] IAllocationService allocationService)
    {
        // Parse dates
        if (!DateTime.TryParse(startDate, out var parsedStartDate))
            throw new ArgumentException("Invalid start date format");
        if (!DateTime.TryParse(endDate, out var parsedEndDate))
            throw new ArgumentException("Invalid end date format");

        return await allocationService.FindAvailableEmployees(
            skillId, minProficiency, allocationPercentage, parsedStartDate, parsedEndDate);
    }
}