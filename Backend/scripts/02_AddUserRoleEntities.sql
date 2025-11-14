-- Migration script for User/Role entities
-- Generated from: 20251114022538_AddUserRoleEntities

USE AudiSoftSchool;
GO

-- Create Roles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [Nombre] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END
GO

-- Create Usuarios table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [UserName] nvarchar(100) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [IdProfesor] int NULL,
        [IdEstudiante] int NULL,
        [Email] nvarchar(255) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Estudiantes_IdEstudiante] FOREIGN KEY ([IdEstudiante]) REFERENCES [Estudiantes] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Usuarios_Profesores_IdProfesor] FOREIGN KEY ([IdProfesor]) REFERENCES [Profesores] ([Id]) ON DELETE SET NULL
    );
END
GO

-- Create UsuarioRoles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsuarioRoles')
BEGIN
    CREATE TABLE [UsuarioRoles] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [IdUsuario] int NOT NULL,
        [IdRol] int NOT NULL,
        [AsignadoEn] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [ValidoHasta] datetime2 NULL,
        [AsignadoPor] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_UsuarioRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UsuarioRoles_Roles_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UsuarioRoles_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create unique indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Roles_Nombre')
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [Roles] ([Nombre]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_UserName')
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_UserName] ON [Usuarios] ([UserName]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_Email')
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]) WHERE [Email] IS NOT NULL;
END
GO

-- Create foreign key indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_IdEstudiante')
BEGIN
    CREATE INDEX [IX_Usuarios_IdEstudiante] ON [Usuarios] ([IdEstudiante]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_IdProfesor')
BEGIN
    CREATE INDEX [IX_Usuarios_IdProfesor] ON [Usuarios] ([IdProfesor]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsuarioRoles_IdRol')
BEGIN
    CREATE INDEX [IX_UsuarioRoles_IdRol] ON [UsuarioRoles] ([IdRol]);
END
GO

-- Create unique composite index for UsuarioRoles (prevents duplicate active role assignments)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsuarioRoles_IdUsuario_IdRol')
BEGIN
    CREATE UNIQUE INDEX [IX_UsuarioRoles_IdUsuario_IdRol] ON [UsuarioRoles] ([IdUsuario], [IdRol]) WHERE [IsDeleted] = 0;
END
GO

-- Insert initial data (Roles)
PRINT 'Inserting initial roles...';

IF NOT EXISTS (SELECT * FROM [Roles] WHERE [Nombre] = 'Admin')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [IsActive], [CreatedAt], [CreatedBy])
    VALUES ('Admin', 'Administrador del sistema con acceso completo', 1, GETUTCDATE(), 'System');
    PRINT 'Role Admin created';
END

IF NOT EXISTS (SELECT * FROM [Roles] WHERE [Nombre] = 'Profesor')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [IsActive], [CreatedAt], [CreatedBy])
    VALUES ('Profesor', 'Profesor con acceso a gestión de estudiantes y notas', 1, GETUTCDATE(), 'System');
    PRINT 'Role Profesor created';
END

IF NOT EXISTS (SELECT * FROM [Roles] WHERE [Nombre] = 'Estudiante')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [IsActive], [CreatedAt], [CreatedBy])
    VALUES ('Estudiante', 'Estudiante con acceso limitado a consulta de notas', 1, GETUTCDATE(), 'System');
    PRINT 'Role Estudiante created';
END

-- Insert admin user (password hash for "Admin123!")
DECLARE @AdminPasswordHash NVARCHAR(500) = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg='; -- SHA256 hash of "Admin123!" + salt

IF NOT EXISTS (SELECT * FROM [Usuarios] WHERE [UserName] = 'admin')
BEGIN
    INSERT INTO [Usuarios] ([UserName], [Email], [PasswordHash], [IsActive], [CreatedAt], [CreatedBy])
    VALUES ('admin', 'admin@audisoft.com', @AdminPasswordHash, 1, GETUTCDATE(), 'System');
    PRINT 'Admin user created';
    
    -- Assign Admin role to admin user
    DECLARE @AdminUserId INT = SCOPE_IDENTITY();
    DECLARE @AdminRoleId INT = (SELECT [Id] FROM [Roles] WHERE [Nombre] = 'Admin');
    
    INSERT INTO [UsuarioRoles] ([IdUsuario], [IdRol], [AsignadoEn], [AsignadoPor], [CreatedAt], [CreatedBy])
    VALUES (@AdminUserId, @AdminRoleId, GETUTCDATE(), 'System', GETUTCDATE(), 'System');
    PRINT 'Admin role assigned to admin user';
END

-- Create sample professor users
PRINT 'Creating sample professor users...';

DECLARE @ProfPasswordHash NVARCHAR(500) = 'tHh4cOmQOGi4C6j8kfwMJhJmhj6yoJhIfvmXD2MfzGE='; -- SHA256 hash of "Profesor123!" + salt
DECLARE @ProfesorRoleId INT = (SELECT [Id] FROM [Roles] WHERE [Nombre] = 'Profesor');

