namespace Vertex.Application.DTOs;

/// <summary>
/// DTO de respuesta con el token JWT
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Token JWT para autenticación
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario autenticado
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expiración del token
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
