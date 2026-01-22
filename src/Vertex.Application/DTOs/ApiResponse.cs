namespace Vertex.Application.DTOs;

/// <summary>
/// Clase genérica para estandarizar las respuestas de la API
/// </summary>
/// <typeparam name="T">Tipo de dato que contiene la respuesta</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo de la operación
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos de la respuesta (puede ser null si hay error)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores (si los hay)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Código de estado HTTP sugerido
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Constructor para respuestas exitosas
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operación exitosa", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Constructor para respuestas de error
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Constructor para respuesta sin datos (operaciones void)
    /// </summary>
    public static ApiResponse<T> SuccessResponseNoData(string message = "Operación exitosa", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }
}
