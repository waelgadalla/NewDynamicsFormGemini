-- ============================================
-- DynamicFormsEditorDB - Database Creation Script
-- Target: (localdb)\MSSQLLocalDB
-- ============================================

-- Create the database
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DynamicFormsEditorDB')
BEGIN
    ALTER DATABASE [DynamicFormsEditorDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DynamicFormsEditorDB];
END
GO

CREATE DATABASE [DynamicFormsEditorDB];
GO

USE [DynamicFormsEditorDB];
GO

-- ============================================
-- Table: ModuleSchemas
-- Stores FormModuleSchema as JSON
-- ============================================
CREATE TABLE [dbo].[ModuleSchemas] (
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [ModuleId]      INT NOT NULL,
    [Version]       FLOAT NOT NULL DEFAULT 1.0,
    [SchemaJson]    NVARCHAR(MAX) NOT NULL,
    [TitleEn]       NVARCHAR(200) NULL,
    [TitleFr]       NVARCHAR(200) NULL,
    [DescriptionEn] NVARCHAR(500) NULL,
    [IsActive]      BIT NOT NULL DEFAULT 1,
    [IsCurrent]     BIT NOT NULL DEFAULT 1,
    [DateCreated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DateUpdated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy]     NVARCHAR(100) NULL,
    [UpdatedBy]     NVARCHAR(100) NULL,

    CONSTRAINT [PK_ModuleSchemas] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Indexes for ModuleSchemas
CREATE UNIQUE NONCLUSTERED INDEX [IX_ModuleSchemas_ModuleId_Current]
    ON [dbo].[ModuleSchemas] ([ModuleId] ASC)
    WHERE [IsActive] = 1 AND [IsCurrent] = 1;

CREATE NONCLUSTERED INDEX [IX_ModuleSchemas_ModuleId]
    ON [dbo].[ModuleSchemas] ([ModuleId] ASC);

CREATE NONCLUSTERED INDEX [IX_ModuleSchemas_DateUpdated]
    ON [dbo].[ModuleSchemas] ([DateUpdated] DESC);
GO

-- ============================================
-- Table: WorkflowSchemas
-- Stores FormWorkflowSchema as JSON
-- ============================================
CREATE TABLE [dbo].[WorkflowSchemas] (
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [WorkflowId]    INT NOT NULL,
    [Version]       FLOAT NOT NULL DEFAULT 1.0,
    [SchemaJson]    NVARCHAR(MAX) NOT NULL,
    [TitleEn]       NVARCHAR(200) NULL,
    [TitleFr]       NVARCHAR(200) NULL,
    [DescriptionEn] NVARCHAR(500) NULL,
    [IsActive]      BIT NOT NULL DEFAULT 1,
    [IsCurrent]     BIT NOT NULL DEFAULT 1,
    [DateCreated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DateUpdated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy]     NVARCHAR(100) NULL,
    [UpdatedBy]     NVARCHAR(100) NULL,

    CONSTRAINT [PK_WorkflowSchemas] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Indexes for WorkflowSchemas
CREATE UNIQUE NONCLUSTERED INDEX [IX_WorkflowSchemas_WorkflowId_Current]
    ON [dbo].[WorkflowSchemas] ([WorkflowId] ASC)
    WHERE [IsActive] = 1 AND [IsCurrent] = 1;

CREATE NONCLUSTERED INDEX [IX_WorkflowSchemas_WorkflowId]
    ON [dbo].[WorkflowSchemas] ([WorkflowId] ASC);

CREATE NONCLUSTERED INDEX [IX_WorkflowSchemas_DateUpdated]
    ON [dbo].[WorkflowSchemas] ([DateUpdated] DESC);
GO

-- ============================================
-- Table: CodeSets (for future use)
-- Stores ManagedCodeSet definitions
-- ============================================
CREATE TABLE [dbo].[CodeSets] (
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [Code]          NVARCHAR(50) NOT NULL,
    [NameEn]        NVARCHAR(200) NOT NULL,
    [NameFr]        NVARCHAR(200) NULL,
    [DescriptionEn] NVARCHAR(500) NULL,
    [DescriptionFr] NVARCHAR(500) NULL,
    [Category]      NVARCHAR(100) NULL,
    [SchemaJson]    NVARCHAR(MAX) NOT NULL,
    [IsActive]      BIT NOT NULL DEFAULT 1,
    [Version]       FLOAT NOT NULL DEFAULT 1.0,
    [DateCreated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DateUpdated]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy]     NVARCHAR(100) NULL,
    [UpdatedBy]     NVARCHAR(100) NULL,

    CONSTRAINT [PK_CodeSets] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_CodeSets_Code]
    ON [dbo].[CodeSets] ([Code] ASC)
    WHERE [IsActive] = 1;
GO

PRINT 'Database DynamicFormsEditorDB created successfully.';
GO
