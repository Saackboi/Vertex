using Microsoft.EntityFrameworkCore;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;
using Vertex.Infrastructure.Data;

namespace Vertex.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta del repositorio de onboarding.
/// Gestiona la persistencia de procesos de onboarding con Entity Framework Core.
/// </summary>
public class OnboardingRepository : IOnboardingRepository
{
    private readonly VertexDbContext _context;

    public OnboardingRepository(VertexDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el proceso de onboarding de un usuario.
    /// </summary>
    public async Task<OnboardingProcess?> GetByUserIdAsync(string userId)
    {
        return await _context.OnboardingProcesses
            .FirstOrDefaultAsync(op => op.UserId == userId);
    }

    /// <summary>
    /// Guarda o actualiza un proceso de onboarding.
    /// REGLA DE NEGOCIO: Si existe un proceso para el usuario, lo actualiza; si no, crea uno nuevo.
    /// Esto previene duplicados y garantiza integridad referencial.
    /// </summary>
    public async Task<OnboardingProcess> SaveOrUpdateAsync(OnboardingProcess process)
    {
        // Verificar si ya existe un proceso para este usuario
        var existingProcess = await GetByUserIdAsync(process.UserId);

        if (existingProcess != null)
        {
            // ACTUALIZAR: Proceso existente
            existingProcess.CurrentStep = process.CurrentStep;
            existingProcess.SerializedData = process.SerializedData;
            existingProcess.IsCompleted = process.IsCompleted;
            existingProcess.UpdatedAt = DateTime.UtcNow;

            _context.OnboardingProcesses.Update(existingProcess);
            await _context.SaveChangesAsync();

            return existingProcess;
        }
        else
        {
            // CREAR: Nuevo proceso
            process.Id = Guid.NewGuid();
            process.UpdatedAt = DateTime.UtcNow;

            await _context.OnboardingProcesses.AddAsync(process);
            await _context.SaveChangesAsync();

            return process;
        }
    }
}
