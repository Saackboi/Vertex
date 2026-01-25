using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Vertex.Application.Interfaces;

namespace Vertex.API.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real.
/// Los clientes se conectan a este hub para recibir actualizaciones.
/// </summary>
[Authorize] // Requiere autenticación JWT
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationRepository _notificationRepository;

    public NotificationHub(
        ILogger<NotificationHub> logger,
        INotificationRepository notificationRepository)
    {
        _logger = logger;
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se conecta al hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar usuario a un grupo con su userId para enviar mensajes personalizados
            await Groups.AddToGroupAsync(connectionId, userId);
            _logger.LogInformation("Usuario {UserId} conectado a SignalR con connectionId {ConnectionId}", userId, connectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se desconecta
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, userId);
            _logger.LogInformation("Usuario {UserId} desconectado de SignalR", userId);
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Error al desconectar usuario {UserId}", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Método invocable desde el cliente para unirse a un grupo específico
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Usuario {ConnectionId} se unió al grupo {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Método invocable desde el cliente para salir de un grupo
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Usuario {ConnectionId} salió del grupo {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Método de ejemplo: El cliente puede enviar un ping y recibir un pong
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    /// <summary>
    /// Marca una notificación específica como leída
    /// Invocable desde el cliente: await connection.invoke('MarkAsRead', notificationId)
    /// </summary>
    /// <param name="notificationId">ID de la notificación a marcar como leída</param>
    public async Task MarkAsRead(string notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Intento de marcar notificación como leída sin userId válido");
            return;
        }

        if (!Guid.TryParse(notificationId, out var guid))
        {
            _logger.LogWarning("notificationId inválido: {NotificationId}", notificationId);
            return;
        }

        // Actualizar en base de datos
        var success = await _notificationRepository.MarkAsReadAsync(guid, userId);

        if (success)
        {
            _logger.LogInformation("Usuario {UserId} marcó notificación {NotificationId} como leída", userId, notificationId);
            
            // Notificar al cliente que la notificación fue marcada como leída
            await Clients.User(userId).SendAsync("NotificationRead", notificationId);
        }
        else
        {
            _logger.LogWarning("No se encontró la notificación {NotificationId} para el usuario {UserId}", notificationId, userId);
        }
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas
    /// Invocable desde el cliente: await connection.invoke('MarkAllAsRead')
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Intento de marcar todas las notificaciones como leídas sin userId válido");
            return;
        }

        // Actualizar en base de datos
        var count = await _notificationRepository.MarkAllAsReadAsync(userId);

        _logger.LogInformation("Usuario {UserId} marcó {Count} notificaciones como leídas", userId, count);

        // Notificar al cliente que todas las notificaciones fueron marcadas como leídas
        await Clients.User(userId).SendAsync("AllNotificationsRead");
    }
}
