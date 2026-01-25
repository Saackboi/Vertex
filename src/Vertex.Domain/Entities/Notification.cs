namespace Vertex.Domain.Entities;

/// <summary>
/// Entidad que representa una notificación en tiempo real del sistema.
/// Se utiliza para comunicación persistente de eventos entre backend y frontend.
/// </summary>
public class Notification
{
    /// <summary>
    /// Identificador único de la notificación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario destinatario (debe coincidir con ApplicationUser.Id)
    /// IMPORTANTE: Garantiza que las notificaciones solo se muestren al usuario correcto
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Título breve de la notificación
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo de la notificación
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación: "info", "success", "warning", "error"
    /// </summary>
    public string Type { get; set; } = "info";

    /// <summary>
    /// Indica si la notificación ha sido leída por el usuario
    /// </summary>
    public bool Read { get; set; } = false;

    /// <summary>
    /// Fecha y hora de creación de la notificación (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Datos adicionales en formato JSON (opcional)
    /// Útil para enviar información contextual (ej: profileId, orderId, etc.)
    /// </summary>
    public string? Data { get; set; }
}
