using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Application.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, string title, string message,
        NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}