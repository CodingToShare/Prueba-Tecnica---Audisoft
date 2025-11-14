-- =============================================
-- AudiSoft School API - Script de Creación e Inicialización
-- Descripción: Script completo para crear la base de datos, tablas y datos iniciales
-- Versión: 1.0
-- Fecha: 2025-11-14
-- =============================================

-- Crear la base de datos si no existe
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'AudiSoftSchoolDb')
BEGIN
    CREATE DATABASE AudiSoftSchoolDb;
    PRINT 'Base de datos AudiSoftSchoolDb creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Base de datos AudiSoftSchoolDb ya existe.';
END
GO

USE AudiSoftSchoolDb;
GO

-- =============================================
-- CREAR TABLAS
-- =============================================

-- Tabla Estudiantes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Estudiantes' AND xtype='U')
BEGIN
    CREATE TABLE Estudiantes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX IX_Estudiantes_Nombre ON Estudiantes(Nombre) WHERE IsDeleted = 0;
    CREATE INDEX IX_Estudiantes_CreatedAt ON Estudiantes(CreatedAt) WHERE IsDeleted = 0;
    
    PRINT 'Tabla Estudiantes creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Estudiantes ya existe.';
END
GO

-- Tabla Profesores
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Profesores' AND xtype='U')
BEGIN
    CREATE TABLE Profesores (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX IX_Profesores_Nombre ON Profesores(Nombre) WHERE IsDeleted = 0;
    CREATE INDEX IX_Profesores_CreatedAt ON Profesores(CreatedAt) WHERE IsDeleted = 0;
    
    PRINT 'Tabla Profesores creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Profesores ya existe.';
END
GO

-- Tabla Notas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notas' AND xtype='U')
BEGIN
    CREATE TABLE Notas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(200) NOT NULL,
        Valor DECIMAL(5,2) NOT NULL CHECK (Valor >= 0 AND Valor <= 100),
        IdProfesor INT NOT NULL,
        IdEstudiante INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        -- Claves foráneas
        CONSTRAINT FK_Notas_Profesores FOREIGN KEY (IdProfesor) REFERENCES Profesores(Id),
        CONSTRAINT FK_Notas_Estudiantes FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(Id)
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX IX_Notas_IdProfesor ON Notas(IdProfesor) WHERE IsDeleted = 0;
    CREATE INDEX IX_Notas_IdEstudiante ON Notas(IdEstudiante) WHERE IsDeleted = 0;
    CREATE INDEX IX_Notas_Valor ON Notas(Valor) WHERE IsDeleted = 0;
    CREATE INDEX IX_Notas_CreatedAt ON Notas(CreatedAt) WHERE IsDeleted = 0;
    
    PRINT 'Tabla Notas creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Notas ya existe.';
END
GO

-- Tabla Roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(50) NOT NULL UNIQUE,
        Descripcion NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    
    -- Índices
    CREATE INDEX IX_Roles_Nombre ON Roles(Nombre) WHERE IsDeleted = 0;
    CREATE INDEX IX_Roles_IsActive ON Roles(IsActive) WHERE IsDeleted = 0;
    
    PRINT 'Tabla Roles creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Roles ya existe.';
END
GO

-- Tabla Usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        LastLoginAt DATETIME2 NULL,
        IdProfesor INT NULL,
        IdEstudiante INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        -- Claves foráneas
        CONSTRAINT FK_Usuarios_Profesores FOREIGN KEY (IdProfesor) REFERENCES Profesores(Id),
        CONSTRAINT FK_Usuarios_Estudiantes FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(Id),
        
        -- Restricciones
        CONSTRAINT CK_Usuarios_ProfesorOrEstudiante CHECK (
            (IdProfesor IS NOT NULL AND IdEstudiante IS NULL) OR 
            (IdProfesor IS NULL AND IdEstudiante IS NOT NULL) OR 
            (IdProfesor IS NULL AND IdEstudiante IS NULL)
        )
    );
    
    -- Índices
    CREATE INDEX IX_Usuarios_UserName ON Usuarios(UserName) WHERE IsDeleted = 0;
    CREATE INDEX IX_Usuarios_Email ON Usuarios(Email) WHERE IsDeleted = 0 AND Email IS NOT NULL;
    CREATE INDEX IX_Usuarios_IsActive ON Usuarios(IsActive) WHERE IsDeleted = 0;
    CREATE INDEX IX_Usuarios_IdProfesor ON Usuarios(IdProfesor) WHERE IsDeleted = 0 AND IdProfesor IS NOT NULL;
    CREATE INDEX IX_Usuarios_IdEstudiante ON Usuarios(IdEstudiante) WHERE IsDeleted = 0 AND IdEstudiante IS NOT NULL;
    
    PRINT 'Tabla Usuarios creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Usuarios ya existe.';
