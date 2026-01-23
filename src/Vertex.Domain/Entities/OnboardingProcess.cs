using Vertex.Domain.ValueObjects;
namespace Vertex.Domain.Entities;

/// <summary>
/// Entidad que representa el proceso de onboarding de un usuario.
/// Almacena el progreso del formulario multi-paso y los datos serializados.
/// </summary>
public class OnboardingProcess
{
    /// <summary>
    /// Identificador único del proceso de onboarding
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario del sistema de identidad (AspNetUsers)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Paso actual del formulario (default: 1)
    /// </summary>
    public int CurrentStep { get; set; } = 1;

    /// <summary>
    /// Datos del formulario tipados; EF Core los persiste como JSON
    /// </summary>
    public OnboardingData Data { get; set; }

    /// <summary>
    /// Fecha de última actualización del proceso
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el proceso de onboarding ha sido completado
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    public OnboardingProcess()
    {
        Data = new OnboardingData();
    }
}
