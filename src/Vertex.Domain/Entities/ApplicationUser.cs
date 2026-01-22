using Microsoft.AspNetCore.Identity;

namespace Vertex.Domain.Entities;

/// <summary>
/// Entidad de usuario del sistema con soporte de Identity.
/// Extiende IdentityUser para agregar propiedades personalizadas.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}
