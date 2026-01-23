using Microsoft.Extensions.Logging;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;
using Vertex.Domain.ValueObjects;

namespace Vertex.Application.Services;

/// <summary>
/// Servicio de aplicación para la lógica de negocio de Onboarding
/// </summary>
public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _repository;
    private readonly IProfessionalProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IOnboardingRepository repository,
        IProfessionalProfileRepository profileRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<OnboardingService> logger)
    {
        _repository = repository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
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
                Data = dto.Data ?? new OnboardingData(),
                IsCompleted = dto.IsCompleted
            };

            // Guardar o actualizar en la base de datos
            var savedProcess = await _repository.SaveOrUpdateAsync(process);

            // Mapear entidad a DTO de respuesta
            var response = new OnboardingStatusDto
            {
                CurrentStep = savedProcess.CurrentStep,
                Data = savedProcess.Data,
                IsCompleted = savedProcess.IsCompleted,
                UpdatedAt = savedProcess.UpdatedAt
            };

            _logger.LogInformation(
                "Progreso guardado para usuario {UserId}, paso {Step}",
                userId,
                dto.CurrentStep);

            // 🔔 NOTIFICACIÓN EN TIEMPO REAL: Notificar al usuario sobre el progreso
            await _notificationService.NotifyOnboardingProgressAsync(
                userId,
                $"Progreso guardado en el paso {dto.CurrentStep}",
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
                Data = process.Data,
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

    /// <summary>
    /// Finaliza el proceso de onboarding y convierte el JSON en un ProfessionalProfile relacional.
    /// TRANSACCIÓN: Todo se guarda o nada se guarda (atomicidad).
    /// </summary>
    public async Task<ApiResponse<ProfessionalProfileDto>> CompleteOnboardingAsync(string userId)
    {
        try
        {
            // 1. VALIDACIÓN DE USUARIO
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Intento de completar onboarding con userId vacío");
                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "El ID de usuario es requerido",
                    400);
            }

            // 2. OBTENER PROCESO DE ONBOARDING
            var process = await _repository.GetByUserIdAsync(userId);
            if (process == null)
            {
                _logger.LogWarning("Intento de completar onboarding sin proceso existente para usuario {UserId}", userId);
                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "No se encontró un proceso de onboarding para este usuario",
                    404);
            }

            // 3. VERIFICAR QUE NO ESTÉ YA COMPLETADO
            if (process.IsCompleted)
            {
                _logger.LogWarning("Intento de completar onboarding ya finalizado para usuario {UserId}", userId);
                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "El proceso de onboarding ya fue completado anteriormente",
                    400);
            }

            // 4. USAR LOS DATOS TIPADOS DEL PROCESO
            var data = process.Data;

            // 5. VALIDAR QUE HAYA DATOS MÍNIMOS
            if (string.IsNullOrWhiteSpace(data.FullName))
            {
                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "Debe completar al menos el nombre completo",
                    400);
            }

            // 5.1 VALIDAR QUE NO EXISTA YA UN PERFIL PROFESIONAL
            var existingProfile = await _profileRepository.GetByUserIdAsync(userId);
            if (existingProfile != null)
            {
                _logger.LogWarning("Intento de crear perfil duplicado para usuario {UserId}", userId);
                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "Ya existe un perfil profesional para este usuario. No se puede completar el onboarding múltiples veces.",
                    400);
            }

            // 6. CREAR PERFIL PROFESIONAL CON RELACIONES
            var profile = new ProfessionalProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = data.FullName,
                Summary = data.Summary ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 7. MAPEAR EXPERIENCIAS LABORALES
            foreach (var exp in data.Experiences)
            {
                profile.Experiences.Add(new WorkExperience
                {
                    CompanyName = exp.Company,
                    Role = exp.Role,
                    StartDate = exp.DateRange.Start,
                    EndDate = exp.DateRange.End,
                    Description = string.Empty,
                    ProfessionalProfileId = profile.Id
                });
            }

            // 8. MAPEAR EDUCACIÓN
            foreach (var edu in data.Educations)
            {
                profile.Educations.Add(new Education
                {
                    Institution = edu.Institution,
                    Degree = edu.Degree,
                    StartDate = edu.DateRange.Start,
                    GraduationDate = edu.DateRange.End,
                    ProfessionalProfileId = profile.Id
                });
            }

            // 9. MAPEAR HABILIDADES
            foreach (var skill in data.Skills)
            {
                profile.Skills.Add(new ProfileSkill
                {
                    SkillName = skill,
                    Level = null,
                    ProfessionalProfileId = profile.Id
                });
            }

            // 10. GUARDAR (EF Core maneja transacciones automáticamente)
            // Cada SaveChangesAsync() es una transacción completa
            try
            {
                // Guardar perfil profesional con todas sus relaciones
                await _profileRepository.CreateAsync(profile);

                // Marcar onboarding como completado
                process.IsCompleted = true;
                process.UpdatedAt = DateTime.UtcNow;
                await _repository.SaveOrUpdateAsync(process);

                _logger.LogInformation(
                    "Onboarding completado exitosamente. Perfil {ProfileId} creado para usuario {UserId}",
                    profile.Id,
                    userId);

                // 🔔 NOTIFICACIÓN EN TIEMPO REAL: Notificar completación del onboarding
                await _notificationService.NotifyOnboardingCompletedAsync(userId, profile.Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar onboarding para usuario {UserId}", userId);

                return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                    "Error al completar el onboarding",
                    500,
                    new List<string> { ex.Message });
            }

            // 11. CONSTRUIR DTO DE RESPUESTA
            var responseDto = new ProfessionalProfileDto
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Summary = profile.Summary,
                Experiences = data.Experiences.Select(e => new WorkExperienceDto
                {
                    CompanyName = e.Company,
                    Role = e.Role,
                    StartDate = e.DateRange.Start,
                    EndDate = e.DateRange.End,
                    Description = string.Empty
                }).ToList(),
                Educations = data.Educations.Select(e => new EducationDto
                {
                    Institution = e.Institution,
                    Degree = e.Degree,
                    StartDate = e.DateRange.Start,
                    GraduationDate = e.DateRange.End
                }).ToList(),
                Skills = data.Skills.Select(s => new SkillDto
                {
                    SkillName = s,
                    Level = null
                }).ToList(),
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return ApiResponse<ProfessionalProfileDto>.SuccessResponse(
                responseDto,
                "Onboarding completado exitosamente. Tu perfil profesional ha sido creado.",
                201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al completar onboarding para usuario {UserId}", userId);
            return ApiResponse<ProfessionalProfileDto>.ErrorResponse(
                "Error inesperado al procesar el onboarding",
                500,
                new List<string> { ex.Message });
        }
    }
}
