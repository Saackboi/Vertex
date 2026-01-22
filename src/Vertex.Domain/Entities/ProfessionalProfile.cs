namespace Vertex.Domain.Entities;

/// <summary>
/// Entidad que representa el perfil profesional final (CV) del usuario.
/// Se genera una vez completado el proceso de onboarding.
/// Modelo Relacional: Usa colecciones de navegación en lugar de JSON plano.
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
    /// Colección de experiencias laborales (Relación 1:N)
    /// </summary>
    public ICollection<WorkExperience> Experiences { get; set; } = new List<WorkExperience>();

    /// <summary>
    /// Colección de educación formal (Relación 1:N)
    /// </summary>
    public ICollection<Education> Educations { get; set; } = new List<Education>();

    /// <summary>
    /// Colección de habilidades técnicas y blandas (Relación 1:N)
    /// </summary>
    public ICollection<ProfileSkill> Skills { get; set; } = new List<ProfileSkill>();

    /// <summary>
    /// Fecha de creación del perfil
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
