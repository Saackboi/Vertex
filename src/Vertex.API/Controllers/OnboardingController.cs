using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;

namespace Vertex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingRepository _repository;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IOnboardingRepository repository,
        ILogger<OnboardingController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint POST para guardar el progreso del onboarding.
    /// SEGURIDAD: El UserId se extrae del token JWT, NO del request body.
    /// </summary>
    /// <param name="dto">Datos del progreso a guardar</param>
    /// <returns>Estado actual del proceso</returns>
    [HttpPost("save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OnboardingStatusDto>> SaveProgress([FromBody] SaveProgressDto dto)
    {
        try
        {
            // SEGURIDAD: Extraer UserId desde el token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Intento de guardar progreso sin userId en token");
                return Unauthorized("Usuario no autenticado");
            }

            // Mapear DTO a entidad de dominio
            var process = new OnboardingProcess
            {
                UserId = userId, // UserId extraído del token JWT (seguro)
                CurrentStep = dto.CurrentStep,
                SerializedData = dto.SerializedData,
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

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar progreso del onboarding");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Endpoint GET para recuperar el estado actual del onboarding del usuario autenticado.
    /// SEGURIDAD: El UserId se extrae del token JWT automáticamente.
    /// </summary>
    /// <returns>Estado del proceso o NotFound si no existe</returns>
    [HttpGet("resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OnboardingStatusDto>> GetResume()
    {
        try
        {
            // SEGURIDAD: Extraer UserId desde el token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Intento de recuperar progreso sin userId en token");
                return Unauthorized("Usuario no autenticado");
            }

            var process = await _repository.GetByUserIdAsync(userId);

            if (process == null)
            {
                return NotFound(new { message = "No se encontró un proceso de onboarding para este usuario" });
            }

            var response = new OnboardingStatusDto
            {
                CurrentStep = process.CurrentStep,
                SerializedData = process.SerializedData,
                IsCompleted = process.IsCompleted,
                UpdatedAt = process.UpdatedAt
            };

            _logger.LogInformation("Progreso recuperado para usuario {UserId}", userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recuperar estado del onboarding");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
