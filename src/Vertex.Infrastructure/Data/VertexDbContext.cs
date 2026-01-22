using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación.
/// Hereda de IdentityDbContext<ApplicationUser> para autenticación con usuario personalizado.
/// </summary>
public class VertexDbContext : IdentityDbContext<ApplicationUser>
{
    public VertexDbContext(DbContextOptions<VertexDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Tabla de procesos de onboarding
    /// </summary>
    public DbSet<OnboardingProcess> OnboardingProcesses { get; set; }

    /// <summary>
    /// Tabla de perfiles profesionales
    /// </summary>
    public DbSet<ProfessionalProfile> ProfessionalProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de OnboardingProcess
        modelBuilder.Entity<OnboardingProcess>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450); // Longitud estándar para IDs de Identity

            entity.Property(e => e.SerializedData)
                .IsRequired()
                .HasColumnType("nvarchar(max)"); // Soporte para JSON largo en SQL Server

            entity.Property(e => e.CurrentStep)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índice único para garantizar un solo proceso activo por usuario
            entity.HasIndex(e => e.UserId)
                .IsUnique();
        });

        // Configuración de ProfessionalProfile
        modelBuilder.Entity<ProfessionalProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Summary)
                .HasMaxLength(1000);

            entity.Property(e => e.SkillsJson)
                .HasColumnType("nvarchar(max)"); // JSON de habilidades

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índice en UserId para búsquedas rápidas
            entity.HasIndex(e => e.UserId);
        });
    }
}
