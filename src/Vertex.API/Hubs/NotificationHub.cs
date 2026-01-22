using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Vertex.API.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real.
/// Los clientes se conectan a este hub para recibir actualizaciones.
/// </summary>
[Authorize] // Requiere autenticación JWT
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
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
}
