# Role-Based Access Control for Visual Editor Opus
# Part 3: Database Schema

**Document Version:** 2.0
**Date:** December 2025
**Database:** SQL Server 2019+

---

## 1. Schema Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         IDENTITY TABLES                             │
│  AspNetUsers, AspNetRoles, AspNetUserRoles (Standard Identity)      │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      ORGANIZATION TABLES                            │
│  Organizations, OrganizationUsers, Workspaces, WorkspaceMembers     │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      PERMISSION TABLES                              │
│  RoleTemplates, RolePermissions, FormPermissions                    │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         AUDIT TABLES                                │
│  AuditLog, FormVersionHistory                                       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Identity Extension Tables

### 2.1 Extended User Profile

```sql
-- Extends AspNetUsers with application-specific fields
CREATE TABLE [dbo].[UserProfiles] (
    [UserId]            NVARCHAR(450)   NOT NULL,  -- FK to AspNetUsers
    [DisplayName]       NVARCHAR(256)   NOT NULL,
    [AvatarUrl]         NVARCHAR(500)   NULL,
    [TimeZone]          NVARCHAR(100)   NOT NULL DEFAULT 'UTC',
    [PreferredLanguage] NVARCHAR(10)    NOT NULL DEFAULT 'en',
    [MfaEnabled]        BIT             NOT NULL DEFAULT 0,
    [LastLoginAt]       DATETIME2       NULL,
    [LastLoginIp]       NVARCHAR(45)    NULL,
    [PasswordChangedAt] DATETIME2       NULL,
    [MustChangePassword] BIT            NOT NULL DEFAULT 0,
    [IsActive]          BIT             NOT NULL DEFAULT 1,
    [DeactivatedAt]     DATETIME2       NULL,
    [DeactivatedBy]     NVARCHAR(450)   NULL,
    [CreatedAt]         DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_UserProfiles_AspNetUsers] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_UserProfiles_IsActive] ON [dbo].[UserProfiles]([IsActive]);
```

---

## 3. Organization & Workspace Tables

### 3.1 Organizations

```sql
CREATE TABLE [dbo].[Organizations] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(256)    NOT NULL,
    [Slug]              NVARCHAR(100)    NOT NULL,  -- URL-friendly identifier
    [Description]       NVARCHAR(1000)   NULL,
    [LogoUrl]           NVARCHAR(500)    NULL,
    [IsActive]          BIT              NOT NULL DEFAULT 1,
    [Settings]          NVARCHAR(MAX)    NULL,      -- JSON for org-specific settings
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         NVARCHAR(450)    NOT NULL,
    [UpdatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedBy]         NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Organizations_Slug] UNIQUE ([Slug])
);

CREATE INDEX [IX_Organizations_IsActive] ON [dbo].[Organizations]([IsActive]);
CREATE INDEX [IX_Organizations_Slug] ON [dbo].[Organizations]([Slug]);
```

### 3.2 Organization Users

```sql
-- Maps users to organizations with their org-level role
CREATE TABLE [dbo].[OrganizationUsers] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [UserId]            NVARCHAR(450)    NOT NULL,
    [Role]              TINYINT          NOT NULL,  -- 0=Member, 1=Admin
    [JoinedAt]          DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [InvitedBy]         NVARCHAR(450)    NULL,
    [IsActive]          BIT              NOT NULL DEFAULT 1,

    CONSTRAINT [PK_OrganizationUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrganizationUsers_Organization] FOREIGN KEY ([OrganizationId])
        REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrganizationUsers_User] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_OrganizationUsers_OrgUser] UNIQUE ([OrganizationId], [UserId])
);

CREATE INDEX [IX_OrganizationUsers_UserId] ON [dbo].[OrganizationUsers]([UserId]);
CREATE INDEX [IX_OrganizationUsers_OrgId_Active] ON [dbo].[OrganizationUsers]([OrganizationId], [IsActive]);
```

### 3.3 Workspaces

```sql
CREATE TABLE [dbo].[Workspaces] (
    [Id]                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]        UNIQUEIDENTIFIER NOT NULL,
    [Name]                  NVARCHAR(256)    NOT NULL,
    [Description]           NVARCHAR(1000)   NULL,
    [IconUrl]               NVARCHAR(500)    NULL,
    [IsActive]              BIT              NOT NULL DEFAULT 1,
    [IsPrivate]             BIT              NOT NULL DEFAULT 0,  -- Visible only to members
    [DefaultPermissionLevel] TINYINT         NOT NULL DEFAULT 20, -- ViewData
    [Settings]              NVARCHAR(MAX)    NULL,  -- JSON for workspace settings
    [CreatedAt]             DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy]             NVARCHAR(450)    NOT NULL,
    [UpdatedAt]             DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Workspaces] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workspaces_Organization] FOREIGN KEY ([OrganizationId])
        REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Workspaces_OrgId_Active] ON [dbo].[Workspaces]([OrganizationId], [IsActive]);
```

