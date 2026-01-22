namespace Vertex.Domain.Entities;

/// <summary>
/// Representa una experiencia laboral del perfil profesional.
/// Relación: Muchos WorkExperience pertenecen a un ProfessionalProfile.
/// </summary>
public class WorkExperience
{
    /// <summary>
    /// ID de la experiencia laboral
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la empresa
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Cargo o rol desempeñado
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del empleo
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de finalización del empleo (null si es trabajo actual)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Descripción de responsabilidades y logros
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// FK: ID del perfil profesional al que pertenece
    /// </summary>
    public Guid ProfessionalProfileId { get; set; }

    /// <summary>
    /// Navegación: Perfil profesional dueño de esta experiencia
    /// </summary>
    public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
}
