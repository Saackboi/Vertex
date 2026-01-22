using Microsoft.AspNetCore.SignalR;
using Vertex.API.Hubs;
using Vertex.Application.Interfaces;

namespace Vertex.API.Services;

/// <summary>
/// Implementación del servicio de notificaciones usando SignalR.
/// Se encuentra en la capa API porque depende de NotificationHub.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Notifica a un usuario específico sobre el progreso de su onboarding
    /// </summary>
    public async Task NotifyOnboardingProgressAsync(string userId, string message, int currentStep)
    {
        await _hubContext.Clients.Group(userId).SendAsync("OnboardingProgress", new
        {
            Message = message,
            CurrentStep = currentStep,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifica a un usuario que su onboarding se completó exitosamente
    /// </summary>
    public async Task NotifyOnboardingCompletedAsync(string userId, string profileId)
    {
        await _hubContext.Clients.Group(userId).SendAsync("OnboardingCompleted", new
        {
            Message = "¡Tu perfil profesional ha sido creado exitosamente!",
            ProfileId = profileId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifica a todos los usuarios conectados (broadcast)
    /// </summary>
    public async Task NotifyAllAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("Notification", new
        {
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifica a un grupo específico de usuarios
    /// </summary>
    public async Task NotifyGroupAsync(string groupName, string message)
    {
        await _hubContext.Clients.Group(groupName).SendAsync("GroupNotification", new
        {
            Message = message,
            GroupName = groupName,
            Timestamp = DateTime.UtcNow
        });
    }
}
