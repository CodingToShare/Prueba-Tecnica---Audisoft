using AudiSoft.School.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Persistence;

/// <summary>
/// DbContext para AudiSoft.School.
/// Configura todas las entidades, relaciones y convenciones de EF Core.
/// </summary>
public class AudiSoftSchoolDbContext : DbContext
{
    public AudiSoftSchoolDbContext(DbContextOptions<AudiSoftSchoolDbContext> options)
        : base(options)
    {
    }

    public DbSet<Estudiante> Estudiantes { get; set; } = null!;
    public DbSet<Profesor> Profesores { get; set; } = null!;
    public DbSet<Nota> Notas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Estudiante
        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configuración de Profesor
        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configuración de Nota
        modelBuilder.Entity<Nota>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Valor)
                .HasPrecision(5, 2); // Máximo 999.99
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Relación con Profesor (1-N)
            entity.HasOne(n => n.Profesor)
                .WithMany(p => p.Notas)
                .HasForeignKey(n => n.IdProfesor)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Estudiante (1-N)
            entity.HasOne(n => n.Estudiante)
                .WithMany(e => e.Notas)
                .HasForeignKey(n => n.IdEstudiante)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
