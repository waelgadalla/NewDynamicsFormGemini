# Role-Based Access Control for Visual Editor Opus
# Part 4: Technical Architecture

**Document Version:** 2.0
**Date:** December 2025
**Stack:** ASP.NET Core 9, Blazor Server, Entity Framework Core, SQL Server

---

## 1. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BLAZOR SERVER UI                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Login.razor │  │ Users.razor │  │ Forms.razor │  │ Editor.razor│        │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘        │
└─────────┼────────────────┼────────────────┼────────────────┼────────────────┘
          │                │                │                │
          ▼                ▼                ▼                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           SERVICE LAYER                                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │ AuthService     │  │ UserService     │  │ FormService     │             │
│  │ SessionService  │  │ InviteService   │  │ PermissionSvc   │             │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘             │
└───────────┼────────────────────┼────────────────────┼───────────────────────┘
            │                    │                    │
            ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        SECURITY & VALIDATION LAYER                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │ PolicyEvaluator │  │ FormValidator   │  │ AuditService    │             │
│  │ ClaimsTransform │  │ JsonDiffEngine  │  │ SessionManager  │             │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘             │
└───────────┼────────────────────┼────────────────────┼───────────────────────┘
            │                    │                    │
            ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          DATA ACCESS LAYER                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │ AppDbContext    │  │ FormRepository  │  │ AuditRepository │             │
│  │ (Identity +     │  │                 │  │                 │             │
│  │  Custom Tables) │  │                 │  │                 │             │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘             │
└───────────┼────────────────────┼────────────────────┼───────────────────────┘
            │                    │                    │
            └────────────────────┴────────────────────┘
                                 │
                                 ▼
                          ┌─────────────┐
                          │  SQL Server │
                          └─────────────┘
```

---

## 2. Core Interfaces

### 2.1 ICurrentUserService

Provides current user context throughout the application.

```csharp
namespace VisualEditorOpus.Services.Identity;

public interface ICurrentUserService
{
    /// <summary>
    /// Current user's ID from claims, null if anonymous
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Current user's email
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Display name for UI
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Current organization context (from route or session)
    /// </summary>
    Guid? OrganizationId { get; }

    /// <summary>
    /// Current workspace context (from route or session)
    /// </summary>
    Guid? WorkspaceId { get; }

    /// <summary>
    /// Check if user has a system-level role
    /// </summary>
    bool IsSystemAdmin { get; }

    /// <summary>
    /// Check if user is admin of current organization
    /// </summary>
    bool IsOrgAdmin { get; }

    /// <summary>
    /// Get all permission keys for the current user
    /// </summary>
    Task<IReadOnlySet<string>> GetPermissionsAsync();

    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(string permissionKey);
}
```

### 2.2 IFormPolicyEvaluator

Centralized form access control evaluation.

```csharp
namespace VisualEditorOpus.Services.Authorization;

public interface IFormPolicyEvaluator
{
    /// <summary>
    /// Check if user can view the form design
    /// </summary>
    Task<PolicyResult> CanViewAsync(string formId);

    /// <summary>
    /// Check if user can edit the form structure
    /// </summary>
    Task<PolicyResult> CanEditAsync(string formId);

    /// <summary>
    /// Check if user can delete the form
    /// </summary>
    Task<PolicyResult> CanDeleteAsync(string formId);

    /// <summary>
    /// Check if user can view form submissions
    /// </summary>
    Task<PolicyResult> CanViewDataAsync(string formId);

    /// <summary>
    /// Check if user can export form submissions
    /// </summary>
    Task<PolicyResult> CanExportDataAsync(string formId);

    /// <summary>
    /// Check if user can share/manage form permissions
    /// </summary>
    Task<PolicyResult> CanManagePermissionsAsync(string formId);

    /// <summary>
    /// Get the effective permission level for a form
    /// </summary>
    Task<PermissionLevel> GetEffectivePermissionAsync(string formId);

    /// <summary>
    /// Get SurveyJS Creator configuration based on permissions
    /// </summary>
    Task<CreatorConfig> GetCreatorConfigAsync(string formId);
}

public record PolicyResult(bool IsAllowed, string? DenialReason = null);