-- Get existing professors
DECLARE @ProfesoresCursor CURSOR;
DECLARE @ProfesorId INT, @ProfesorNombre NVARCHAR(255);

SET @ProfesoresCursor = CURSOR FOR
    SELECT [Id], [Nombre] FROM [Profesores] WHERE [IsDeleted] = 0;

OPEN @ProfesoresCursor;
FETCH NEXT FROM @ProfesoresCursor INTO @ProfesorId, @ProfesorNombre;

DECLARE @UserCounter INT = 1;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ProfUserName NVARCHAR(100) = 'prof' + CAST(@UserCounter AS NVARCHAR(10));
    DECLARE @ProfEmail NVARCHAR(255) = @ProfUserName + '@audisoft.com';
    
    IF NOT EXISTS (SELECT * FROM [Usuarios] WHERE [UserName] = @ProfUserName)
    BEGIN
        INSERT INTO [Usuarios] ([UserName], [Email], [PasswordHash], [IdProfesor], [IsActive], [CreatedAt], [CreatedBy])
        VALUES (@ProfUserName, @ProfEmail, @ProfPasswordHash, @ProfesorId, 1, GETUTCDATE(), 'System');
        
        DECLARE @ProfUserId INT = SCOPE_IDENTITY();
        
        INSERT INTO [UsuarioRoles] ([IdUsuario], [IdRol], [AsignadoEn], [AsignadoPor], [CreatedAt], [CreatedBy])
        VALUES (@ProfUserId, @ProfesorRoleId, GETUTCDATE(), 'System', GETUTCDATE(), 'System');
        
        PRINT 'Professor user created: ' + @ProfUserName;
    END
    
    SET @UserCounter = @UserCounter + 1;
    FETCH NEXT FROM @ProfesoresCursor INTO @ProfesorId, @ProfesorNombre;
END

CLOSE @ProfesoresCursor;
DEALLOCATE @ProfesoresCursor;

-- Create sample student users
PRINT 'Creating sample student users...';

DECLARE @EstPasswordHash NVARCHAR(500) = 'K8+hxKdE1ptBxaBLjXg+K3vQbGJLhz0gzfPhkGxBzVs='; -- SHA256 hash of "Estudiante123!" + salt
DECLARE @EstudianteRoleId INT = (SELECT [Id] FROM [Roles] WHERE [Nombre] = 'Estudiante');

-- Get existing students
DECLARE @EstudiantesCursor CURSOR;
DECLARE @EstudianteId INT, @EstudianteNombre NVARCHAR(255);

SET @EstudiantesCursor = CURSOR FOR
    SELECT [Id], [Nombre] FROM [Estudiantes] WHERE [IsDeleted] = 0;

OPEN @EstudiantesCursor;
FETCH NEXT FROM @EstudiantesCursor INTO @EstudianteId, @EstudianteNombre;

SET @UserCounter = 1;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @EstUserName NVARCHAR(100) = 'est' + CAST(@UserCounter AS NVARCHAR(10));
    DECLARE @EstEmail NVARCHAR(255) = @EstUserName + '@estudiante.audisoft.com';
    
    IF NOT EXISTS (SELECT * FROM [Usuarios] WHERE [UserName] = @EstUserName)
    BEGIN
        INSERT INTO [Usuarios] ([UserName], [Email], [PasswordHash], [IdEstudiante], [IsActive], [CreatedAt], [CreatedBy])
        VALUES (@EstUserName, @EstEmail, @EstPasswordHash, @EstudianteId, 1, GETUTCDATE(), 'System');
        
        DECLARE @EstUserId INT = SCOPE_IDENTITY();
        
        INSERT INTO [UsuarioRoles] ([IdUsuario], [IdRol], [AsignadoEn], [AsignadoPor], [CreatedAt], [CreatedBy])
        VALUES (@EstUserId, @EstudianteRoleId, GETUTCDATE(), 'System', GETUTCDATE(), 'System');
        
        PRINT 'Student user created: ' + @EstUserName;
    END
    
    SET @UserCounter = @UserCounter + 1;
    FETCH NEXT FROM @EstudiantesCursor INTO @EstudianteId, @EstudianteNombre;
END

CLOSE @EstudiantesCursor;
DEALLOCATE @EstudiantesCursor;

-- Register migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251114022538_AddUserRoleEntities')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251114022538_AddUserRoleEntities', '8.0.0');
END
GO

PRINT 'User/Role migration applied successfully!';
PRINT 'Default credentials:';
PRINT '  Admin: admin / Admin123!';
PRINT '  Professors: prof1, prof2, prof3 / Profesor123!';
PRINT '  Students: est1, est2, est3, est4 / Estudiante123!';