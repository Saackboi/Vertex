using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vertex.Domain.Entities;
using Vertex.Domain.ValueObjects;

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

    /// <summary>
    /// Tabla de experiencias laborales
    /// </summary>
    public DbSet<WorkExperience> WorkExperiences { get; set; }

    /// <summary>
    /// Tabla de educación formal
    /// </summary>
    public DbSet<Education> Educations { get; set; }

    /// <summary>
    /// Tabla de habilidades profesionales
    /// </summary>
    public DbSet<ProfileSkill> ProfileSkills { get; set; }

    /// <summary>
    /// Tabla de notificaciones en tiempo real persistentes
    /// </summary>
    public DbSet<Notification> Notifications { get; set; }

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

            // Mapea la propiedad tipada Data como JSON en una columna nvarchar(max)
            entity.OwnsOne(p => p.Data, b =>
            {
                b.ToJson();

                // Colección de experiencias (sin configuración adicional, EF mapea automáticamente)
                b.OwnsMany(d => d.Experiences);

                // Colección de educación (sin configuración adicional, EF mapea automáticamente)
                b.OwnsMany(d => d.Educations);
            });

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

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Índice en UserId para búsquedas rápidas
            entity.HasIndex(e => e.UserId);

            // Relaciones 1:N con cascada (borrar perfil elimina sus hijos)
            entity.HasMany(p => p.Experiences)
                .WithOne(e => e.ProfessionalProfile)
                .HasForeignKey(e => e.ProfessionalProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Educations)
                .WithOne(e => e.ProfessionalProfile)
                .HasForeignKey(e => e.ProfessionalProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Skills)
                .WithOne(s => s.ProfessionalProfile)
                .HasForeignKey(s => s.ProfessionalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de WorkExperience
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.StartDate)
                .IsRequired();
        });

        // Configuración de Education
        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Institution)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Degree)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.StartDate)
                .IsRequired();
        });

        // Configuración de ProfileSkill
        modelBuilder.Entity<ProfileSkill>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SkillName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Level)
                .HasMaxLength(50);
        });

        // Configuración de Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("info");

            entity.Property(e => e.Read)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.Timestamp)
                .IsRequired();

            entity.Property(e => e.Data)
                .HasMaxLength(4000); // JSON opcional con datos adicionales

            // Índice para consultas rápidas por usuario y estado
            entity.HasIndex(e => new { e.UserId, e.Read, e.Timestamp })
                .HasDatabaseName("IX_Notifications_UserId_Read_Timestamp");

            // Índice adicional solo por UserId para obtener todas las notificaciones del usuario
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Notifications_UserId");
        });
    }
}
