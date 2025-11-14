-- Script para asignar IdProfesor e IdEstudiante a los usuarios correspondientes
-- Esto asegura que los claims en el JWT sean correctamente poblados

-- Asignar IdProfesor a usuarios con rol Profesor
UPDATE u
SET u.IdProfesor = p.Id,
    u.UpdatedAt = GETUTCDATE(),
    u.UpdatedBy = 'AdminScript'
FROM Usuarios u
INNER JOIN UsuarioRoles ur ON u.Id = ur.IdUsuario
INNER JOIN Roles r ON ur.IdRol = r.Id
INNER JOIN Profesores p ON u.UserName LIKE '%' + SUBSTRING(p.Nombre, 1, CHARINDEX(' ', p.Nombre)-1) + '%'
WHERE r.Nombre = 'Profesor'
  AND u.IdProfesor IS NULL
  AND p.IsDeleted = 0
  AND u.IsDeleted = 0;

-- Asignar IdEstudiante a usuarios con rol Estudiante
UPDATE u
SET u.IdEstudiante = e.Id,
    u.UpdatedAt = GETUTCDATE(),
    u.UpdatedBy = 'AdminScript'
FROM Usuarios u
INNER JOIN UsuarioRoles ur ON u.Id = ur.IdUsuario
INNER JOIN Roles r ON ur.IdRol = r.Id
INNER JOIN Estudiantes e ON u.UserName LIKE '%' + SUBSTRING(e.Nombre, 1, CHARINDEX(' ', e.Nombre)-1) + '%'
WHERE r.Nombre = 'Estudiante'
  AND u.IdEstudiante IS NULL
  AND e.IsDeleted = 0
  AND u.IsDeleted = 0;

-- Verificar asignaciones
SELECT 
    u.Id,
    u.UserName,
    r.Nombre as Rol,
    u.IdProfesor,
    u.IdEstudiante,
    CASE WHEN u.IdProfesor IS NOT NULL THEN p.Nombre ELSE 'N/A' END as NombreProfesor,
    CASE WHEN u.IdEstudiante IS NOT NULL THEN e.Nombre ELSE 'N/A' END as NombreEstudiante
FROM Usuarios u
LEFT JOIN UsuarioRoles ur ON u.Id = ur.IdUsuario
LEFT JOIN Roles r ON ur.IdRol = r.Id
LEFT JOIN Profesores p ON u.IdProfesor = p.Id
LEFT JOIN Estudiantes e ON u.IdEstudiante = e.Id
WHERE u.IsDeleted = 0
ORDER BY u.Id;
