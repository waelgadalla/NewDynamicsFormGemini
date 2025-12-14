# Role-Based Access Control for Visual Editor Opus
# Part 7: AI Implementation Prompts

**Document Version:** 2.0
**Date:** December 2025
**Purpose:** Ready-to-use prompts for AI-assisted implementation

---

## How to Use This Document

Each section contains a detailed prompt you can copy and use with an AI coding assistant (Claude, Copilot, etc.). The prompts reference the other documents in this folder for context.

**Best Practice:** Before using a prompt, ask the AI to read the relevant documentation files first.

---

## Phase 1 Prompts

### 1.1 Setup ASP.NET Core Identity

```text
I need to add ASP.NET Core Identity to my existing Blazor Server project.

Current state:
- Project: VisualEditorOpus (Blazor Server, .NET 9)
- Database: SQL Server, connection string in appsettings.json as "DefaultConnection"
- Current Program.cs has NO authentication configured

Tasks:
1. Create `ApplicationDbContext` that inherits from `IdentityDbContext<ApplicationUser>`
2. Create `ApplicationUser` class that extends `IdentityUser` (empty for now, will extend later)
3. Update Program.cs to:
   - Register ApplicationDbContext with SQL Server
   - Add Identity with these password rules:
     - Minimum 12 characters
     - Require digit, lowercase, uppercase, special character
   - Configure lockout: 5 attempts, 15 minute lockout
   - Add authentication cookie with 8-hour expiry, sliding expiration
   - Add authorization services
   - Add UseAuthentication() and UseAuthorization() middleware (in correct order)
4. Create the EF Core migration command I should run

Do NOT create login pages yet - just the infrastructure.
Reference: Docs/Opus/03-Database-Schema.md for schema details
```

### 1.2 Create Login and Logout Pages

```text
Create login and logout functionality for my Blazor Server app with ASP.NET Core Identity.

Context:
- Using ApplicationUser : IdentityUser
- Using SignInManager<ApplicationUser> and UserManager<ApplicationUser>
- Blazor Server (not WASM)

Requirements:
1. Create `Components/Account/Login.razor`:
   - Route: /account/login
   - Email and Password fields with validation
   - "Remember me" checkbox
   - Error message display area
   - Submit calls SignInManager.PasswordSignInAsync
   - On success, redirect to "/" or returnUrl
   - Handle lockout scenario with user-friendly message
   - Handle invalid credentials with generic "Invalid email or password" message

2. Create `Components/Account/Logout.razor`:
   - Route: /account/logout
   - Immediately signs out and redirects to login
   - Use POST to prevent CSRF

3. Create `Components/Account/AccessDenied.razor`:
   - Route: /account/access-denied
   - User-friendly message explaining access was denied
   - Link back to home page

4. Create a shared `_LoginPartial.razor` component showing:
   - If authenticated: User email + Logout button
   - If anonymous: Login link

Use Blazor's EditForm with DataAnnotationsValidator.
No external CSS frameworks - use simple inline styles or existing project styles.
```

### 1.3 Create CurrentUserService

```text
Create an ICurrentUserService to provide the current user's context throughout the application.

Context:
- Blazor Server app
- ASP.NET Core Identity with ApplicationUser
- Need to access current user info in services and components

Interface requirements (from Docs/Opus/04-Technical-Architecture.md):
```csharp
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    Guid? OrganizationId { get; }
    Guid? WorkspaceId { get; }
    bool IsSystemAdmin { get; }
    bool IsOrgAdmin { get; }
    Task<IReadOnlySet<string>> GetPermissionsAsync();
    Task<bool> HasPermissionAsync(string permissionKey);
}
```

Implementation requirements:
1. Inject AuthenticationStateProvider to get current user claims
2. UserId comes from ClaimTypes.NameIdentifier
3. Email comes from ClaimTypes.Email
4. For now, OrganizationId and WorkspaceId can return null (will implement later)
5. IsSystemAdmin checks for "SystemAdmin" role claim
6. GetPermissionsAsync returns empty set for now (will implement in Phase 3)
7. Register as Scoped service in Program.cs

The service should be usable in both Razor components and other services.
```

