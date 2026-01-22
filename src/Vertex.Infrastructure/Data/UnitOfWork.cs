using Microsoft.EntityFrameworkCore.Storage;
using Vertex.Application.Interfaces;

namespace Vertex.Infrastructure.Data;

/// <summary>
/// Implementación del patrón Unit of Work con Entity Framework Core.
/// Gestiona transacciones explícitas manteniendo la separación de capas.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly VertexDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(VertexDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Inicia una transacción explícita con nivel de aislamiento por defecto
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Confirma la transacción y persiste todos los cambios
    /// </summary>
    public async Task CommitAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No hay transacción activa para hacer commit");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Deshace todos los cambios de la transacción
    /// </summary>
    public async Task RollbackAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No hay transacción activa para hacer rollback");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Guarda cambios sin afectar la transacción
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Libera recursos de la transacción si existe
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
