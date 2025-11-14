-- =============================================
-- Script para arreglar schema de UsuarioRoles
-- Descripción: Añade las columnas faltantes a la tabla UsuarioRoles
-- Fecha: 2025-11-14
-- =============================================

USE AudiSoftSchoolDb;
GO

-- Verificar y añadir columnas faltantes a UsuarioRoles
-- Estas columnas son requeridas por BaseEntity en el modelo de EF Core

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Columna IsDeleted añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna IsDeleted ya existe en UsuarioRoles.';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'DeletedAt')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD DeletedAt DATETIME2 NULL;
    PRINT 'Columna DeletedAt añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna DeletedAt ya existe en UsuarioRoles.';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Columna UpdatedAt añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna UpdatedAt ya existe en UsuarioRoles.';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'UpdatedBy')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD UpdatedBy NVARCHAR(100) NULL;
    PRINT 'Columna UpdatedBy añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna UpdatedBy ya existe en UsuarioRoles.';
END
GO

-- Verificar que AsignadoEn existe (debe haber sido creada por el script 02_CreateTables_And_Seed.sql)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'AsignadoEn')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD AsignadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Columna AsignadoEn añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna AsignadoEn ya existe en UsuarioRoles.';
END
GO

-- Verificar que ValidoHasta existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'ValidoHasta')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD ValidoHasta DATETIME2 NULL;
    PRINT 'Columna ValidoHasta añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna ValidoHasta ya existe en UsuarioRoles.';
END
GO

-- Verificar que AsignadoPor existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = 'UsuarioRoles' AND COLUMN_NAME = 'AsignadoPor')
BEGIN
    ALTER TABLE UsuarioRoles
    ADD AsignadoPor NVARCHAR(100) NULL;
    PRINT 'Columna AsignadoPor añadida a UsuarioRoles.';
END
ELSE
BEGIN
    PRINT 'Columna AsignadoPor ya existe en UsuarioRoles.';
END
GO

-- Verificar todas las columnas de UsuarioRoles
PRINT '';
PRINT 'ESQUEMA ACTUAL DE UsuarioRoles:';
PRINT '============================================';
SELECT 
    COLUMN_NAME as 'Columna',
    DATA_TYPE as 'Tipo',
    IS_NULLABLE as 'Nullable'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UsuarioRoles'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'REGISTROS EN UsuarioRoles:';
SELECT COUNT(*) as Total FROM UsuarioRoles;

PRINT '';
PRINT '============================================';
PRINT 'ESQUEMA CORREGIDO EXITOSAMENTE';
PRINT '============================================';