### 1.4 Create Session Management Service

```text
Create a session management service to track and validate user sessions.

Context:
- Need to limit concurrent sessions to 5 per user
- Need sliding expiration (2 hours) and absolute expiration (24 hours)
- Need ability to revoke all sessions on password change

Database table (from Docs/Opus/03-Database-Schema.md):
```sql
CREATE TABLE [dbo].[UserSessions] (
    [Id]                NVARCHAR(100)    NOT NULL,
    [UserId]            NVARCHAR(450)    NOT NULL,
    [DeviceInfo]        NVARCHAR(500)    NULL,
    [IpAddress]         NVARCHAR(45)     NULL,
    [CreatedAt]         DATETIME2        NOT NULL,
    [LastActivityAt]    DATETIME2        NOT NULL,
    [ExpiresAt]         DATETIME2        NOT NULL,
    [IsRevoked]         BIT              NOT NULL DEFAULT 0,
    [RevokedAt]         DATETIME2        NULL,
    [RevokedReason]     NVARCHAR(200)    NULL,
    PRIMARY KEY ([Id])
);
```

Create:
1. UserSession entity class
2. ISessionService interface with methods:
   - CreateSessionAsync(userId, deviceInfo)
   - ValidateSessionAsync(sessionId)
   - TouchSessionAsync(sessionId)
   - RevokeSessionAsync(sessionId, reason)
   - RevokeAllUserSessionsAsync(userId, reason)
   - GetUserSessionsAsync(userId)
   - CleanupExpiredSessionsAsync()
3. SessionService implementation
4. SessionOptions configuration class (for timeout values)

Session ID should be 32 bytes of cryptographically secure random data, base64 encoded.
```

### 1.5 Seed Initial Admin User

```text
Create a data seeding mechanism for the initial system administrator.

Requirements:
1. Create a SeedData static class with method InitializeAsync(IServiceProvider)
2. On first run:
   - Create "SystemAdmin" role if it doesn't exist
   - Create admin user if no users exist
   - Email: read from configuration "SeedAdmin:Email" (default: admin@localhost)
   - Password: read from configuration "SeedAdmin:Password" (required, fail if missing)
   - Assign SystemAdmin role to this user
3. Log warnings if using default email
4. Never log the password
5. Call SeedData.InitializeAsync at app startup (after app.Build())

Important security notes:
- Password MUST come from configuration (appsettings, env var, or secret manager)
- Fail startup if password is not configured
- Set MustChangePassword = true in UserProfile after creation
```

---

## Phase 2 Prompts

### 2.1 Create Organization and Workspace Entities

```text
Create the Organization and Workspace entity classes and DbContext configuration.

Reference: Docs/Opus/03-Database-Schema.md for complete schema

Create these entities:
1. Organization - multi-tenant organization
2. Workspace - collection of forms within an organization
3. OrganizationUser - maps users to orgs with role (0=Member, 1=Admin)
4. WorkspaceMember - maps users to workspaces with role template

Each entity needs:
- Entity class with properties matching the schema
- Fluent API configuration in ApplicationDbContext.OnModelCreating
- Proper indexes and foreign keys as specified in schema doc

Also create:
- OrganizationRole enum (Member = 0, Admin = 1)

Register entities in ApplicationDbContext and generate migration.
```

### 2.2 Create User Management Service

