using Vertex.Domain.ValueObjects;
namespace Vertex.Application.DTOs;

/// <summary>
/// DTO para recibir datos del frontend al guardar el progreso del onboarding.
/// Contiene solo la información necesaria sin exponer la entidad de dominio.
/// NOTA: UserId se obtiene del token JWT, NO se recibe del cliente.
/// </summary>
public class SaveProgressDto
{
    /// <summary>
    /// Paso actual del formulario
    /// </summary>
    public int CurrentStep { get; set; }

    /// <summary>
    /// Datos del formulario tipados
    /// </summary>
    public OnboardingData Data { get; set; } = new();

    /// <summary>
    /// Indica si el usuario completó el último paso
    /// </summary>
    public bool IsCompleted { get; set; } = false;
}
