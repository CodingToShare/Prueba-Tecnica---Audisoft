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
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<UsuarioRol> UsuarioRoles { get; set; } = null!;

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

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(e => e.UserName)
                .IsUnique();
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(255);
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Relación opcional con Profesor
            entity.HasOne(u => u.Profesor)
                .WithMany()
                .HasForeignKey(u => u.IdProfesor)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación opcional con Estudiante
            entity.HasOne(u => u.Estudiante)
                .WithMany()
                .HasForeignKey(u => u.IdEstudiante)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);
            entity.HasIndex(e => e.Nombre)
                .IsUnique();
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configuración de UsuarioRol
        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AsignadoPor)
                .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Índice único compuesto para evitar duplicados
            entity.HasIndex(e => new { e.IdUsuario, e.IdRol })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Relación con Usuario
            entity.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Rol
            entity.HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