END
GO

-- Tabla UsuarioRoles (relación many-to-many)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UsuarioRoles' AND xtype='U')
BEGIN
    CREATE TABLE UsuarioRoles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdUsuario INT NOT NULL,
        IdRol INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        
        -- Claves foráneas
        CONSTRAINT FK_UsuarioRoles_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
        CONSTRAINT FK_UsuarioRoles_Roles FOREIGN KEY (IdRol) REFERENCES Roles(Id),
        
        -- Evitar duplicados
        CONSTRAINT UK_UsuarioRoles_Usuario_Rol UNIQUE (IdUsuario, IdRol)
    );
    
    -- Índices
    CREATE INDEX IX_UsuarioRoles_IdUsuario ON UsuarioRoles(IdUsuario);
    CREATE INDEX IX_UsuarioRoles_IdRol ON UsuarioRoles(IdRol);
    
    PRINT 'Tabla UsuarioRoles creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla UsuarioRoles ya existe.';
END
GO

-- =============================================
-- INSERTAR DATOS INICIALES (SEED)
-- =============================================

PRINT 'Iniciando inserción de datos iniciales...';

-- Insertar Roles básicos
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Admin')
BEGIN
    INSERT INTO Roles (Nombre, Descripcion, IsActive, CreatedBy) VALUES
    ('Admin', 'Administrador del sistema con acceso completo', 1, 'SYSTEM'),
    ('Profesor', 'Profesor con permisos para gestionar notas de sus estudiantes', 1, 'SYSTEM'),
    ('Estudiante', 'Estudiante con permisos de solo lectura sobre sus notas', 1, 'SYSTEM');
    
    PRINT 'Roles iniciales insertados.';
END
ELSE
BEGIN
    PRINT 'Roles iniciales ya existen.';
END
GO

-- Insertar Profesores de ejemplo
IF NOT EXISTS (SELECT 1 FROM Profesores WHERE Nombre = 'María García López')
BEGIN
    INSERT INTO Profesores (Nombre, CreatedBy) VALUES
    ('María García López', 'SYSTEM'),
    ('Carlos Rodríguez Martín', 'SYSTEM'),
    ('Ana Fernández Silva', 'SYSTEM'),
    ('Miguel Torres Ruiz', 'SYSTEM'),
    ('Laura Sánchez Moreno', 'SYSTEM');
    
    PRINT 'Profesores de ejemplo insertados.';
END
ELSE
BEGIN
    PRINT 'Profesores de ejemplo ya existen.';
END
GO

-- Insertar Estudiantes de ejemplo
IF NOT EXISTS (SELECT 1 FROM Estudiantes WHERE Nombre = 'Juan Pérez González')
BEGIN
    INSERT INTO Estudiantes (Nombre, CreatedBy) VALUES
    ('Juan Pérez González', 'SYSTEM'),
    ('Sofia Martín López', 'SYSTEM'),
    ('Diego Hernández Castro', 'SYSTEM'),
    ('Valentina Ruiz Jiménez', 'SYSTEM'),
    ('Sebastián Torres Morales', 'SYSTEM'),
    ('Camila Vargas Silva', 'SYSTEM'),
    ('Mateo Rojas Herrera', 'SYSTEM'),
    ('Isabella Cruz Mendoza', 'SYSTEM'),
    ('Nicolás Reyes Vega', 'SYSTEM'),
    ('Antonella Flores Ramos', 'SYSTEM');
    
    PRINT 'Estudiantes de ejemplo insertados.';
END
ELSE
BEGIN
    PRINT 'Estudiantes de ejemplo ya existen.';
END
GO

-- Insertar Usuario Administrador
-- Contraseña: Admin@123456 (se debe cambiar en producción)
-- Hash generado con BCrypt para Admin@123456
DECLARE @AdminPasswordHash NVARCHAR(255) = '$2a$11$rIwbeWPG/Vp5k0Q2YsOP6OE.VjJhx1zZQmwNLKjGhzFQWi5ZXh9Gi';

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UserName = 'admin')
BEGIN
    INSERT INTO Usuarios (UserName, PasswordHash, Email, IsActive, CreatedBy)
    VALUES ('admin', @AdminPasswordHash, 'admin@audisoft.com', 1, 'SYSTEM');
    
    -- Asignar rol Admin
    DECLARE @AdminUserId INT = SCOPE_IDENTITY();
    DECLARE @AdminRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Admin');
    
    INSERT INTO UsuarioRoles (IdUsuario, IdRol, CreatedBy)
    VALUES (@AdminUserId, @AdminRolId, 'SYSTEM');
    
    PRINT 'Usuario administrador creado (admin / Admin@123456).';
