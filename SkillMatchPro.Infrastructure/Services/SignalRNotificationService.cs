using Microsoft.AspNetCore.SignalR;
using SkillMatchPro.Application.Services;
using Microsoft.Extensions.Logging;

namespace SkillMatchPro.Infrastructure.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubClients _hubClients;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubClients hubClients,
        ILogger<SignalRNotificationService> logger)
    {
        _hubClients = hubClients;
        _logger = logger;
    }

    public async Task NotifyEmployeeAssignedToProject(
        Guid employeeId,
        Guid projectId,
        string projectName,
        string role,
        int hoursPerWeek)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.EmployeeAssigned,
            Title = "New Project Assignment",
            Message = $"You have been assigned to {projectName} as {role} for {hoursPerWeek} hours/week",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName,
                ["role"] = role,
                ["hoursPerWeek"] = hoursPerWeek
            }
        };

        await _hubClients
            .Group($"user-{employeeId}")
            .SendAsync("ReceiveNotification", notification);

        await _hubClients
            .Group($"project-{projectId}")
            .SendAsync("TeamUpdate", new
            {
                action = "memberAdded",
                projectId,
                employeeId,
                role,
                hoursPerWeek
            });

        _logger.LogInformation($"Notified employee {employeeId} about assignment to {projectName}");
    }

    public async Task NotifyEmployeeRemovedFromProject(
        Guid employeeId,
        Guid projectId,
        string projectName)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.EmployeeRemoved,
            Title = "Removed from Project",
            Message = $"You have been removed from {projectName}",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName
            }
        };

        await _hubClients
            .Group($"user-{employeeId}")
            .SendAsync("ReceiveNotification", notification);

        await _hubClients
            .Group($"project-{projectId}")
            .SendAsync("TeamUpdate", new
            {
                action = "memberRemoved",
                projectId,
                employeeId
            });
    }

    public async Task NotifyEmployeeHoursUpdated(
        Guid employeeId,
        Guid projectId,
        string projectName,
        int oldHours,
        int newHours)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.HoursUpdated,
            Title = "Work Hours Updated",
            Message = $"Your hours on {projectName} changed from {oldHours} to {newHours} hours/week",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName,
                ["oldHours"] = oldHours,
                ["newHours"] = newHours
            }
        };

        await _hubClients
            .Group($"user-{employeeId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyNewSkillAdded(
        Guid employeeId,
        string employeeName,
        string skillName,
        string proficiencyLevel)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.SkillAdded,
            Title = "New Skill Added",
            Message = $"{employeeName} added {skillName} skill at {proficiencyLevel} level",
            Data = new Dictionary<string, object>
            {
                ["employeeId"] = employeeId,
                ["employeeName"] = employeeName,
                ["skillName"] = skillName,
                ["proficiencyLevel"] = proficiencyLevel
            }
        };

        await _hubClients
            .Group("management")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyProjectUpdated(
        Guid projectId,
        string projectName,
        string whatChanged,
        string changedBy)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.ProjectUpdated,
            Title = "Project Updated",
            Message = $"{projectName}: {whatChanged} updated by {changedBy}",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName,
                ["whatChanged"] = whatChanged,
                ["changedBy"] = changedBy
            }
        };

        await _hubClients
            .Group($"project-{projectId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyProjectStatusChanged(
        Guid projectId,
        string projectName,
        string oldStatus,
        string newStatus)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.StatusChanged,
            Title = "Project Status Changed",
            Message = $"{projectName} status: {oldStatus} → {newStatus}",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName,
                ["oldStatus"] = oldStatus,
                ["newStatus"] = newStatus
            }
        };

        await _hubClients
            .Groups($"project-{projectId}", "management")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyOptimizationComplete(
        Guid projectId,
        string projectName,
        int optionsGenerated)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.OptimizationComplete,
            Title = "Team Optimization Complete",
            Message = $"Generated {optionsGenerated} team options for {projectName}",
            Data = new Dictionary<string, object>
            {
                ["projectId"] = projectId,
                ["projectName"] = projectName,
                ["optionsCount"] = optionsGenerated
            }
        };

        await _hubClients
            .Group("management")
            .SendAsync("ReceiveNotification", notification);
    }
}