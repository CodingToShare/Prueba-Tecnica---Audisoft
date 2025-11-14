using AudiSoft.School.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Persistence;

/// <summary>
/// Clase para seeder inicial de datos
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seeder inicial de la base de datos
    /// </summary>
    /// <param name="context">Contexto de base de datos</param>
    public static async Task SeedAsync(AudiSoftSchoolDbContext context)
    {
        // Asegurar que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();

        // Verificar si ya existen datos
        if (await context.Roles.AnyAsync())
        {
            return; // Ya hay datos sembrados
        }

        // Crear roles básicos
        var roles = new List<Rol>
        {
            new Rol
            {
                Nombre = "Admin",
                Descripcion = "Administrador del sistema con acceso completo",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Rol
            {
                Nombre = "Profesor",
                Descripcion = "Profesor con acceso a gestión de estudiantes y notas",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Rol
            {
                Nombre = "Estudiante",
                Descripcion = "Estudiante con acceso limitado a consulta de notas",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // Crear profesores de ejemplo
        var profesores = new List<Profesor>
        {
            new Profesor
            {
                Nombre = "Juan Carlos García López - Matemáticas",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Profesor
            {
                Nombre = "María Elena Rodríguez Sánchez - Historia",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Profesor
            {
                Nombre = "Carlos Alberto Mendoza Pérez - Ciencias",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await context.Profesores.AddRangeAsync(profesores);
        await context.SaveChangesAsync();

        // Crear estudiantes de ejemplo
        var estudiantes = new List<Estudiante>
        {
            new Estudiante
            {
                Nombre = "Ana Isabel Martínez Torres - 10°",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Estudiante
            {
                Nombre = "Diego Alejandro Vásquez Herrera - 9°",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Estudiante
            {
                Nombre = "Lucía Patricia Morales Jiménez - 10°",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Estudiante
            {
                Nombre = "Santiago José Castro Ruiz - 9°",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await context.Estudiantes.AddRangeAsync(estudiantes);
        await context.SaveChangesAsync();

        // Crear usuarios de ejemplo
        var usuarios = new List<Usuario>();

        // Usuario administrador
        usuarios.Add(new Usuario
        {
            UserName = "admin",
            Email = "admin@audisoft.com",
            PasswordHash = HashPassword("Admin123!"), // Usar el mismo método que en UsuarioService
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        });

        // Usuarios para profesores
        for (int i = 0; i < profesores.Count; i++)
        {
            var profesor = profesores[i];
            usuarios.Add(new Usuario
            {
                UserName = $"prof{i + 1}",
                Email = $"prof{i + 1}@audisoft.com",
                PasswordHash = HashPassword("Profesor123!"),
                IdProfesor = profesor.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        // Usuarios para estudiantes
        for (int i = 0; i < estudiantes.Count; i++)
        {
            var estudiante = estudiantes[i];
            usuarios.Add(new Usuario
            {
                UserName = $"est{i + 1}",
                Email = $"est{i + 1}@estudiante.audisoft.com",
                PasswordHash = HashPassword("Estudiante123!"),
                IdEstudiante = estudiante.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        await context.Usuarios.AddRangeAsync(usuarios);
        await context.SaveChangesAsync();

        // Asignar roles a usuarios
        var usuarioRoles = new List<UsuarioRol>();

        // Asignar rol Admin al usuario admin
        var adminUser = usuarios.First(u => u.UserName == "admin");
        var adminRole = roles.First(r => r.Nombre == "Admin");
        usuarioRoles.Add(new UsuarioRol
        {
            IdUsuario = adminUser.Id,
            IdRol = adminRole.Id,
            AsignadoEn = DateTime.UtcNow,
            AsignadoPor = "System",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        });

        // Asignar rol Profesor a usuarios de profesores
        var profesorRole = roles.First(r => r.Nombre == "Profesor");
        var usuariosProfesores = usuarios.Where(u => u.IdProfesor != null);
        foreach (var usuarioProfesor in usuariosProfesores)
        {
            usuarioRoles.Add(new UsuarioRol
            {
                IdUsuario = usuarioProfesor.Id,
                IdRol = profesorRole.Id,
                AsignadoEn = DateTime.UtcNow,
                AsignadoPor = "System",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        // Asignar rol Estudiante a usuarios de estudiantes
        var estudianteRole = roles.First(r => r.Nombre == "Estudiante");
        var usuarioEstudiantes = usuarios.Where(u => u.IdEstudiante != null);
        foreach (var usuarioEstudiante in usuarioEstudiantes)
        {
            usuarioRoles.Add(new UsuarioRol
            {
                IdUsuario = usuarioEstudiante.Id,
                IdRol = estudianteRole.Id,
                AsignadoEn = DateTime.UtcNow,
                AsignadoPor = "System",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        await context.UsuarioRoles.AddRangeAsync(usuarioRoles);
        await context.SaveChangesAsync();

        // Crear algunas notas de ejemplo
        var notas = new List<Nota>();
        var profesorMath = profesores.First(p => p.Nombre.Contains("Matemáticas"));
        var profesorHistory = profesores.First(p => p.Nombre.Contains("Historia"));

        foreach (var estudiante in estudiantes)
        {
            // Notas de matemáticas
            notas.Add(new Nota
            {
                Nombre = "Evaluación Matemáticas - Parcial 1",
                IdEstudiante = estudiante.Id,
                IdProfesor = profesorMath.Id,
                Valor = GetRandomGrade(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });

            // Notas de historia
            notas.Add(new Nota
            {
                Nombre = "Evaluación Historia - Examen 1",
                IdEstudiante = estudiante.Id,
                IdProfesor = profesorHistory.Id,
                Valor = GetRandomGrade(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        await context.Notas.AddRangeAsync(notas);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Hashea una contraseña usando el mismo método que UsuarioService
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var saltBytes = System.Text.Encoding.UTF8.GetBytes("AudiSoft_School_Salt_2024");
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
        
        Array.Copy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
        Array.Copy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
        
        var hashBytes = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Genera una nota aleatoria entre 1.0 y 5.0
    /// </summary>
    private static decimal GetRandomGrade()
    {
        var random = new Random();
        return Math.Round((decimal)(random.NextDouble() * 4.0 + 1.0), 2);
    }
}