public enum PermissionLevel
{
    None = 0,
    View = 10,
    ViewData = 20,
    EditData = 30,
    Edit = 40,
    EditAll = 50,
    Admin = 60
}
```

### 2.3 IFormValidationService

**Critical: Server-side validation to prevent client-side bypasses.**

```csharp
namespace VisualEditorOpus.Services.Validation;

public interface IFormValidationService
{
    /// <summary>
    /// Validates that a form update is allowed based on user permissions.
    /// Compares old and new JSON to detect unauthorized changes.
    /// </summary>
    Task<ValidationResult> ValidateUpdateAsync(
        string formId,
        string oldJson,
        string newJson,
        string userId);

    /// <summary>
    /// Detects what types of changes were made between two form versions
    /// </summary>
    FormChangeSummary DetectChanges(string oldJson, string newJson);
}

public record ValidationResult(
    bool IsValid,
    List<string> Errors,
    FormChangeSummary? Changes = null);

public record FormChangeSummary
{
    public bool HasStructureChanges { get; init; }      // Questions added/removed/reordered
    public bool HasTextChanges { get; init; }           // Labels, descriptions changed
    public bool HasLogicChanges { get; init; }          // Visibility rules, skip logic
    public bool HasValidationChanges { get; init; }     // Validation rules modified
    public bool HasThemeChanges { get; init; }          // Styling changes
    public List<string> AddedQuestions { get; init; } = new();
    public List<string> RemovedQuestions { get; init; } = new();
    public List<string> ModifiedQuestions { get; init; } = new();
}
```

---

## 3. Implementation Classes

### 3.1 FormPolicyEvaluator Implementation

```csharp
namespace VisualEditorOpus.Services.Authorization;

public class FormPolicyEvaluator : IFormPolicyEvaluator
{
    private readonly ICurrentUserService _currentUser;
    private readonly IFormPermissionRepository _permissionRepo;
    private readonly IWorkspaceMemberRepository _memberRepo;
    private readonly IRoleTemplateRepository _roleRepo;

    public FormPolicyEvaluator(
        ICurrentUserService currentUser,
        IFormPermissionRepository permissionRepo,
        IWorkspaceMemberRepository memberRepo,
        IRoleTemplateRepository roleRepo)
    {
        _currentUser = currentUser;
        _permissionRepo = permissionRepo;
        _memberRepo = memberRepo;
        _roleRepo = roleRepo;
    }

    public async Task<PermissionLevel> GetEffectivePermissionAsync(string formId)
    {
        // System admins have full access
        if (_currentUser.IsSystemAdmin)
            return PermissionLevel.Admin;

        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return PermissionLevel.None;

        // 1. Check for explicit user-level permission (including deny)
        var userPermission = await _permissionRepo.GetUserPermissionAsync(formId, userId);
        if (userPermission != null)
        {
            if (userPermission.PermissionLevel == 0) // Explicit deny
                return PermissionLevel.None;
            return (PermissionLevel)userPermission.PermissionLevel;
        }

        // 2. Check for role-based permission via workspace membership
        var form = await _permissionRepo.GetFormWithWorkspaceAsync(formId);
        if (form?.WorkspaceId == null)
            return PermissionLevel.None;

        var membership = await _memberRepo.GetMembershipAsync(
            form.WorkspaceId.Value, userId);

        if (membership == null)
            return PermissionLevel.None;

        // Workspace owners have admin access
        if (membership.IsOwner)
            return PermissionLevel.Admin;

        // Get role template permissions
        if (membership.RoleTemplateId != null)
        {
            var rolePermission = await _permissionRepo.GetRolePermissionAsync(
                formId, membership.RoleTemplateId.Value);

            if (rolePermission != null)
                return (PermissionLevel)rolePermission.PermissionLevel;

            // Fall back to role template's default form permission
            var template = await _roleRepo.GetByIdAsync(membership.RoleTemplateId.Value);
            return GetDefaultPermissionFromTemplate(template);
        }

        // 3. Fall back to workspace default
        var workspace = await _memberRepo.GetWorkspaceAsync(form.WorkspaceId.Value);
        return (PermissionLevel)(workspace?.DefaultPermissionLevel ?? 0);
    }

