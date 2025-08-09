namespace SkillMatchPro.Domain.Entities;

public class Skill
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    private Skill() { }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

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