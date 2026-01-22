namespace Vertex.Application.Interfaces;

/// <summary>
/// Patrón Unit of Work para gestionar transacciones explícitas.
/// Mantiene la integridad de Clean Architecture sin exponer DbContext a la capa Application.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Inicia una transacción explícita
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Confirma todos los cambios y hace commit de la transacción
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Deshace todos los cambios y hace rollback de la transacción
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// Guarda los cambios en la base de datos sin commit
    /// </summary>
    Task<int> SaveChangesAsync();
}
