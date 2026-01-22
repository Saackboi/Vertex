using Vertex.Domain.Entities;

namespace Vertex.Application.Interfaces;

/// <summary>
/// Respuesta del generador de tokens JWT
/// </summary>
public record JwtTokenResponse(string Token, DateTime ExpiresAt);

/// <summary>
/// Interfaz para generación de tokens JWT.
/// Permite abstraer la lógica de JWT de la capa de aplicación.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un token JWT para el usuario autenticado
    /// </summary>
    /// <param name="user">Usuario autenticado</param>
    /// <returns>Token JWT y fecha de expiración</returns>
    JwtTokenResponse GenerateToken(ApplicationUser user);
}
