namespace Vertex.Application.DTOs;

/// <summary>
/// DTO para el registro de nuevos usuarios
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Email del usuario (también será el username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}