END
ELSE
BEGIN
    PRINT 'Usuario administrador ya existe.';
END
GO

-- Insertar usuarios de ejemplo para profesores
-- Contraseña para todos: Profesor@123
DECLARE @ProfesorPasswordHash NVARCHAR(255) = '$2a$11$xWFBqU7r6YvrD5Jq6YGP6.SgI.lmH4k5YGrHKzO8WV4McY7QcX.h6';
DECLARE @ProfesorRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Profesor');

-- Profesor 1: María García
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UserName = 'maria.garcia')
BEGIN
    DECLARE @ProfesorId1 INT = (SELECT Id FROM Profesores WHERE Nombre = 'María García López');
    
    INSERT INTO Usuarios (UserName, PasswordHash, Email, IsActive, IdProfesor, CreatedBy)
    VALUES ('maria.garcia', @ProfesorPasswordHash, 'maria.garcia@audisoft.com', 1, @ProfesorId1, 'SYSTEM');
    
    INSERT INTO UsuarioRoles (IdUsuario, IdRol, CreatedBy)
    VALUES (SCOPE_IDENTITY(), @ProfesorRolId, 'SYSTEM');
    
    PRINT 'Usuario profesor María García creado.';
END

-- Profesor 2: Carlos Rodríguez  
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UserName = 'carlos.rodriguez')
BEGIN
    DECLARE @ProfesorId2 INT = (SELECT Id FROM Profesores WHERE Nombre = 'Carlos Rodríguez Martín');
    
    INSERT INTO Usuarios (UserName, PasswordHash, Email, IsActive, IdProfesor, CreatedBy)
    VALUES ('carlos.rodriguez', @ProfesorPasswordHash, 'carlos.rodriguez@audisoft.com', 1, @ProfesorId2, 'SYSTEM');
    
    INSERT INTO UsuarioRoles (IdUsuario, IdRol, CreatedBy)
    VALUES (SCOPE_IDENTITY(), @ProfesorRolId, 'SYSTEM');
    
    PRINT 'Usuario profesor Carlos Rodríguez creado.';
END
GO

-- Insertar usuarios de ejemplo para estudiantes
-- Contraseña para todos: Estudiante@123
DECLARE @EstudiantePasswordHash NVARCHAR(255) = '$2a$11$K4eD8Wm7qOjBXhFUO4gL2.Nz5PqRv3CyM8sT6bY9jK1aE7hF2nG3c';
DECLARE @EstudianteRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Estudiante');

-- Estudiante 1: Juan Pérez
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UserName = 'juan.perez')
BEGIN
    DECLARE @EstudianteId1 INT = (SELECT Id FROM Estudiantes WHERE Nombre = 'Juan Pérez González');
    
    INSERT INTO Usuarios (UserName, PasswordHash, Email, IsActive, IdEstudiante, CreatedBy)
    VALUES ('juan.perez', @EstudiantePasswordHash, 'juan.perez@student.audisoft.com', 1, @EstudianteId1, 'SYSTEM');
    
    INSERT INTO UsuarioRoles (IdUsuario, IdRol, CreatedBy)
    VALUES (SCOPE_IDENTITY(), @EstudianteRolId, 'SYSTEM');
    
    PRINT 'Usuario estudiante Juan Pérez creado.';
END

-- Estudiante 2: Sofia Martín
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE UserName = 'sofia.martin')
BEGIN
    DECLARE @EstudianteId2 INT = (SELECT Id FROM Estudiantes WHERE Nombre = 'Sofia Martín López');
    
    INSERT INTO Usuarios (UserName, PasswordHash, Email, IsActive, IdEstudiante, CreatedBy)
    VALUES ('sofia.martin', @EstudiantePasswordHash, 'sofia.martin@student.audisoft.com', 1, @EstudianteId2, 'SYSTEM');
    
    INSERT INTO UsuarioRoles (IdUsuario, IdRol, CreatedBy)
    VALUES (SCOPE_IDENTITY(), @EstudianteRolId, 'SYSTEM');
    
    PRINT 'Usuario estudiante Sofia Martín creado.';
END
GO

