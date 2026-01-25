using Microsoft.AspNetCore.SignalR;
using Vertex.API.Hubs;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;

namespace Vertex.API.Services;

/// <summary>
/// Implementación del servicio de notificaciones usando SignalR.
/// Se encuentra en la capa API porque depende de NotificationHub.
/// Usa Clients.User() para filtrado seguro por usuario autenticado.
/// IMPORTANTE: Persiste notificaciones en BD antes de enviarlas por SignalR.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationRepository _notificationRepository;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        INotificationRepository notificationRepository)
    {
        _hubContext = hubContext;
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Envía una notificación personalizada a un usuario específico usando Clients.User()
    /// IMPORTANTE: Filtra por UserId del JWT - solo el usuario destinatario la recibe
    /// Persiste en BD antes de enviar por SignalR
    /// </summary>
    public async Task SendNotificationAsync(string userId, string title, string message, string type = "info", string? data = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Read = false,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        // 1. Persistir en base de datos
        await _notificationRepository.AddAsync(notification);

        // 2. Enviar por SignalR al usuario conectado (si está online)
        await _hubContext.Clients.User(userId).SendAsync("Notification", new
        {
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.Read,
            notification.Timestamp,
            notification.Data
        });
    }

    /// <summary>
    /// Notifica a un usuario específico sobre el progreso de su onboarding
    /// Usa Clients.User() para garantizar que solo el usuario correcto la recibe.
    /// Si ya existe una notificación de progreso, la actualiza en lugar de crear una nueva (evita spam).
    /// </summary>
    public async Task NotifyOnboardingProgressAsync(string userId, string message, int currentStep)
    {
        const string progressTitle = "Progreso del Onboarding";

        // 0. Buscar si ya existe una notificación de progreso para este usuario
        var existing = await _notificationRepository.GetLatestProgressNotificationAsync(userId);

        Notification notification;

        if (existing is not null)
        {
            // Actualizar la existente
            existing.Message = message;
            existing.Type = "info";
            existing.Read = false;
            existing.Timestamp = DateTime.UtcNow;
            existing.Data = $"{{\"currentStep\":{currentStep}}}";

            await _notificationRepository.UpdateAsync(existing);
            notification = existing;
        }
        else
        {
            // Crear una nueva si no existe
            notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = progressTitle,
                Message = message,
                Type = "info",
                Read = false,
                Timestamp = DateTime.UtcNow,
                Data = $"{{\"currentStep\":{currentStep}}}"
            };

            await _notificationRepository.AddAsync(notification);
        }

        // 2. Enviar por SignalR
        await _hubContext.Clients.User(userId).SendAsync("OnboardingProgress", new
        {
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Message,
            notification.Type,
            CurrentStep = currentStep,
            notification.Read,
            notification.Timestamp
        });
    }

    /// <summary>
    /// Notifica a un usuario que su onboarding se completó exitosamente
    /// Usa Clients.User() para garantizar privacidad
    /// </summary>
    public async Task NotifyOnboardingCompletedAsync(string userId, string profileId)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "¡Onboarding Completado!",
            Message = "¡Tu perfil profesional ha sido creado exitosamente!",
            Type = "success",
            Read = false,
            Timestamp = DateTime.UtcNow,
            Data = $"{{\"profileId\":\"{profileId}\"}}"
        };

        // 1. Persistir en base de datos
        await _notificationRepository.AddAsync(notification);

        // 2. Enviar por SignalR
        await _hubContext.Clients.User(userId).SendAsync("OnboardingCompleted", new
        {
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Message,
            notification.Type,
            ProfileId = profileId,
            notification.Read,
            notification.Timestamp
        });
    }

    /// <summary>
    /// Notifica a todos los usuarios conectados (broadcast)
    /// NO persiste en BD porque no está dirigida a un usuario específico
    /// </summary>
    public async Task NotifyAllAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("Notification", new
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Notificación del Sistema",
            Message = message,
            Type = "info",
            Read = false,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifica a un grupo específico de usuarios
    /// NO persiste en BD porque es notificación de grupo
    /// </summary>
    public async Task NotifyGroupAsync(string groupName, string message)
    {
        await _hubContext.Clients.Group(groupName).SendAsync("GroupNotification", new
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"Notificación del Grupo {groupName}",
            Message = message,
            Type = "info",
            GroupName = groupName,
            Read = false,
            Timestamp = DateTime.UtcNow
        });
    }
}
