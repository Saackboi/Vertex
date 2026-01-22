namespace Vertex.Application.DTOs;

/// <summary>
/// DTO para el inicio de sesión
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
