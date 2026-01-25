namespace Vertex.Application.Interfaces;

/// <summary>
/// Servicio para enviar notificaciones en tiempo real a los clientes.
/// Abstracción de SignalR para mantener la separación de capas.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envía una notificación personalizada a un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario destinatario</param>
    /// <param name="title">Título de la notificación</param>
    /// <param name="message">Mensaje descriptivo</param>
    /// <param name="type">Tipo: "info", "success", "warning", "error"</param>
    /// <param name="data">Datos adicionales opcionales en formato JSON</param>
    Task SendNotificationAsync(string userId, string title, string message, string type = "info", string? data = null);

    /// <summary>
    /// Notifica a un usuario específico sobre el progreso de su onboarding
    /// </summary>
    Task NotifyOnboardingProgressAsync(string userId, string message, int currentStep);

    /// <summary>
    /// Notifica a un usuario que su onboarding se completó exitosamente
    /// </summary>
    Task NotifyOnboardingCompletedAsync(string userId, string profileId);

    /// <summary>
    /// Notifica a todos los usuarios conectados (broadcast)
    /// </summary>
    Task NotifyAllAsync(string message);

    /// <summary>
    /// Notifica a un grupo específico de usuarios
    /// </summary>
    Task NotifyGroupAsync(string groupName, string message);
}
