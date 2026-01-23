using Vertex.Domain.ValueObjects;
namespace Vertex.Application.DTOs;

/// <summary>
/// DTO de respuesta que contiene el estado actual del proceso de onboarding.
/// Se envía al frontend para reanudar el formulario.
/// </summary>
public class OnboardingStatusDto
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
    /// Indica si el proceso está completado
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
