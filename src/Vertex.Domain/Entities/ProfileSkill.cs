namespace Vertex.Domain.Entities;

/// <summary>
/// Representa una habilidad técnica o blanda del perfil profesional.
/// Relación: Muchos ProfileSkill pertenecen a un ProfessionalProfile.
/// </summary>
public class ProfileSkill
{
    /// <summary>
    /// ID de la habilidad
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la habilidad (Ej: "C#", "Liderazgo", "Docker")
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de dominio (Opcional: 1-5, Básico/Intermedio/Avanzado)
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// FK: ID del perfil profesional al que pertenece
    /// </summary>
    public Guid ProfessionalProfileId { get; set; }

    /// <summary>
    /// Navegación: Perfil profesional dueño de esta habilidad
    /// </summary>
    public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
}
