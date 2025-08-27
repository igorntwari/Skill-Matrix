using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SkillMatchPro.API.Hubs;

[Authorize]
public class ProjectNotificationHub : Hub
{
    private readonly ILogger<ProjectNotificationHub> _logger;

    public ProjectNotificationHub(ILogger<ProjectNotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // If manager or admin, add to management group
            if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Manager") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "management");
            }

            _logger.LogInformation($"User {userEmail} connected with ID: {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation($"User {userEmail} disconnected");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinProjectGroup(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        _logger.LogInformation($"User joined project group: {projectId}");
    }

    public async Task LeaveProjectGroup(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }
}