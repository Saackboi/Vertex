namespace Vertex.Domain.Entities;

/// <summary>
/// Entidad que representa el perfil profesional final (CV) del usuario.
/// Se genera una vez completado el proceso de onboarding.
/// </summary>
public class ProfessionalProfile
{
    /// <summary>
    /// Identificador único del perfil profesional
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario propietario del perfil
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del profesional
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Resumen profesional o biografía
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Habilidades del profesional serializadas en formato JSON
    /// Ejemplo: ["C#", "SQL Server", "React"]
    /// </summary>
    public string SkillsJson { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del perfil
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
