using Microsoft.AspNetCore.Mvc;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;

namespace Vertex.API.Controllers;

/// <summary>
/// Controlador de autenticación.
/// Responsabilidad: Solo manejar requests/responses HTTP.
/// La lógica de negocio está en AuthService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="registerDto">Datos del nuevo usuario</param>
    /// <returns>Respuesta con el token JWT</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        // Delegar toda la lógica al servicio
        var result = await _authService.RegisterAsync(registerDto);
        
        return result.StatusCode switch
        {
            201 => Created(string.Empty, result), // 201 Created
            400 => BadRequest(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    /// <param name="loginDto">Credenciales del usuario</param>
    /// <returns>Respuesta con el token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        // Delegar toda la lógica al servicio
        var result = await _authService.LoginAsync(loginDto);
        
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            401 => Unauthorized(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