```text
Create a service for managing users within an organization.

Interface:
```csharp
public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(Guid organizationId, int page, int pageSize);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<Result> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<Result> DeactivateUserAsync(string userId, string reason);
    Task<Result> ReactivateUserAsync(string userId);
    Task<Result> ResetPasswordAsync(string userId);
    Task<Result> ChangeUserRoleAsync(string userId, Guid organizationId, OrganizationRole role);
}
```

DTO classes needed:
- UserDto (Id, Email, DisplayName, IsActive, OrganizationRole, CreatedAt, LastLoginAt)
- CreateUserRequest (Email, DisplayName, OrganizationId, Role, SendInvitation)
- UpdateUserRequest (DisplayName, IsActive)

Implementation requirements:
1. Inject UserManager<ApplicationUser>, ApplicationDbContext, ICurrentUserService
2. Only org admins or system admins can manage users
3. Users can only manage users in their own organization
4. CreateUser should:
   - Create ApplicationUser
   - Create UserProfile with MustChangePassword = true
   - Create OrganizationUser mapping
   - Optionally send invitation email (placeholder for now)
5. DeactivateUser sets IsActive = false and revokes all sessions
6. Use Result pattern for error handling (return errors, don't throw)
```

### 2.3 Create Invitation Service

```text
Create an invitation service for inviting new users to an organization.

Database table (from Docs/Opus/03-Database-Schema.md):
```sql
CREATE TABLE [dbo].[Invitations] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [Email]             NVARCHAR(256)    NOT NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NOT NULL,
    [WorkspaceId]       UNIQUEIDENTIFIER NULL,
    [RoleTemplateId]    UNIQUEIDENTIFIER NULL,
    [InvitedBy]         NVARCHAR(450)    NOT NULL,
    [Token]             NVARCHAR(100)    NOT NULL,
    [CreatedAt]         DATETIME2        NOT NULL,
    [ExpiresAt]         DATETIME2        NOT NULL,
    [AcceptedAt]        DATETIME2        NULL,
    [AcceptedByUserId]  NVARCHAR(450)    NULL,
    PRIMARY KEY ([Id])
);
```

Interface:
```csharp
public interface IInvitationService
{
    Task<Result<Invitation>> CreateInvitationAsync(InviteRequest request);
    Task<InvitationDto?> GetInvitationByTokenAsync(string token);
    Task<Result<UserDto>> AcceptInvitationAsync(string token, AcceptInviteRequest request);
    Task<Result> RevokeInvitationAsync(Guid invitationId);
    Task<List<InvitationDto>> GetPendingInvitationsAsync(Guid organizationId);
}
```

Requirements:
1. Token is 32 bytes of secure random, URL-safe base64
2. Invitations expire after 7 days
3. AcceptInvitation:
   - Validates token is valid and not expired
   - Creates new user if email doesn't exist
   - Adds user to organization and optionally workspace
   - Marks invitation as accepted
4. CreateInvitation checks if email already exists in org (error if so)
5. Include placeholder for sending email (just log for now)
```

### 2.4 Create User Management UI

```text
Create Blazor components for user management in the admin area.

Components needed:

1. `Components/Admin/Users/UserList.razor`
   - Route: /admin/users
   - Requires [Authorize(Policy = "CanManageUsers")]
   - DataGrid showing: Email, Display Name, Role, Status, Last Login
   - Search box to filter by email/name
   - Pagination
   - "Invite User" button opens modal
   - Row actions: Edit, Deactivate/Reactivate

2. `Components/Admin/Users/InviteUserModal.razor`
   - Modal dialog
   - Fields: Email, Role dropdown
   - Validates email format
   - Shows success message with note about email being sent

3. `Components/Admin/Users/EditUserModal.razor`
   - Modal dialog
   - Fields: Display Name, Role dropdown, Active toggle
   - Save and Cancel buttons

Use the existing modal pattern in the project (ModalBase.razor.cs if it exists).
Inject IUserService and IInvitationService.
Show toast notifications for success/error using IToastService.
```

---

## Phase 3 Prompts

### 3.1 Create Role Template and Permission Entities

