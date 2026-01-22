using Vertex.Application.DTOs;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Interfaz del servicio de autenticación.
/// Maneja toda la lógica de registro, login y generación de tokens JWT.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="registerDto">Datos de registro del usuario</param>
    /// <returns>Respuesta con token JWT y datos del usuario</returns>
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    /// <param name="loginDto">Credenciales del usuario</param>
    /// <returns>Respuesta con token JWT y datos del usuario</returns>
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
}
