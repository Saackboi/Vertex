using Vertex.Domain.Entities;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Interfaz del repositorio de perfiles profesionales.
/// Define el contrato para acceso a datos de ProfessionalProfile.
/// </summary>
public interface IProfessionalProfileRepository
{
    /// <summary>
    /// Crea un nuevo perfil profesional con todas sus relaciones
    /// </summary>
    /// <param name="profile">Perfil profesional con sus colecciones (Experiences, Educations, Skills)</param>
    /// <returns>Perfil guardado</returns>
    Task<ProfessionalProfile> CreateAsync(ProfessionalProfile profile);

    /// <summary>
    /// Obtiene un perfil profesional por el ID del usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Perfil con todas sus relaciones cargadas, o null si no existe</returns>
    Task<ProfessionalProfile?> GetByUserIdAsync(string userId);

    /// <summary>
    /// Obtiene un perfil profesional por su ID
    /// </summary>
    /// <param name="id">ID del perfil</param>
    /// <returns>Perfil con todas sus relaciones cargadas, o null si no existe</returns>
    Task<ProfessionalProfile?> GetByIdAsync(Guid id);
}
