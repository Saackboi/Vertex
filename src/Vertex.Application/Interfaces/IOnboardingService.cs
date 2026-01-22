using Vertex.Application.DTOs;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Interfaz de servicio para la lógica de negocio de Onboarding
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Guarda o actualiza el progreso del onboarding de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario autenticado</param>
    /// <param name="dto">Datos del progreso</param>
    /// <returns>Estado actualizado del onboarding</returns>
    Task<ApiResponse<OnboardingStatusDto>> SaveProgressAsync(string userId, SaveProgressDto dto);

    /// <summary>
    /// Recupera el progreso actual del onboarding de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario autenticado</param>
    /// <returns>Estado del onboarding o error si no existe</returns>
    Task<ApiResponse<OnboardingStatusDto>> GetProgressAsync(string userId);
}