    public async Task<PolicyResult> CanEditAsync(string formId)
    {
        var level = await GetEffectivePermissionAsync(formId);

        if (level >= PermissionLevel.Edit)
            return new PolicyResult(true);

        return new PolicyResult(false, "You do not have permission to edit this form.");
    }

    public async Task<CreatorConfig> GetCreatorConfigAsync(string formId)
    {
        var level = await GetEffectivePermissionAsync(formId);
        var permissions = await _currentUser.GetPermissionsAsync();

        return new CreatorConfig
        {
            ReadOnly = level < PermissionLevel.Edit,
            ShowJSONEditorTab = permissions.Contains("form.edit_json"),
            ShowLogicTab = permissions.Contains("form.edit_logic"),
            ShowThemeTab = permissions.Contains("form.edit_theme"),
            ShowPreviewTab = true,
            AllowAddQuestions = level >= PermissionLevel.Edit &&
                               permissions.Contains("form.edit_structure"),
            AllowDeleteQuestions = level >= PermissionLevel.Edit &&
                                   permissions.Contains("form.edit_structure"),
            AllowDragDrop = level >= PermissionLevel.Edit,
            AllowChangeType = permissions.Contains("form.edit_structure"),
            HiddenToolboxItems = GetHiddenToolboxItems(permissions)
        };
    }

    private List<string> GetHiddenToolboxItems(IReadOnlySet<string> permissions)
    {
        var hidden = new List<string>();

        // Always hide potentially dangerous elements unless explicitly allowed
        if (!permissions.Contains("form.edit_json"))
        {
            hidden.Add("html");        // Can contain scripts
            hidden.Add("expression");  // Can execute expressions
        }

        return hidden;
    }

    private PermissionLevel GetDefaultPermissionFromTemplate(RoleTemplate? template)
    {
        if (template == null) return PermissionLevel.None;

        // Parse template permissions to determine default level
        var perms = System.Text.Json.JsonSerializer
            .Deserialize<List<string>>(template.Permissions) ?? new();

        if (perms.Contains("form.*")) return PermissionLevel.Admin;
        if (perms.Contains("form.edit_structure")) return PermissionLevel.Edit;
        if (perms.Contains("data.edit_submissions")) return PermissionLevel.EditData;
        if (perms.Contains("data.view_submissions")) return PermissionLevel.ViewData;
        if (perms.Contains("form.view_design")) return PermissionLevel.View;

        return PermissionLevel.None;
    }

    // ... other interface methods
}
```

### 3.2 FormValidationService Implementation

**This is the critical server-side validation that prevents UI bypasses.**

```csharp
namespace VisualEditorOpus.Services.Validation;

