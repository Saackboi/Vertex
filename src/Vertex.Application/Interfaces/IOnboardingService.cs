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

    /// <summary>
    /// Finaliza el proceso de onboarding y convierte el JSON temporal en un ProfessionalProfile relacional.
    /// REGLA DE NEGOCIO: Solo se puede completar si IsCompleted es false y hay datos válidos.
    /// </summary>
    /// <param name="userId">ID del usuario (extraído del JWT)</param>
    /// <returns>Perfil profesional creado</returns>
    Task<ApiResponse<ProfessionalProfileDto>> CompleteOnboardingAsync(string userId);
}
