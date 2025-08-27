namespace SkillMatchPro.Application.Services;

public interface INotificationService
{
    // Project Assignment Notifications
    Task NotifyEmployeeAssignedToProject(Guid employeeId, Guid projectId, string projectName, string role, int hoursPerWeek);
    Task NotifyEmployeeRemovedFromProject(Guid employeeId, Guid projectId, string projectName);
    Task NotifyEmployeeHoursUpdated(Guid employeeId, Guid projectId, string projectName, int oldHours, int newHours);

    // Skill Notifications
    Task NotifyNewSkillAdded(Guid employeeId, string employeeName, string skillName, string proficiencyLevel);

    // Project Update Notifications
    Task NotifyProjectUpdated(Guid projectId, string projectName, string whatChanged, string changedBy);
    Task NotifyProjectStatusChanged(Guid projectId, string projectName, string oldStatus, string newStatus);

    // Optimization Notifications
    Task NotifyOptimizationComplete(Guid projectId, string projectName, int optionsGenerated);
}

public class NotificationType
{
    public const string EmployeeAssigned = "EmployeeAssigned";
    public const string EmployeeRemoved = "EmployeeRemoved";
    public const string HoursUpdated = "HoursUpdated";
    public const string SkillAdded = "SkillAdded";
    public const string ProjectUpdated = "ProjectUpdated";
    public const string StatusChanged = "StatusChanged";
    public const string OptimizationComplete = "OptimizationComplete";
}

public class NotificationMessage
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
}