public class FormValidationService : IFormValidationService
{
    private readonly IFormPolicyEvaluator _policyEvaluator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public async Task<ValidationResult> ValidateUpdateAsync(
        string formId,
        string oldJson,
        string newJson,
        string userId)
    {
        var errors = new List<string>();

        // Get user's effective permissions
        var permissions = await _currentUser.GetPermissionsAsync();
        var changes = DetectChanges(oldJson, newJson);

        // Validate each type of change against permissions
        if (changes.HasStructureChanges)
        {
            if (!permissions.Contains("form.edit_structure"))
            {
                errors.Add("You do not have permission to add, remove, or reorder questions.");

                // Log attempted unauthorized action
                await _auditService.LogSecurityEventAsync(
                    "form.unauthorized_structure_change",
                    formId,
                    $"User attempted to modify form structure without permission. " +
                    $"Added: {string.Join(", ", changes.AddedQuestions)}, " +
                    $"Removed: {string.Join(", ", changes.RemovedQuestions)}");
            }
        }

        if (changes.HasLogicChanges && !permissions.Contains("form.edit_logic"))
        {
            errors.Add("You do not have permission to modify form logic rules.");
            await _auditService.LogSecurityEventAsync(
                "form.unauthorized_logic_change", formId, null);
        }

        if (changes.HasValidationChanges && !permissions.Contains("form.edit_validation"))
        {
            errors.Add("You do not have permission to modify validation rules.");
        }

        if (changes.HasThemeChanges && !permissions.Contains("form.edit_theme"))
        {
            errors.Add("You do not have permission to modify form styling.");
        }

        // Check for dangerous content (XSS prevention)
        var dangerousContent = DetectDangerousContent(newJson);
        if (dangerousContent.Any())
        {
            errors.Add($"Form contains potentially dangerous content: {string.Join(", ", dangerousContent)}");
            await _auditService.LogSecurityEventAsync(
                "form.dangerous_content_detected", formId,
                System.Text.Json.JsonSerializer.Serialize(dangerousContent),
                AuditSeverity.Warning);
        }

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors,
            Changes: changes);
    }

    public FormChangeSummary DetectChanges(string oldJson, string newJson)
    {
        using var oldDoc = System.Text.Json.JsonDocument.Parse(oldJson);
        using var newDoc = System.Text.Json.JsonDocument.Parse(newJson);

        var oldQuestions = ExtractQuestions(oldDoc.RootElement);
        var newQuestions = ExtractQuestions(newDoc.RootElement);

        var added = newQuestions.Keys.Except(oldQuestions.Keys).ToList();
        var removed = oldQuestions.Keys.Except(newQuestions.Keys).ToList();
        var modified = new List<string>();

        foreach (var key in oldQuestions.Keys.Intersect(newQuestions.Keys))
        {
            if (!JsonElementEquals(oldQuestions[key], newQuestions[key]))
            {
                modified.Add(key);
            }
        }

        return new FormChangeSummary
        {
            HasStructureChanges = added.Any() || removed.Any() ||
                                 HasOrderChanged(oldDoc, newDoc),
            HasTextChanges = HasTextPropertyChanges(oldDoc, newDoc),
            HasLogicChanges = HasLogicPropertyChanges(oldDoc, newDoc),
            HasValidationChanges = HasValidationPropertyChanges(oldDoc, newDoc),
            HasThemeChanges = HasThemePropertyChanges(oldDoc, newDoc),
            AddedQuestions = added,
            RemovedQuestions = removed,
            ModifiedQuestions = modified
        };
    }

    private Dictionary<string, JsonElement> ExtractQuestions(JsonElement root)
    {
        var questions = new Dictionary<string, JsonElement>();

        if (root.TryGetProperty("pages", out var pages))
        {
            foreach (var page in pages.EnumerateArray())
            {
                if (page.TryGetProperty("elements", out var elements))
                {
                    foreach (var element in elements.EnumerateArray())
                    {
                        if (element.TryGetProperty("name", out var name))
                        {
                            questions[name.GetString() ?? ""] = element;
                        }
                    }
                }
            }
        }

        return questions;
    }

    private bool HasLogicPropertyChanges(JsonDocument oldDoc, JsonDocument newDoc)
    {
        // Check for changes in visibleIf, enableIf, requiredIf, etc.
        var logicProperties = new[] { "visibleIf", "enableIf", "requiredIf", "triggers" };

        return HasPropertyChanges(oldDoc, newDoc, logicProperties);
    }

    private bool HasValidationPropertyChanges(JsonDocument oldDoc, JsonDocument newDoc)
    {
        var validationProperties = new[]
        {
            "isRequired", "validators", "minLength", "maxLength",
            "min", "max", "inputType", "pattern"
        };

        return HasPropertyChanges(oldDoc, newDoc, validationProperties);
    }

    private List<string> DetectDangerousContent(string json)
    {
        var dangerous = new List<string>();

        // Check for script injection
        if (json.Contains("<script", StringComparison.OrdinalIgnoreCase))
            dangerous.Add("script tags");

        if (json.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
            dangerous.Add("javascript: URLs");

        if (json.Contains("onerror=", StringComparison.OrdinalIgnoreCase) ||
            json.Contains("onclick=", StringComparison.OrdinalIgnoreCase) ||
            json.Contains("onload=", StringComparison.OrdinalIgnoreCase))
            dangerous.Add("event handlers");

        // Check for expression injection in SurveyJS
        if (System.Text.RegularExpressions.Regex.IsMatch(json, @"\{[^}]*eval\s*\("))
            dangerous.Add("eval expressions");

        return dangerous;
    }

    // ... helper methods
}
```

---

## 4. Blazor Integration

### 4.1 Authentication State Provider

```csharp
namespace VisualEditorOpus.Services.Identity;

