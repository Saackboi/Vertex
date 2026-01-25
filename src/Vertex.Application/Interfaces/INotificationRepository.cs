using Vertex.Domain.Entities;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Repositorio para operaciones de persistencia de notificaciones.
/// Permite almacenar, consultar y actualizar notificaciones en tiempo real.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Obtiene todas las notificaciones de un usuario ordenadas por fecha (más recientes primero)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="limit">Número máximo de notificaciones a retornar (por defecto 50)</param>
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, int limit = 50);

    /// <summary>
    /// Obtiene solo las notificaciones no leídas de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>
    /// Agrega una nueva notificación a la base de datos
    /// </summary>
    /// <param name="notification">Entidad de notificación a persistir</param>
    Task<Notification> AddAsync(Notification notification);

    /// <summary>
    /// Marca una notificación específica como leída
    /// </summary>
    /// <param name="notificationId">ID de la notificación</param>
    /// <param name="userId">ID del usuario (para validar pertenencia)</param>
    Task<bool> MarkAsReadAsync(Guid notificationId, string userId);

    /// <summary>
    /// Marca todas las notificaciones de un usuario como leídas
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    Task<int> MarkAllAsReadAsync(string userId);

    /// <summary>
    /// Elimina notificaciones antiguas (limpieza automática)
    /// </summary>
    /// <param name="olderThanDays">Eliminar notificaciones con más de X días</param>
    Task<int> DeleteOldNotificationsAsync(int olderThanDays = 30);

    /// <summary>
    /// Obtiene la última notificación de progreso de onboarding para un usuario
    /// </summary>
    Task<Notification?> GetLatestProgressNotificationAsync(string userId);

    /// <summary>
    /// Actualiza una notificación existente
    /// </summary>
    Task UpdateAsync(Notification notification);
}
