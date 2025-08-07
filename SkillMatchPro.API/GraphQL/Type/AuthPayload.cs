using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.API.GraphQL.Types;

public class AuthPayload
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public Employee? Employee { get; set; }
}