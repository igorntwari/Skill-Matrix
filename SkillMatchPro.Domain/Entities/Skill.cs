namespace SkillMatchPro.Domain.Entities;

public class Skill
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }

    private Skill() { }

    public Skill(string name, string category, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Skill name is required");
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category is required");

        Id = Guid.NewGuid();
        Name = name;
        Category = category;
        Description = description;
        IsActive = true;
    }
}