```text
Create entities for role templates and form permissions.

Reference: Docs/Opus/03-Database-Schema.md

Entities needed:

1. RoleTemplate:
   - Id, OrganizationId (null for system templates), Name, Description
   - IsSystemRole (cannot be deleted)
   - Permissions (JSON string of permission keys)
   - SurveyJsConfig (JSON string of creator config)

2. FormPermission:
   - Id, FormId, PrincipalId, PrincipalType
   - PermissionLevel (0=None, 10=View, 20=ViewData, 30=EditData, 40=Edit, 50=EditAll, 60=Admin)
   - CustomPermissions (JSON), GrantedBy, GrantedAt, ExpiresAt, Reason

Create:
- Entity classes with EF configuration
- PrincipalType enum (User = 0, RoleTemplate = 1, Workspace = 2)
- PermissionLevel enum matching values above
- Migration

Also seed the default role templates (from Docs/Opus/02-Role-Permission-Model.md):
- Workspace Owner (form.*, data.*)
- Form Designer (form.create, form.edit_*, form.delete, form.publish)
- Data Manager (form.view_design, data.*)
- Reviewer (form.view_design, data.view_*)
```

### 3.2 Create Form Policy Evaluator

```text
Create the form policy evaluator service that determines user permissions for forms.

Reference: Docs/Opus/04-Technical-Architecture.md for full interface

The evaluator must implement permission resolution in this order:
1. System admin → Admin access to everything
2. Explicit user deny → Deny access
3. Explicit user grant → Use that level
4. Workspace owner → Admin access to workspace forms
5. Role template from workspace membership → Use template permissions
6. Workspace default permission level → Use workspace default
7. No grant found → Deny

Interface:
```csharp
public interface IFormPolicyEvaluator
{
    Task<PolicyResult> CanViewAsync(string formId);
    Task<PolicyResult> CanEditAsync(string formId);
    Task<PolicyResult> CanDeleteAsync(string formId);
    Task<PolicyResult> CanViewDataAsync(string formId);
    Task<PolicyResult> CanExportDataAsync(string formId);
    Task<PolicyResult> CanManagePermissionsAsync(string formId);
    Task<PermissionLevel> GetEffectivePermissionAsync(string formId);
    Task<CreatorConfig> GetCreatorConfigAsync(string formId);
}
```

CreatorConfig should include all SurveyJS creator options:
- ReadOnly, ShowJSONEditorTab, ShowLogicTab, ShowThemeTab, ShowPreviewTab
- AllowAddQuestions, AllowDeleteQuestions, AllowDragDrop, AllowChangeType
- HiddenToolboxItems (list of question types to hide)

Include comprehensive unit tests for all permission scenarios.
```

### 3.3 Create Authorization Handlers

```text
Create ASP.NET Core authorization handlers for form operations.

Requirements:
1. Create FormOperationRequirement class implementing IAuthorizationRequirement
2. Create FormOperation enum (View, Edit, Delete, ViewData, ExportData, ManagePermissions)
3. Create FormAuthorizationHandler : AuthorizationHandler<FormOperationRequirement, string>
   - The resource (string) is the formId
   - Use IFormPolicyEvaluator to check permissions
   - Call context.Succeed or context.Fail based on result

4. Register policies in Program.cs:
   - "CanViewForm" → FormOperation.View
   - "CanEditForm" → FormOperation.Edit
   - "CanDeleteForm" → FormOperation.Delete
   - "CanViewFormData" → FormOperation.ViewData
   - "CanManageFormPermissions" → FormOperation.ManagePermissions

5. Register FormAuthorizationHandler as scoped service

Usage example:
```csharp
var authResult = await _authService.AuthorizeAsync(User, formId, "CanEditForm");
if (!authResult.Succeeded) return Forbid();
```
```

### 3.4 Create Form Sharing UI

