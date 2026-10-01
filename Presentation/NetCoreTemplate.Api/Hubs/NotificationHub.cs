using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace NetCoreTemplate.Api.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }

    public async Task ReceiveNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
