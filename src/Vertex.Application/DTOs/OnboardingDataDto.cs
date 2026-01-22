namespace Vertex.Application.DTOs;

/// <summary>
/// DTO temporal para deserializar el JSON de OnboardingProcess.SerializedData
/// Este objeto representa la estructura completa del borrador del CV.
/// </summary>
public class OnboardingDataDto
{
    /// <summary>
    /// Información personal del usuario
    /// </summary>
    public PersonalInfoDto? PersonalInfo { get; set; }

    /// <summary>
    /// Lista de experiencias laborales
    /// </summary>
    public List<WorkExperienceDto> Experiences { get; set; } = new();

    /// <summary>
    /// Lista de educación formal
    /// </summary>
    public List<EducationDto> Educations { get; set; } = new();

    /// <summary>
    /// Lista de habilidades
    /// </summary>
    public List<SkillDto> Skills { get; set; } = new();
}

/// <summary>
/// Información personal del borrador
/// </summary>
public class PersonalInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// DTO de experiencia laboral del JSON
/// </summary>
public class WorkExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO de educación del JSON
/// </summary>
public class EducationDto
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? GraduationDate { get; set; }
}

/// <summary>
/// DTO de habilidad del JSON
/// </summary>
public class SkillDto
{
    public string SkillName { get; set; } = string.Empty;
    public string? Level { get; set; }
}

/// <summary>
/// DTO de respuesta para el perfil profesional completo
/// </summary>
public class ProfessionalProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<WorkExperienceDto> Experiences { get; set; } = new();
    public List<EducationDto> Educations { get; set; } = new();
    public List<SkillDto> Skills { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