```text
Create a modal component for sharing forms with users.

Component: `Components/Editor/Modals/ShareFormModal.razor`

Features:
1. Show current permissions:
   - List of users/roles with access
   - Their permission level
   - Who granted it and when

2. Add new permission:
   - Search for user by email
   - Select permission level from dropdown
   - Optional expiration date
   - Add button

3. Modify existing permission:
   - Change permission level
   - Remove access entirely

4. Show workspace-inherited permissions (read-only display)

Use IFormPermissionService for backend operations:
```csharp
public interface IFormPermissionService
{
    Task<List<FormPermissionDto>> GetPermissionsAsync(string formId);
    Task<Result> GrantPermissionAsync(string formId, GrantPermissionRequest request);
    Task<Result> RevokePermissionAsync(Guid permissionId);
    Task<Result> UpdatePermissionAsync(Guid permissionId, UpdatePermissionRequest request);
}
```
```

---

## Phase 4 Prompts

### 4.1 Create SurveyJS Permission Interop

```text
Create the JavaScript interop for applying permissions to SurveyJS Creator.

Reference: Docs/Opus/04-Technical-Architecture.md section 5

Create `wwwroot/js/editor-security.js`:

1. `window.initSurveyCreator(formId, config)` function that:
   - Stores config globally as window.creatorConfig
   - Creates SurveyCreator.SurveyCreator with options from config
   - Applies onElementAllowOperations restrictions
   - Removes hidden toolbox items
   - Hides add question buttons via CSS if not allowed
   - Sets up save interception to validate server-side

2. Config object structure:
```javascript
{
    readOnly: boolean,
    showJSONEditorTab: boolean,
    showLogicTab: boolean,
    showThemeTab: boolean,
    showPreviewTab: boolean,
    allowAddQuestions: boolean,
    allowDeleteQuestions: boolean,
    allowDragDrop: boolean,
    allowChangeType: boolean,
    allowedQuestionTypes: string[] | null,
    hiddenToolboxItems: string[]
}
```

3. Save interception:
   - Before saving, call DotNet.invokeMethodAsync to validate
   - If validation fails, show errors and don't save
   - If validation passes, proceed with save

4. Also update Editor.razor to:
   - Load CreatorConfig from IFormPolicyEvaluator
   - Pass config to JS via IJSRuntime on first render
```

### 4.2 Create Server-Side Form Validation

```text
Create the form validation service that prevents unauthorized changes.

Reference: Docs/Opus/04-Technical-Architecture.md section 3.3

Interface:
```csharp
public interface IFormValidationService
{
    Task<ValidationResult> ValidateUpdateAsync(
        string formId,
        string oldJson,
        string newJson,
        string userId);

    FormChangeSummary DetectChanges(string oldJson, string newJson);
}
```

FormChangeSummary must detect:
- HasStructureChanges (questions added/removed/reordered)
- HasTextChanges (labels, descriptions, help text)
- HasLogicChanges (visibleIf, enableIf, requiredIf, triggers)
- HasValidationChanges (validators, min/max, required)
- HasThemeChanges (styling properties)
- Lists of added/removed/modified question names

ValidateUpdateAsync must:
1. Get user's permissions via ICurrentUserService
2. Detect all change types
3. For each change type, check if user has required permission:
   - Structure changes require "form.edit_structure"
   - Logic changes require "form.edit_logic"
   - Validation changes require "form.edit_validation"
   - Theme changes require "form.edit_theme"
4. Check for dangerous content (XSS patterns)
5. Log unauthorized attempts via IAuditService
6. Return ValidationResult with IsValid and list of Errors

Dangerous content patterns to detect:
- <script> tags
- javascript: URLs
- Event handlers (onclick, onerror, onload)
- eval() in expressions
```

### 4.3 Create Form Version History

