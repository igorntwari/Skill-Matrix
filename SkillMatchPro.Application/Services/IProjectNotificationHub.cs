namespace SkillMatchPro.Application.Services;

public interface IProjectNotificationHub
{
    Task ReceiveNotification(object notification);
    Task TeamUpdate(object update);
}