### 3.4 Workspace Members

```sql
-- Maps users to workspaces with their workspace role
CREATE TABLE [dbo].[WorkspaceMembers] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [WorkspaceId]       UNIQUEIDENTIFIER NOT NULL,
    [UserId]            NVARCHAR(450)    NOT NULL,
    [RoleTemplateId]    UNIQUEIDENTIFIER NULL,       -- Optional: use predefined role
    [CustomPermissions] NVARCHAR(MAX)    NULL,       -- JSON: override specific permissions
    [AddedAt]           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [AddedBy]           NVARCHAR(450)    NOT NULL,
    [IsOwner]           BIT              NOT NULL DEFAULT 0,

    CONSTRAINT [PK_WorkspaceMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkspaceMembers_Workspace] FOREIGN KEY ([WorkspaceId])
        REFERENCES [dbo].[Workspaces]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkspaceMembers_User] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkspaceMembers_RoleTemplate] FOREIGN KEY ([RoleTemplateId])
        REFERENCES [dbo].[RoleTemplates]([Id]),
    CONSTRAINT [UQ_WorkspaceMembers_WsUser] UNIQUE ([WorkspaceId], [UserId])
);

CREATE INDEX [IX_WorkspaceMembers_UserId] ON [dbo].[WorkspaceMembers]([UserId]);
CREATE INDEX [IX_WorkspaceMembers_WsId_Owner] ON [dbo].[WorkspaceMembers]([WorkspaceId], [IsOwner]);
```

---

## 4. Permission Tables

### 4.1 Role Templates

```sql
-- Predefined role templates that can be assigned to workspace members
CREATE TABLE [dbo].[RoleTemplates] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrganizationId]    UNIQUEIDENTIFIER NULL,       -- NULL = system-wide template
    [Name]              NVARCHAR(100)    NOT NULL,
    [Description]       NVARCHAR(500)    NULL,
    [IsSystemRole]      BIT              NOT NULL DEFAULT 0,  -- Cannot be deleted
    [Permissions]       NVARCHAR(MAX)    NOT NULL,   -- JSON array of permission keys
    [SurveyJsConfig]    NVARCHAR(MAX)    NULL,       -- JSON for SurveyJS Creator config
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_RoleTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleTemplates_Organization] FOREIGN KEY ([OrganizationId])
        REFERENCES [dbo].[Organizations]([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_RoleTemplates_OrgId] ON [dbo].[RoleTemplates]([OrganizationId]);
```

### 4.2 Form Permissions

```sql
-- Grants access to specific forms for users or roles
CREATE TABLE [dbo].[FormPermissions] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [FormId]            NVARCHAR(450)    NOT NULL,   -- FK to FormModuleSchema
    [PrincipalId]       NVARCHAR(450)    NOT NULL,   -- UserId, RoleTemplateId, or "workspace:{id}"
    [PrincipalType]     TINYINT          NOT NULL,   -- 0=User, 1=RoleTemplate, 2=Workspace
    [PermissionLevel]   TINYINT          NOT NULL,   -- See permission levels in doc 02
    [CustomPermissions] NVARCHAR(MAX)    NULL,       -- JSON for fine-grained overrides
    [GrantedBy]         NVARCHAR(450)    NOT NULL,
    [GrantedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [ExpiresAt]         DATETIME2        NULL,       -- NULL = never expires
    [Reason]            NVARCHAR(500)    NULL,       -- Why was this granted

    CONSTRAINT [PK_FormPermissions] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_FormPermissions_FormPrincipal] UNIQUE ([FormId], [PrincipalId], [PrincipalType])
);

CREATE INDEX [IX_FormPermissions_FormId] ON [dbo].[FormPermissions]([FormId]);
CREATE INDEX [IX_FormPermissions_PrincipalId] ON [dbo].[FormPermissions]([PrincipalId]);
CREATE INDEX [IX_FormPermissions_ExpiresAt] ON [dbo].[FormPermissions]([ExpiresAt]) WHERE [ExpiresAt] IS NOT NULL;
```

---

## 5. Audit Tables

### 5.1 Audit Log

