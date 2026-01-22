using Vertex.Domain.Entities;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Contrato para el repositorio de procesos de onboarding.
/// Define las operaciones de persistencia necesarias.
/// </summary>
public interface IOnboardingRepository
{
    /// <summary>
    /// Obtiene el proceso de onboarding activo de un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Proceso de onboarding o null si no existe</returns>
    Task<OnboardingProcess?> GetByUserIdAsync(string userId);

    /// <summary>
    /// Guarda o actualiza un proceso de onboarding (Upsert).
    /// Si existe un registro para el usuario, lo actualiza; si no, crea uno nuevo.
    /// </summary>
    /// <param name="process">Proceso de onboarding a guardar</param>
    /// <returns>Proceso guardado con los cambios aplicados</returns>
    Task<OnboardingProcess> SaveOrUpdateAsync(OnboardingProcess process);
}