```text
Create form version tracking to store history of all changes.

Database table (from Docs/Opus/03-Database-Schema.md):
```sql
CREATE TABLE [dbo].[FormVersionHistory] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [FormId]            NVARCHAR(450)    NOT NULL,
    [Version]           INT              NOT NULL,
    [SchemaJson]        NVARCHAR(MAX)    NOT NULL,
    [ChangeDescription] NVARCHAR(500)    NULL,
    [ChangedBy]         NVARCHAR(450)    NOT NULL,
    [ChangedAt]         DATETIME2        NOT NULL,
    [IsMajorVersion]    BIT              NOT NULL DEFAULT 0,
    PRIMARY KEY ([Id])
);
```

Create:
1. FormVersionHistory entity
2. IFormVersionService interface:
```csharp
public interface IFormVersionService
{
    Task SaveVersionAsync(string formId, string schemaJson, string? description, bool isMajor);
    Task<List<FormVersionDto>> GetVersionHistoryAsync(string formId, int limit = 50);
    Task<FormVersionDto?> GetVersionAsync(string formId, int version);
    Task<string?> GetVersionJsonAsync(string formId, int version);
    Task<Result> RestoreVersionAsync(string formId, int version);
}
```

3. Integration with form save:
   - After successful validation, save new version
   - Auto-increment version number
   - Mark as major version if explicit save (vs auto-save)

4. Version history component showing:
   - Version number, date, who changed it
   - Ability to view JSON at that version
   - Ability to restore (creates new version with old content)
```

---

## Phase 5 Prompts

### 5.1 Create Audit Service

```text
Create a comprehensive audit logging service.

Reference: Docs/Opus/05-Security-Audit.md for full implementation

Database table (from Docs/Opus/03-Database-Schema.md):
```sql
CREATE TABLE [dbo].[AuditLog] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [Timestamp]         DATETIME2        NOT NULL,
    [UserId]            NVARCHAR(450)    NULL,
    [UserEmail]         NVARCHAR(256)    NULL,
    [IpAddress]         NVARCHAR(45)     NULL,
    [UserAgent]         NVARCHAR(500)    NULL,
    [Action]            NVARCHAR(100)    NOT NULL,
    [EntityType]        NVARCHAR(100)    NULL,
    [EntityId]          NVARCHAR(450)    NULL,
    [OrganizationId]    UNIQUEIDENTIFIER NULL,
    [WorkspaceId]       UNIQUEIDENTIFIER NULL,
    [OldValues]         NVARCHAR(MAX)    NULL,
    [NewValues]         NVARCHAR(MAX)    NULL,
    [Metadata]          NVARCHAR(MAX)    NULL,
    [Severity]          TINYINT          NOT NULL DEFAULT 0,
    PRIMARY KEY ([Id])
);
```

Interface (from Docs/Opus/05-Security-Audit.md):
```csharp
public interface IAuditService
{
    Task LogAsync(AuditEntry entry);
    Task LogAuthEventAsync(string action, string? userId, string? email, bool success, string? failureReason = null);
    Task LogFormChangeAsync(string formId, string action, string? oldJson, string? newJson, string? description = null);
    Task LogSecurityEventAsync(string action, string? entityId, string? details, AuditSeverity severity = AuditSeverity.Warning);
    Task<PagedResult<AuditEntry>> QueryAsync(AuditQuery query);
}
```

Implementation requirements:
1. Get current user context from ICurrentUserService
2. Get IP address from IHttpContextAccessor (check X-Forwarded-For header)
3. For form changes, generate a compact diff summary, not full JSON
4. For critical security events (severity >= Error), also log via ILogger
5. All audit writes should be async and not block the main request
```

### 5.2 Create Audit Log Viewer

```text
Create an admin page for viewing and searching audit logs.

Component: `Components/Admin/AuditLogs/AuditLogViewer.razor`
Route: /admin/audit-logs
Authorization: [Authorize(Policy = "OrgAdmin")]

Features:
1. Filters:
   - Date range (start/end date pickers)
   - User (search by email)
   - Action type (dropdown: All, Authentication, Forms, Security)
   - Entity type/ID
   - Severity (dropdown: All, Info+, Warning+, Error+)

2. Results table:
   - Timestamp
   - User (email)
   - Action
   - Entity (Type:Id)
   - IP Address
   - Severity (color-coded badge)
   - Details button

3. Details modal:
   - Full audit entry details
   - Old/New values (formatted JSON)
   - Metadata

4. Export button:
   - Export filtered results to CSV
   - Include all fields

5. Pagination:
   - 50 entries per page
   - Page navigation

Use IAuditService.QueryAsync for data retrieval.
Color code severity: Info=gray, Warning=yellow, Error=red, Critical=dark red
```

