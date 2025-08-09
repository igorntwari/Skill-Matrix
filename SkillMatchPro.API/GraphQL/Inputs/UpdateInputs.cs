namespace SkillMatchPro.API.GraphQL.Inputs;

public class UpdateEmployeeInput
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public string? Title { get; set; }
}

public class UpdateSkillInput
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
}