public class OpusAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OpusAuthenticationStateProvider> _logger;

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // Get user manager in a new scope to avoid DbContext threading issues
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var sessionService = scope.ServiceProvider
            .GetRequiredService<ISessionService>();

        var user = await userManager.GetUserAsync(authenticationState.User);
        if (user == null)
            return false;

        // Check if user is still active
        var profile = await scope.ServiceProvider
            .GetRequiredService<IUserProfileRepository>()
            .GetByUserIdAsync(user.Id);

        if (profile?.IsActive != true)
        {
            _logger.LogWarning("User {UserId} is deactivated, invalidating session", user.Id);
            return false;
        }

        // Check if session is still valid
        var sessionId = authenticationState.User.FindFirst("session_id")?.Value;
        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = await sessionService.ValidateSessionAsync(sessionId);
            if (!session.IsValid)
            {
                _logger.LogWarning("Session {SessionId} is invalid: {Reason}",
                    sessionId, session.InvalidReason);
                return false;
            }
        }

        // Validate security stamp hasn't changed (password change, etc.)
        return await userManager.VerifySecurityStampAsync(user,
            authenticationState.User.FindFirst("security_stamp")?.Value);
    }
}
```

### 4.2 Authorization Handler for Forms

```csharp
namespace VisualEditorOpus.Services.Authorization;

public class FormAuthorizationHandler : AuthorizationHandler<FormOperationRequirement, string>
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FormOperationRequirement requirement,
        string formId)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var policyEvaluator = scope.ServiceProvider
            .GetRequiredService<IFormPolicyEvaluator>();

        var result = requirement.Operation switch
        {
            FormOperation.View => await policyEvaluator.CanViewAsync(formId),
            FormOperation.Edit => await policyEvaluator.CanEditAsync(formId),
            FormOperation.Delete => await policyEvaluator.CanDeleteAsync(formId),
            FormOperation.ViewData => await policyEvaluator.CanViewDataAsync(formId),
            FormOperation.ManagePermissions => await policyEvaluator.CanManagePermissionsAsync(formId),
            _ => new PolicyResult(false, "Unknown operation")
        };

        if (result.IsAllowed)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, result.DenialReason));
        }
    }
}

public class FormOperationRequirement : IAuthorizationRequirement
{
    public FormOperation Operation { get; }

    public FormOperationRequirement(FormOperation operation)
    {
        Operation = operation;
    }
}

public enum FormOperation
{
    View,
    Edit,
    Delete,
    ViewData,
    ExportData,
    ManagePermissions
}
```

### 4.3 Blazor Component Integration

```csharp
// In Editor.razor
@page "/editor/{FormId}"
@attribute [Authorize]
@inject IFormPolicyEvaluator PolicyEvaluator
@inject IJSRuntime JS

@code {
    [Parameter] public string FormId { get; set; } = "";

    private CreatorConfig? _creatorConfig;
    private bool _isAuthorized = false;
    private string? _denialReason;

    protected override async Task OnInitializedAsync()
    {
        // Check authorization before loading anything
        var result = await PolicyEvaluator.CanViewAsync(FormId);

        if (!result.IsAllowed)
        {
            _isAuthorized = false;
            _denialReason = result.DenialReason;
            return;
        }

        _isAuthorized = true;
        _creatorConfig = await PolicyEvaluator.GetCreatorConfigAsync(FormId);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _isAuthorized && _creatorConfig != null)
        {
            // Initialize SurveyJS Creator with permission-based config
            await JS.InvokeVoidAsync("initSurveyCreator", FormId, _creatorConfig);
        }
    }
}
```

---

## 5. JavaScript Interop for SurveyJS

### 5.1 Creator Initialization

```javascript
// wwwroot/js/editor-security.js