### 5.3 Add Audit Logging Throughout Application

```text
Add audit logging calls to all relevant operations in the application.

Authentication events (in AuthService):
- "user.login_success" - successful login
- "user.login_failed" - failed login attempt
- "user.logout" - user logged out
- "user.password_changed" - password was changed
- "user.password_reset_requested" - password reset email sent
- "user.session_created" - new session created
- "user.session_revoked" - session was revoked

User management events (in UserService):
- "user.created" - new user created
- "user.updated" - user profile updated
- "user.deactivated" - user deactivated
- "user.reactivated" - user reactivated
- "user.role_changed" - user's role changed

Form events (in FormService):
- "form.created" - new form created
- "form.updated" - form modified (include change summary)
- "form.deleted" - form deleted
- "form.published" - form status changed to published
- "form.permission_granted" - access granted to user
- "form.permission_revoked" - access revoked

Security events (automatic):
- "security.unauthorized_access" - access denied to resource
- "security.validation_failed" - server-side validation blocked change
- "security.dangerous_content" - XSS attempt detected
- "security.session_expired" - session validation failed

For each call, include relevant context (entity IDs, old/new values where appropriate).
```

---

## Testing Prompts

### Test: Permission Evaluation

```text
Write comprehensive unit tests for FormPolicyEvaluator.

Test scenarios:
1. System admin gets Admin access to any form
2. Org admin gets Admin access to forms in their org
3. Workspace owner gets Admin access to workspace forms
4. User with explicit Admin grant gets Admin
5. User with explicit View grant gets View
6. User with explicit None (deny) gets denied even with other grants
7. User with role template Designer gets Edit permissions
8. User with role template DataManager gets ViewData permissions
9. User with role template Reviewer gets View permissions
10. User with no grants gets denied
11. Expired permission is not used
12. Permission in different workspace is not used

Use xUnit, Moq for dependencies.
Create in-memory test fixtures for users, workspaces, forms, permissions.
```

### Test: Form Validation

```text
Write unit tests for FormValidationService.

Test scenarios for DetectChanges:
1. No changes returns all false
2. Added question detected as structure change
3. Removed question detected as structure change
4. Reordered questions detected as structure change
5. Changed label detected as text change
6. Changed visibleIf detected as logic change
7. Changed validator detected as validation change
8. Changed theme property detected as theme change

Test scenarios for ValidateUpdateAsync:
1. User with all permissions - all changes allowed
2. User with text-only permission - structure change rejected
3. User with no-logic permission - logic change rejected
4. XSS in HTML question type detected and rejected
5. javascript: URL detected and rejected
6. Unauthorized change logs security event

Mock ICurrentUserService, IFormPolicyEvaluator, IAuditService.
```

---

## Deployment Prompts

### Database Migration

```text
Create a SQL migration script that can be run in production.

Requirements:
1. Create all new tables from Docs/Opus/03-Database-Schema.md
2. Add new columns to existing FormModuleSchema table
3. Seed default role templates
4. Create default organization and workspace
5. Migrate existing forms to default workspace
6. All operations should be idempotent (safe to run multiple times)
7. Include rollback script

Output:
- migrations/V1__RBAC_Initial.sql (forward migration)
- migrations/V1__RBAC_Initial_Rollback.sql (rollback migration)

Use transactions where appropriate.
Include GO statements for SQL Server batch separation.
```

---

## Notes for Implementation

1. **Read the docs first**: Before using any prompt, have the AI read the relevant documentation files
2. **Context is key**: The prompts reference other documents - make sure those are available
3. **Iterate**: These prompts may need adjustment based on your specific implementation
4. **Test thoroughly**: Each phase should have comprehensive tests before moving on
5. **Security review**: Have security-critical code reviewed before deployment
