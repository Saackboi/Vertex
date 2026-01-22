using Microsoft.EntityFrameworkCore;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;
using Vertex.Infrastructure.Data;

namespace Vertex.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de perfiles profesionales.
/// Responsabilidad: Acceso a datos de ProfessionalProfile y sus relaciones.
/// </summary>
public class ProfessionalProfileRepository : IProfessionalProfileRepository
{
    private readonly VertexDbContext _context;

    public ProfessionalProfileRepository(VertexDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea un nuevo perfil profesional con todas sus relaciones en una transacción
    /// </summary>
    public async Task<ProfessionalProfile> CreateAsync(ProfessionalProfile profile)
    {
        // EF Core detecta automáticamente las relaciones y guarda todo en cascada
        await _context.ProfessionalProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    /// <summary>
    /// Obtiene un perfil por UserId con todas sus relaciones (Eager Loading)
    /// </summary>
    public async Task<ProfessionalProfile?> GetByUserIdAsync(string userId)
    {
        return await _context.ProfessionalProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    /// <summary>
    /// Obtiene un perfil por ID con todas sus relaciones (Eager Loading)
    /// </summary>
    public async Task<ProfessionalProfile?> GetByIdAsync(Guid id)
    {
        return await _context.ProfessionalProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
