-- =============================================
-- Theme Editor: Create Themes Table
-- Version: 1.0
-- Date: 2025-12-13
-- =============================================

-- Create the Themes table for storing FormTheme configurations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Themes')
BEGIN
    CREATE TABLE [dbo].[Themes] (
        [Id]              NVARCHAR(50)    NOT NULL PRIMARY KEY,
        [Name]            NVARCHAR(100)   NOT NULL,
        [Description]     NVARCHAR(500)   NULL,
        [BasePreset]      NVARCHAR(50)    NULL,
        [ThemeJson]       NVARCHAR(MAX)   NOT NULL,
        [PreviewColor]    NVARCHAR(20)    NULL,  -- Primary color for quick preview
        [Mode]            NVARCHAR(10)    NOT NULL DEFAULT 'Light',  -- Light, Dark, Auto
        [IsDefault]       BIT             NOT NULL DEFAULT 0,
        [IsLocked]        BIT             NOT NULL DEFAULT 0,
        [IsActive]        BIT             NOT NULL DEFAULT 1,
        [OrganizationId]  NVARCHAR(50)    NULL,  -- For multi-tenant support
        [CreatedBy]       NVARCHAR(100)   NULL,
        [CreatedAt]       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedAt]      DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        [Version]         INT             NOT NULL DEFAULT 1,

        -- Indexes for common queries
        INDEX [IX_Themes_Name] NONCLUSTERED ([Name]),
        INDEX [IX_Themes_IsDefault] NONCLUSTERED ([IsDefault]) WHERE [IsActive] = 1,
        INDEX [IX_Themes_OrganizationId] NONCLUSTERED ([OrganizationId]) WHERE [IsActive] = 1,
        INDEX [IX_Themes_Mode] NONCLUSTERED ([Mode]) WHERE [IsActive] = 1
    );

    PRINT 'Created Themes table';
END
ELSE
BEGIN
    PRINT 'Themes table already exists';
END
GO

-- Create trigger to update ModifiedAt on update
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Themes_UpdateModifiedAt')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TR_Themes_UpdateModifiedAt]
    ON [dbo].[Themes]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE [dbo].[Themes]
        SET [ModifiedAt] = GETUTCDATE()
        FROM [dbo].[Themes] t
        INNER JOIN inserted i ON t.[Id] = i.[Id];
    END
    ');
    PRINT 'Created TR_Themes_UpdateModifiedAt trigger';
END
GO

-- Ensure only one default theme per organization
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Themes_DefaultPerOrg')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Themes_DefaultPerOrg]
    ON [dbo].[Themes] ([OrganizationId], [IsDefault])
    WHERE [IsDefault] = 1 AND [IsActive] = 1;

    PRINT 'Created unique index for default theme per organization';
END
GO
