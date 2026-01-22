namespace Vertex.Domain.Entities;

/// <summary>
/// Representa un título académico o educación formal del perfil profesional.
/// Relación: Muchos Education pertenecen a un ProfessionalProfile.
/// </summary>
public class Education
{
    /// <summary>
    /// ID del registro educativo
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la institución educativa
    /// </summary>
    public string Institution { get; set; } = string.Empty;

    /// <summary>
    /// Título o grado obtenido (Ej: "Ingeniería en Sistemas", "Bachiller")
    /// </summary>
    public string Degree { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio de los estudios
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de graduación (null si aún está estudiando)
    /// </summary>
    public DateTime? GraduationDate { get; set; }

    /// <summary>
    /// FK: ID del perfil profesional al que pertenece
    /// </summary>
    public Guid ProfessionalProfileId { get; set; }

    /// <summary>
    /// Navegación: Perfil profesional dueño de esta educación
    /// </summary>
    public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
}
