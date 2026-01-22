using Microsoft.Extensions.Logging;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;

namespace Vertex.Application.Services;

/// <summary>
/// Servicio de aplicación para la lógica de negocio de Onboarding
/// </summary>
public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _repository;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IOnboardingRepository repository,
        ILogger<OnboardingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Guarda o actualiza el progreso del onboarding de un usuario
    /// </summary>
    public async Task<ApiResponse<OnboardingStatusDto>> SaveProgressAsync(string userId, SaveProgressDto dto)
    {
        try
        {
            // Validación de userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Intento de guardar progreso con userId vacío");
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "El ID de usuario es requerido",
                    400);
            }

            // Validación de datos
            if (dto.CurrentStep < 1)
            {
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "El paso actual debe ser mayor a 0",
                    400);
            }

            // Mapear DTO a entidad de dominio
            var process = new OnboardingProcess
            {
                UserId = userId,
                CurrentStep = dto.CurrentStep,
                SerializedData = dto.SerializedData ?? string.Empty,
                IsCompleted = dto.IsCompleted
            };

            // Guardar o actualizar en la base de datos
            var savedProcess = await _repository.SaveOrUpdateAsync(process);

            // Mapear entidad a DTO de respuesta
            var response = new OnboardingStatusDto
            {
                CurrentStep = savedProcess.CurrentStep,
                SerializedData = savedProcess.SerializedData,
                IsCompleted = savedProcess.IsCompleted,
                UpdatedAt = savedProcess.UpdatedAt
            };

            _logger.LogInformation(
                "Progreso guardado para usuario {UserId}, paso {Step}",
                userId,
                dto.CurrentStep);

            return ApiResponse<OnboardingStatusDto>.SuccessResponse(
                response,
                "Progreso guardado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar progreso del onboarding para usuario {UserId}", userId);
            return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Error al guardar el progreso",
                500,
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Recupera el progreso actual del onboarding de un usuario
    /// </summary>
    public async Task<ApiResponse<OnboardingStatusDto>> GetProgressAsync(string userId)
    {
        try
        {
            // Validación de userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Intento de recuperar progreso con userId vacío");
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "El ID de usuario es requerido",
                    400);
            }

            var process = await _repository.GetByUserIdAsync(userId);

            if (process == null)
            {
                _logger.LogInformation("No se encontró proceso de onboarding para usuario {UserId}", userId);
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "No se encontró un proceso de onboarding para este usuario",
                    404);
            }

            var response = new OnboardingStatusDto
            {
                CurrentStep = process.CurrentStep,
                SerializedData = process.SerializedData,
                IsCompleted = process.IsCompleted,
                UpdatedAt = process.UpdatedAt
            };

            _logger.LogInformation("Progreso recuperado para usuario {UserId}", userId);

            return ApiResponse<OnboardingStatusDto>.SuccessResponse(
                response,
                "Progreso recuperado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recuperar progreso del onboarding para usuario {UserId}", userId);
            return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Error al recuperar el progreso",
                500,
                new List<string> { ex.Message });
        }
    }
}