window.initSurveyCreator = function(formId, config) {
    // Store config globally for reference
    window.creatorConfig = config;

    const creatorOptions = {
        showJSONEditorTab: config.showJSONEditorTab,
        showLogicTab: config.showLogicTab,
        showThemeTab: config.showThemeTab,
        showPreviewTab: config.showPreviewTab,
        readOnly: config.readOnly,
        questionTypes: config.allowedQuestionTypes || undefined
    };

    const creator = new SurveyCreator.SurveyCreator(creatorOptions);

    // Apply element-level restrictions
    creator.onElementAllowOperations.add((sender, options) => {
        if (config.readOnly) {
            options.allowDelete = false;
            options.allowCopy = false;
            options.allowDrag = false;
            options.allowEdit = false;
            options.allowChangeType = false;
            return;
        }

        options.allowDelete = config.allowDeleteQuestions;
        options.allowCopy = config.allowAddQuestions;
        options.allowDrag = config.allowDragDrop;
        options.allowChangeType = config.allowChangeType;
        options.allowAddToToolbox = false; // Always disabled
    });

    // Remove hidden toolbox items
    if (config.hiddenToolboxItems && config.hiddenToolboxItems.length > 0) {
        config.hiddenToolboxItems.forEach(itemName => {
            const item = creator.toolbox.getItemByName(itemName);
            if (item) {
                creator.toolbox.removeItem(itemName);
            }
        });
    }

    // Hide add question button if not allowed
    if (!config.allowAddQuestions) {
        creator.onSurveyInstanceCreated.add((sender, options) => {
            if (options.reason === "designer") {
                options.survey.showQuestionNumbers = "off";
            }
        });

        // CSS hide the add buttons
        const style = document.createElement('style');
        style.textContent = `
            .svc-page__add-new-question,
            .svc-panel__add-new-question,
            .svc-row__drop-target { display: none !important; }
        `;
        document.head.appendChild(style);
    }

    // Intercept save to validate server-side
    creator.saveSurveyFunc = async (saveNo, callback) => {
        const json = JSON.stringify(creator.JSON);

        try {
            const response = await DotNet.invokeMethodAsync(
                'VisualEditorOpus',
                'ValidateAndSaveForm',
                formId,
                json
            );

            if (response.isValid) {
                callback(saveNo, true);
            } else {
                // Show validation errors
                showValidationErrors(response.errors);
                callback(saveNo, false);
            }
        } catch (error) {
            console.error('Save failed:', error);
            callback(saveNo, false);
        }
    };

    window.surveyCreator = creator;
    creator.render("creatorContainer");
};

function showValidationErrors(errors) {
    // Display errors in UI
    const errorHtml = errors.map(e => `<li>${e}</li>`).join('');
    // Use your toast/notification system
    window.toastService?.showError(`<ul>${errorHtml}</ul>`);
}
```

---

## 6. Program.cs Configuration

```csharp
// Program.cs - Security Configuration

var builder = WebApplication.CreateBuilder(args);

// === IDENTITY CONFIGURATION ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;

    // Lockout policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// === AUTHENTICATION ===
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// === AUTHORIZATION POLICIES ===
builder.Services.AddAuthorizationCore(options =>
{
    // System-level policies
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireRole("SystemAdmin"));

    options.AddPolicy("OrgAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("SystemAdmin") ||
            ctx.User.HasClaim("org_role", "Admin")));

    // Form operation policies (evaluated dynamically)
    options.AddPolicy("CanViewForm", policy =>
        policy.Requirements.Add(new FormOperationRequirement(FormOperation.View)));

    options.AddPolicy("CanEditForm", policy =>
        policy.Requirements.Add(new FormOperationRequirement(FormOperation.Edit)));

    options.AddPolicy("CanDeleteForm", policy =>
        policy.Requirements.Add(new FormOperationRequirement(FormOperation.Delete)));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("permission", "org.manage_users"));
});

// === SERVICES ===
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFormPolicyEvaluator, FormPolicyEvaluator>();
builder.Services.AddScoped<IFormValidationService, FormValidationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, FormAuthorizationHandler>();

// Custom auth state provider
builder.Services.AddScoped<AuthenticationStateProvider, OpusAuthenticationStateProvider>();

var app = builder.Build();

// === MIDDLEWARE PIPELINE ===
app.UseExceptionHandler("/error");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Seed initial data
await SeedData.InitializeAsync(app.Services);

app.Run();
```

---

## Next Document

Proceed to **05-Security-Audit.md** for audit logging implementation and session management details.
