using Microsoft.EntityFrameworkCore;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;
using Vertex.Infrastructure.Data;

namespace Vertex.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de notificaciones usando Entity Framework Core.
/// Maneja la persistencia de notificaciones en tiempo real en la base de datos.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly VertexDbContext _context;

    public NotificationRepository(VertexDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las notificaciones de un usuario ordenadas por fecha (más recientes primero)
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, int limit = 50)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene solo las notificaciones no leídas de un usuario
    /// </summary>
    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.Read)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas de un usuario
    /// </summary>
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.Read)
            .CountAsync();
    }

    /// <summary>
    /// Agrega una nueva notificación a la base de datos
    /// </summary>
    public async Task<Notification> AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    /// <summary>
    /// Marca una notificación específica como leída
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return false;

        notification.Read = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Marca todas las notificaciones de un usuario como leídas
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.Read)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.Read = true;
        }

        await _context.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Elimina notificaciones antiguas (limpieza automática)
    /// </summary>
    public async Task<int> DeleteOldNotificationsAsync(int olderThanDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        var oldNotifications = await _context.Notifications
            .Where(n => n.Timestamp < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();

        return oldNotifications.Count;
    }

    /// <summary>
    /// Obtiene la última notificación de progreso de onboarding para un usuario
    /// Identificada por título fijo "Progreso del Onboarding"
    /// </summary>
    public async Task<Notification?> GetLatestProgressNotificationAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.Title == "Progreso del Onboarding")
            .OrderByDescending(n => n.Timestamp)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Actualiza una notificación existente
    /// </summary>
    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }
}