-- Insertar notas de ejemplo
IF NOT EXISTS (SELECT 1 FROM Notas WHERE Nombre = 'Examen Matemáticas')
BEGIN
    DECLARE @ProfesorMariaId INT = (SELECT Id FROM Profesores WHERE Nombre = 'María García López');
    DECLARE @ProfesorCarlosId INT = (SELECT Id FROM Profesores WHERE Nombre = 'Carlos Rodríguez Martín');
    DECLARE @EstudianteJuanId INT = (SELECT Id FROM Estudiantes WHERE Nombre = 'Juan Pérez González');
    DECLARE @EstudianteSofiaId INT = (SELECT Id FROM Estudiantes WHERE Nombre = 'Sofia Martín López');
    DECLARE @EstudianteDiegoId INT = (SELECT Id FROM Estudiantes WHERE Nombre = 'Diego Hernández Castro');
    
    INSERT INTO Notas (Nombre, Valor, IdProfesor, IdEstudiante, CreatedBy) VALUES
    -- Notas de María García
    ('Examen Matemáticas', 85.5, @ProfesorMariaId, @EstudianteJuanId, 'SYSTEM'),
    ('Tarea Álgebra', 92.0, @ProfesorMariaId, @EstudianteJuanId, 'SYSTEM'),
    ('Examen Matemáticas', 78.5, @ProfesorMariaId, @EstudianteSofiaId, 'SYSTEM'),
    ('Tarea Álgebra', 88.0, @ProfesorMariaId, @EstudianteSofiaId, 'SYSTEM'),
    ('Quiz Geometría', 95.0, @ProfesorMariaId, @EstudianteDiegoId, 'SYSTEM'),
    
    -- Notas de Carlos Rodríguez  
    ('Ensayo Literatura', 87.0, @ProfesorCarlosId, @EstudianteJuanId, 'SYSTEM'),
    ('Análisis de Texto', 91.5, @ProfesorCarlosId, @EstudianteSofiaId, 'SYSTEM'),
    ('Examen Gramática', 83.0, @ProfesorCarlosId, @EstudianteDiegoId, 'SYSTEM'),
    ('Presentación Oral', 89.0, @ProfesorCarlosId, @EstudianteJuanId, 'SYSTEM'),
    ('Comprensión Lectora', 94.5, @ProfesorCarlosId, @EstudianteSofiaId, 'SYSTEM');
    
    PRINT 'Notas de ejemplo insertadas.';
END
ELSE
BEGIN
    PRINT 'Notas de ejemplo ya existen.';
END
GO

-- Create __EFMigrationsHistory table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251112_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251112_InitialCreate', '10.0.0');
END
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251114022538_AddUserRoleEntities')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251114022538_AddUserRoleEntities', '8.0.0');
END
GO

-- =============================================
-- VERIFICACIÓN FINAL
-- =============================================

PRINT '================================================';
PRINT 'RESUMEN DE LA INSTALACIÓN';
PRINT '================================================';
PRINT 'Tablas creadas:';
SELECT 
    TABLE_NAME as 'Tabla',
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as 'Columnas'
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE' 
    AND TABLE_NAME IN ('Estudiantes', 'Profesores', 'Notas', 'Roles', 'Usuarios', 'UsuarioRoles')
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Registros insertados:';
SELECT 'Roles' as Tabla, COUNT(*) as Registros FROM Roles WHERE IsDeleted = 0
UNION ALL SELECT 'Profesores', COUNT(*) FROM Profesores WHERE IsDeleted = 0
UNION ALL SELECT 'Estudiantes', COUNT(*) FROM Estudiantes WHERE IsDeleted = 0  
UNION ALL SELECT 'Usuarios', COUNT(*) FROM Usuarios WHERE IsDeleted = 0
UNION ALL SELECT 'UsuarioRoles', COUNT(*) FROM UsuarioRoles
UNION ALL SELECT 'Notas', COUNT(*) FROM Notas WHERE IsDeleted = 0;

PRINT '';
PRINT '================================================';
PRINT 'USUARIOS DE PRUEBA CREADOS:';
PRINT '================================================';
PRINT 'Administrador:';
PRINT '  Usuario: admin';
PRINT '  Contraseña: Admin@123456';
PRINT '  Email: admin@audisoft.com';
PRINT '';
PRINT 'Profesores:';
PRINT '  Usuario: maria.garcia / Contraseña: Profesor@123';
PRINT '  Usuario: carlos.rodriguez / Contraseña: Profesor@123';
PRINT '';
PRINT 'Estudiantes:';
PRINT '  Usuario: juan.perez / Contraseña: Estudiante@123';
PRINT '  Usuario: sofia.martin / Contraseña: Estudiante@123';
PRINT '';
PRINT '¡IMPORTANTE!: Cambiar las contraseñas por defecto en entorno de producción.';
PRINT '================================================';
PRINT 'INSTALACIÓN COMPLETADA EXITOSAMENTE';
PRINT '================================================';