using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;

namespace Vertex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación para todos los endpoints
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IOnboardingService onboardingService,
        ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
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
    public async Task<ActionResult<ApiResponse<OnboardingStatusDto>>> SaveProgress([FromBody] SaveProgressDto dto)
    {
        // SEGURIDAD: Extraer UserId desde el token JWT
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Intento de guardar progreso sin userId en token");
            return Unauthorized(ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Usuario no autenticado", 
                401));
        }

        // Delegar la lógica al servicio
        var result = await _onboardingService.SaveProgressAsync(userId, dto);
        
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result)
        };
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
    public async Task<ActionResult<ApiResponse<OnboardingStatusDto>>> GetResume()
    {
        // SEGURIDAD: Extraer UserId desde el token JWT
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Intento de recuperar progreso sin userId en token");
            return Unauthorized(ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Usuario no autenticado", 
                401));
        }

        // Delegar la lógica al servicio
        var result = await _onboardingService.GetProgressAsync(userId);
        
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
