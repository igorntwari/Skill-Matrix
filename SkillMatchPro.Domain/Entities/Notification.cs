namespace SkillMatchPro.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private Notification()
    {
        Title = string.Empty;
        Message = string.Empty;
    }

    public Notification(Guid userId, string title, string message,
        NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}

public enum NotificationType
{
    Info = 1,
    Success = 2,
    Warning = 3,
    Error = 4,
    SkillAssigned = 5,
    ProjectAssigned = 6,
    RoleChanged = 7
}