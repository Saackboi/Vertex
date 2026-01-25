using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vertex.Application.Interfaces;
using Vertex.Application.DTOs;

namespace Vertex.API.Controllers;

/// <summary>
/// Controlador para gestión de notificaciones en tiempo real.
/// Permite obtener historial de notificaciones y estadísticas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationRepository notificationRepository,
        ILogger<NotificationsController> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las notificaciones del usuario autenticado (últimas 50)
    /// </summary>
    /// <returns>Lista de notificaciones ordenadas por fecha descendente</returns>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 50)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Usuario no autenticado"));
        }

        try
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, limit);
            return Ok(ApiResponse<object>.SuccessResponse(notifications, "Notificaciones obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener notificaciones"));
        }
    }

    /// <summary>
    /// Obtiene solo las notificaciones no leídas del usuario autenticado
    /// </summary>
    /// <returns>Lista de notificaciones no leídas</returns>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Usuario no autenticado"));
        }

        try
        {
            var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(notifications, "Notificaciones no leídas obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones no leídas para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener notificaciones no leídas"));
        }
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas del usuario autenticado
    /// </summary>
    /// <returns>Número de notificaciones no leídas</returns>
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Usuario no autenticado"));
        }

        try
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { unreadCount = count }, "Conteo de notificaciones no leídas obtenido exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conteo de notificaciones no leídas para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener conteo de notificaciones"));
        }
    }

    /// <summary>
    /// Marca una notificación específica como leída
    /// </summary>
    /// <param name="id">ID de la notificación</param>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Usuario no autenticado"));
        }

        try
        {
            var success = await _notificationRepository.MarkAsReadAsync(id, userId);
            
            if (!success)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Notificación no encontrada"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { notificationId = id }, "Notificación marcada como leída"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {NotificationId} como leída para usuario {UserId}", id, userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al marcar notificación como leída"));
        }
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Usuario no autenticado"));
        }

        try
        {
            var count = await _notificationRepository.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(
                new { markedCount = count }, 
                $"{count} notificaciones marcadas como leídas"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas para usuario {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al marcar notificaciones como leídas"));
        }
    }
}
