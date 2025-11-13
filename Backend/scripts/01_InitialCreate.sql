-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AudiSoftSchool')
BEGIN
    CREATE DATABASE AudiSoftSchool;
END
GO

USE AudiSoftSchool;
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

-- Create Estudiantes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Estudiantes')
BEGIN
    CREATE TABLE [Estudiantes] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [Nombre] nvarchar(255) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Estudiantes] PRIMARY KEY ([Id])
    );
END
GO

-- Create Profesores table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Profesores')
BEGIN
    CREATE TABLE [Profesores] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [Nombre] nvarchar(255) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Profesores] PRIMARY KEY ([Id])
    );
END
GO

-- Create Notas table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notas')
BEGIN
    CREATE TABLE [Notas] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [Nombre] nvarchar(255) NOT NULL,
        [Valor] decimal(5,2) NOT NULL,
        [IdProfesor] int NOT NULL,
        [IdEstudiante] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Notas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notas_Profesores_IdProfesor] FOREIGN KEY ([IdProfesor]) REFERENCES [Profesores] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notas_Estudiantes_IdEstudiante] FOREIGN KEY ([IdEstudiante]) REFERENCES [Estudiantes] ([Id]) ON DELETE NO ACTION
    );
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notas_IdProfesor')
BEGIN
    CREATE INDEX [IX_Notas_IdProfesor] ON [Notas] ([IdProfesor]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notas_IdEstudiante')
BEGIN
    CREATE INDEX [IX_Notas_IdEstudiante] ON [Notas] ([IdEstudiante]);
END
GO

-- Register migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251112_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251112_InitialCreate', '10.0.0');
END
GO

PRINT 'Migration applied successfully!';