```sql
CREATE TABLE [dbo].[AuditLog] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [Timestamp]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [UserId]            NVARCHAR(450)    NULL,       -- NULL for system actions
    [UserEmail]         NVARCHAR(256)    NULL,       -- Denormalized for query performance
    [IpAddress]         NVARCHAR(45)     NULL,
    [UserAgent]         NVARCHAR(500)    NULL,
    [Action]            NVARCHAR(100)    NOT NULL,   -- e.g., "form.created", "user.login"
    [EntityType]        NVARCHAR(100)    NULL,       -- e.g., "Form", "User", "Workspace"
    [EntityId]          NVARCHAR(450)    NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [WorkspaceId]       UNIQUEIDENTIFIER NULL,
    [OldValues]         NVARCHAR(MAX)    NULL,       -- JSON of previous state
    [NewValues]         NVARCHAR(MAX)    NULL,       -- JSON of new state
    [Metadata]          NVARCHAR(MAX)    NULL,       -- Additional context as JSON
    [Severity]          TINYINT          NOT NULL DEFAULT 0,  -- 0=Info, 1=Warning, 2=Error, 3=Critical

    CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([Id])
);

-- Partition-friendly indexes for time-based queries
CREATE INDEX [IX_AuditLog_Timestamp] ON [dbo].[AuditLog]([Timestamp] DESC);
CREATE INDEX [IX_AuditLog_UserId_Timestamp] ON [dbo].[AuditLog]([UserId], [Timestamp] DESC);
CREATE INDEX [IX_AuditLog_EntityType_EntityId] ON [dbo].[AuditLog]([EntityType], [EntityId]);
CREATE INDEX [IX_AuditLog_OrgId_Timestamp] ON [dbo].[AuditLog]([OrganizationId], [Timestamp] DESC);
CREATE INDEX [IX_AuditLog_Action] ON [dbo].[AuditLog]([Action]);
```

### 5.2 Form Version History

```sql
-- Stores snapshots of form JSON for version control
CREATE TABLE [dbo].[FormVersionHistory] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [FormId]            NVARCHAR(450)    NOT NULL,
    [Version]           INT              NOT NULL,
    [SchemaJson]        NVARCHAR(MAX)    NOT NULL,   -- Complete form JSON at this version
    [ChangeDescription] NVARCHAR(500)    NULL,
    [ChangedBy]         NVARCHAR(450)    NOT NULL,
    [ChangedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [IsMajorVersion]    BIT              NOT NULL DEFAULT 0,  -- Explicit save vs auto-save

    CONSTRAINT [PK_FormVersionHistory] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_FormVersionHistory_FormVersion] UNIQUE ([FormId], [Version])
);

CREATE INDEX [IX_FormVersionHistory_FormId_Version] ON [dbo].[FormVersionHistory]([FormId], [Version] DESC);
```

---

## 6. Extend Existing Tables

### 6.1 FormModuleSchema Extensions

```sql
-- Add columns to existing FormModuleSchema table
ALTER TABLE [dbo].[FormModuleSchema]
ADD
    [WorkspaceId]       UNIQUEIDENTIFIER NULL,
    [CreatedBy]         NVARCHAR(450)    NULL,
    [CreatedAt]         DATETIME2        NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedBy]         NVARCHAR(450)    NULL,
    [UpdatedAt]         DATETIME2        NULL DEFAULT SYSUTCDATETIME(),
    [CurrentVersion]    INT              NOT NULL DEFAULT 1,
    [Status]            TINYINT          NOT NULL DEFAULT 0,  -- 0=Draft, 1=Published, 2=Archived
    [IsTemplate]        BIT              NOT NULL DEFAULT 0;

-- Add foreign key and indexes
ALTER TABLE [dbo].[FormModuleSchema]
ADD CONSTRAINT [FK_FormModuleSchema_Workspace] FOREIGN KEY ([WorkspaceId])
    REFERENCES [dbo].[Workspaces]([Id]);

CREATE INDEX [IX_FormModuleSchema_WorkspaceId] ON [dbo].[FormModuleSchema]([WorkspaceId]);
CREATE INDEX [IX_FormModuleSchema_CreatedBy] ON [dbo].[FormModuleSchema]([CreatedBy]);
CREATE INDEX [IX_FormModuleSchema_Status] ON [dbo].[FormModuleSchema]([Status]);
```

---

## 7. Invitations Table

```sql
CREATE TABLE [dbo].[Invitations] (
    [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Email]             NVARCHAR(256)    NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [WorkspaceId]       UNIQUEIDENTIFIER NULL,       -- Optional: direct workspace invite
    [RoleTemplateId]    UNIQUEIDENTIFIER NULL,
    [InvitedBy]         NVARCHAR(450)    NOT NULL,
    [Token]             NVARCHAR(100)    NOT NULL,   -- Secure random token
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [ExpiresAt]         DATETIME2        NOT NULL,
    [AcceptedAt]        DATETIME2        NULL,
    [AcceptedByUserId]  NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Invitations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invitations_Organization] FOREIGN KEY ([OrganizationId])
        REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Invitations_Workspace] FOREIGN KEY ([WorkspaceId])
        REFERENCES [dbo].[Workspaces]([Id]),
    CONSTRAINT [UQ_Invitations_Token] UNIQUE ([Token])
);

CREATE INDEX [IX_Invitations_Email] ON [dbo].[Invitations]([Email]);
CREATE INDEX [IX_Invitations_Token] ON [dbo].[Invitations]([Token]);
CREATE INDEX [IX_Invitations_ExpiresAt] ON [dbo].[Invitations]([ExpiresAt]);
```

---

## 8. Session Management

```sql
CREATE TABLE [dbo].[UserSessions] (
    [Id]                NVARCHAR(100)    NOT NULL,   -- Session token
    [UserId]            NVARCHAR(450)    NOT NULL,
    [DeviceInfo]        NVARCHAR(500)    NULL,
    [IpAddress]         NVARCHAR(45)     NULL,
    [CreatedAt]         DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [LastActivityAt]    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [ExpiresAt]         DATETIME2        NOT NULL,
    [IsRevoked]         BIT              NOT NULL DEFAULT 0,
    [RevokedAt]         DATETIME2        NULL,
    [RevokedReason]     NVARCHAR(200)    NULL,

    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSessions_User] FOREIGN KEY ([UserId])
        REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_UserSessions_UserId] ON [dbo].[UserSessions]([UserId]);
CREATE INDEX [IX_UserSessions_ExpiresAt] ON [dbo].[UserSessions]([ExpiresAt]);
```

---

## 9. Seed Data

### 9.1 System Role Templates

```sql
-- Insert default system-wide role templates
INSERT INTO [dbo].[RoleTemplates] ([Id], [OrganizationId], [Name], [Description], [IsSystemRole], [Permissions], [SurveyJsConfig])
VALUES
    (NEWID(), NULL, 'Workspace Owner', 'Full control over workspace and all forms', 1,
     '["workspace.*", "form.*", "data.*"]',
     '{"showJSONEditorTab": true, "allowChangeType": true}'),

    (NEWID(), NULL, 'Form Designer', 'Creates and edits forms, no data access', 1,
     '["form.create", "form.edit_structure", "form.edit_text", "form.edit_logic", "form.edit_validation", "form.edit_theme", "form.delete", "form.publish", "form.view_design", "form.duplicate"]',
     '{"showJSONEditorTab": false, "hiddenToolboxItems": ["html"]}'),

    (NEWID(), NULL, 'Data Manager', 'Manages submissions, read-only form access', 1,
     '["form.view_design", "data.view_submissions", "data.export_submissions", "data.edit_submissions", "data.delete_submissions", "data.view_analytics"]',
     '{"readOnly": true}'),

    (NEWID(), NULL, 'Reviewer', 'Read-only access to forms and data', 1,
     '["form.view_design", "data.view_submissions", "data.view_analytics"]',
     '{"readOnly": true}');
```

---

## 10. Migration Strategy

### 10.1 Migration Order

1. Create Identity tables (ASP.NET Identity migration)
2. Create `Organizations` and `Workspaces`
3. Create `RoleTemplates` with seed data
4. Create `OrganizationUsers` and `WorkspaceMembers`
5. Create `FormPermissions`
6. Extend `FormModuleSchema` with new columns
7. Create `AuditLog` and `FormVersionHistory`
8. Create `Invitations` and `UserSessions`
9. Migrate existing forms to default workspace

### 10.2 Data Migration Script

```sql
-- Migrate existing forms to a default workspace
DECLARE @DefaultOrgId UNIQUEIDENTIFIER = NEWID();
DECLARE @DefaultWorkspaceId UNIQUEIDENTIFIER = NEWID();
DECLARE @SystemUserId NVARCHAR(450) = 'system';

-- Create default organization
INSERT INTO [dbo].[Organizations] ([Id], [Name], [Slug], [CreatedBy])
VALUES (@DefaultOrgId, 'Default Organization', 'default', @SystemUserId);

-- Create default workspace
INSERT INTO [dbo].[Workspaces] ([Id], [OrganizationId], [Name], [CreatedBy])
VALUES (@DefaultWorkspaceId, @DefaultOrgId, 'Default Workspace', @SystemUserId);

-- Assign existing forms to default workspace
UPDATE [dbo].[FormModuleSchema]
SET [WorkspaceId] = @DefaultWorkspaceId,
    [CreatedBy] = @SystemUserId,
    [CreatedAt] = SYSUTCDATETIME()
WHERE [WorkspaceId] IS NULL;
```

---

## Next Document

Proceed to **04-Technical-Architecture.md** for service layer design and server-side validation patterns.
