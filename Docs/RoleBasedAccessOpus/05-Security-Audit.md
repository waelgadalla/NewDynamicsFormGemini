# Role-Based Access Control for Visual Editor Opus
# Part 5: Security & Audit Implementation

**Document Version:** 2.0
**Date:** December 2025
**Compliance Targets:** NIST 800-53, SOC 2 Type II

---

## 1. Audit Logging Architecture

### 1.1 Audit Event Categories

| Category | Events | Retention |
|----------|--------|-----------|
| **Authentication** | Login, logout, failed login, password change, MFA events | 2 years |
| **Authorization** | Access denied, permission changes, role assignments | 2 years |
| **Form Operations** | Create, edit, delete, publish, version changes | 5 years |
| **Data Access** | View submissions, export data, delete submissions | 5 years |
| **Admin Actions** | User management, org settings, workspace changes | 5 years |
| **Security Events** | Unauthorized attempts, suspicious activity, session issues | 7 years |

### 1.2 IAuditService Interface

```csharp
namespace VisualEditorOpus.Services.Audit;

public interface IAuditService
{
    /// <summary>
    /// Log a general audit event
    /// </summary>
    Task LogAsync(AuditEntry entry);

    /// <summary>
    /// Log authentication event (login, logout, etc.)
    /// </summary>
    Task LogAuthEventAsync(
        string action,
        string? userId,
        string? email,
        bool success,
        string? failureReason = null);

    /// <summary>
    /// Log form modification with before/after state
    /// </summary>
    Task LogFormChangeAsync(
        string formId,
        string action,
        string? oldJson,
        string? newJson,
        string? description = null);

    /// <summary>
    /// Log security-relevant event
    /// </summary>
    Task LogSecurityEventAsync(
        string action,
        string? entityId,
        string? details,
        AuditSeverity severity = AuditSeverity.Warning);

    /// <summary>
    /// Query audit logs with filtering
    /// </summary>
    Task<PagedResult<AuditEntry>> QueryAsync(AuditQuery query);
}

public record AuditEntry
{
    public string Action { get; init; } = "";
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public Guid? OrganizationId { get; init; }
    public Guid? WorkspaceId { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public AuditSeverity Severity { get; init; } = AuditSeverity.Info;
}

public enum AuditSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public record AuditQuery
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? UserId { get; init; }
    public string? Action { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public Guid? OrganizationId { get; init; }
    public AuditSeverity? MinSeverity { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
```

### 1.3 AuditService Implementation

```csharp
namespace VisualEditorOpus.Services.Audit;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContext,
        ILogger<AuditService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _httpContext = httpContext;
        _logger = logger;
    }

    public async Task LogAsync(AuditEntry entry)
    {
        var httpContext = _httpContext.HttpContext;

        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            OrganizationId = entry.OrganizationId ?? _currentUser.OrganizationId,
            WorkspaceId = entry.WorkspaceId ?? _currentUser.WorkspaceId,
            OldValues = entry.OldValues,
            NewValues = entry.NewValues,
            Metadata = entry.Metadata != null
                ? JsonSerializer.Serialize(entry.Metadata)
                : null,
            Severity = (byte)entry.Severity
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();

        // Also log to structured logging for real-time monitoring
        _logger.LogInformation(
            "Audit: {Action} by {UserId} on {EntityType}:{EntityId}",
            entry.Action, _currentUser.UserId, entry.EntityType, entry.EntityId);
    }

    public async Task LogAuthEventAsync(
        string action,
        string? userId,
        string? email,
        bool success,
        string? failureReason = null)
    {
        var metadata = new Dictionary<string, object>
        {
            ["success"] = success
        };

        if (!string.IsNullOrEmpty(failureReason))
        {
            metadata["failure_reason"] = failureReason;
        }

        await LogAsync(new AuditEntry
        {
            Action = action,
            EntityType = "User",
            EntityId = userId,
            Metadata = metadata,
            Severity = success ? AuditSeverity.Info : AuditSeverity.Warning
        });
    }

    public async Task LogFormChangeAsync(
        string formId,
        string action,
        string? oldJson,
        string? newJson,
        string? description = null)
    {
        // Don't store full JSON in audit log - just the diff summary
        var changeSummary = oldJson != null && newJson != null
            ? GenerateChangeSummary(oldJson, newJson)
            : null;

        await LogAsync(new AuditEntry
        {
            Action = action,
            EntityType = "Form",
            EntityId = formId,
            OldValues = changeSummary?.OldSummary,
            NewValues = changeSummary?.NewSummary,
            Metadata = new Dictionary<string, object>
            {
                ["description"] = description ?? "",
                ["change_stats"] = changeSummary?.Stats ?? new()
            },
            Severity = AuditSeverity.Info
        });
    }

    public async Task LogSecurityEventAsync(
        string action,
        string? entityId,
        string? details,
        AuditSeverity severity = AuditSeverity.Warning)
    {
        await LogAsync(new AuditEntry
        {
            Action = action,
            EntityType = "Security",
            EntityId = entityId,
            Metadata = new Dictionary<string, object>
            {
                ["details"] = details ?? ""
            },
            Severity = severity
        });

        // For critical security events, also trigger alerts
        if (severity >= AuditSeverity.Error)
        {
            _logger.LogError(
                "SECURITY ALERT: {Action} - Entity: {EntityId} - Details: {Details}",
                action, entityId, details);

            // TODO: Integration with alerting system (email, Slack, etc.)
        }
    }

    private string? GetClientIpAddress(HttpContext? context)
    {
        if (context == null) return null;

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private ChangeSummary? GenerateChangeSummary(string oldJson, string newJson)
    {
        // Generate a compact summary of changes for audit storage
        // Full JSON is stored in FormVersionHistory, not audit log
        try
        {
            using var oldDoc = JsonDocument.Parse(oldJson);
            using var newDoc = JsonDocument.Parse(newJson);

            var oldQuestions = CountQuestions(oldDoc);
            var newQuestions = CountQuestions(newDoc);

            return new ChangeSummary
            {
                OldSummary = $"Questions: {oldQuestions}",
                NewSummary = $"Questions: {newQuestions}",
                Stats = new Dictionary<string, object>
                {
                    ["questions_before"] = oldQuestions,
                    ["questions_after"] = newQuestions,
                    ["questions_delta"] = newQuestions - oldQuestions
                }
            };
        }
        catch
        {
            return null;
        }
    }

    private record ChangeSummary
    {
        public string? OldSummary { get; init; }
        public string? NewSummary { get; init; }
        public Dictionary<string, object> Stats { get; init; } = new();
    }
}
```

---

## 2. Session Management

### 2.1 ISessionService Interface

```csharp
namespace VisualEditorOpus.Services.Identity;

public interface ISessionService
{
    /// <summary>
    /// Create a new session for a user
    /// </summary>
    Task<UserSession> CreateSessionAsync(
        string userId,
        string? deviceInfo = null);

    /// <summary>
    /// Validate an existing session
    /// </summary>
    Task<SessionValidationResult> ValidateSessionAsync(string sessionId);

    /// <summary>
    /// Update session activity timestamp
    /// </summary>
    Task TouchSessionAsync(string sessionId);

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    Task RevokeSessionAsync(string sessionId, string reason);

    /// <summary>
    /// Revoke all sessions for a user (e.g., password change)
    /// </summary>
    Task RevokeAllUserSessionsAsync(string userId, string reason);

    /// <summary>
    /// Get all active sessions for a user
    /// </summary>
    Task<List<UserSession>> GetUserSessionsAsync(string userId);

    /// <summary>
    /// Clean up expired sessions
    /// </summary>
    Task CleanupExpiredSessionsAsync();
}

public record SessionValidationResult(
    bool IsValid,
    string? InvalidReason = null,
    UserSession? Session = null);
```

### 2.2 SessionService Implementation

```csharp
namespace VisualEditorOpus.Services.Identity;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IAuditService _auditService;
    private readonly SessionOptions _options;

    public SessionService(
        ApplicationDbContext db,
        IHttpContextAccessor httpContext,
        IAuditService auditService,
        IOptions<SessionOptions> options)
    {
        _db = db;
        _httpContext = httpContext;
        _auditService = auditService;
        _options = options.Value;
    }

    public async Task<UserSession> CreateSessionAsync(
        string userId,
        string? deviceInfo = null)
    {
        var httpContext = _httpContext.HttpContext;

        // Check concurrent session limit
        var activeSessions = await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .CountAsync();

        if (activeSessions >= _options.MaxConcurrentSessions)
        {
            // Revoke oldest session
            var oldestSession = await _db.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (oldestSession != null)
            {
                await RevokeSessionAsync(oldestSession.Id, "Max concurrent sessions exceeded");
            }
        }

        var session = new UserSession
        {
            Id = GenerateSecureSessionId(),
            UserId = userId,
            DeviceInfo = deviceInfo ?? httpContext?.Request.Headers.UserAgent.ToString(),
            IpAddress = GetClientIpAddress(httpContext),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_options.AbsoluteExpiration),
            IsRevoked = false
        };

        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync();

        await _auditService.LogAuthEventAsync(
            "session.created",
            userId,
            null,
            true);

        return session;
    }

    public async Task<SessionValidationResult> ValidateSessionAsync(string sessionId)
    {
        var session = await _db.UserSessions.FindAsync(sessionId);

        if (session == null)
        {
            return new SessionValidationResult(false, "Session not found");
        }

        if (session.IsRevoked)
        {
            return new SessionValidationResult(false, $"Session revoked: {session.RevokedReason}");
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            return new SessionValidationResult(false, "Session expired");
        }

        // Check sliding expiration
        var slidingExpiry = session.LastActivityAt.Add(_options.SlidingExpiration);
        if (slidingExpiry < DateTime.UtcNow)
        {
            return new SessionValidationResult(false, "Session inactive too long");
        }

        return new SessionValidationResult(true, Session: session);
    }

    public async Task TouchSessionAsync(string sessionId)
    {
        var session = await _db.UserSessions.FindAsync(sessionId);
        if (session != null && !session.IsRevoked)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task RevokeSessionAsync(string sessionId, string reason)
    {
        var session = await _db.UserSessions.FindAsync(sessionId);
        if (session != null && !session.IsRevoked)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason;
            await _db.SaveChangesAsync();

            await _auditService.LogAuthEventAsync(
                "session.revoked",
                session.UserId,
                null,
                true);
        }
    }

    public async Task RevokeAllUserSessionsAsync(string userId, string reason)
    {
        var sessions = await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason;
        }

        await _db.SaveChangesAsync();

        await _auditService.LogAuthEventAsync(
            "session.all_revoked",
            userId,
            null,
            true);
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7); // Keep revoked sessions for 7 days

        var expiredSessions = await _db.UserSessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || (s.IsRevoked && s.RevokedAt < cutoff))
            .ToListAsync();

        _db.UserSessions.RemoveRange(expiredSessions);
        await _db.SaveChangesAsync();
    }

    private static string GenerateSecureSessionId()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
    }
}

public class SessionOptions
{
    public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromHours(2);
    public int MaxConcurrentSessions { get; set; } = 5;
}
```

---

## 3. Password Policies

### 3.1 Secure Password Configuration

```csharp
// Program.cs configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // === PASSWORD REQUIREMENTS ===
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;          // NIST recommends 8+, we use 12
    options.Password.RequiredUniqueChars = 4;

    // === LOCKOUT SETTINGS ===
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // === USER SETTINGS ===
    options.User.RequireUniqueEmail = true;

    // === SIGN-IN SETTINGS ===
    options.SignIn.RequireConfirmedEmail = true;   // Require email verification
    options.SignIn.RequireConfirmedAccount = true;
});
```

### 3.2 Password History Validator

```csharp
namespace VisualEditorOpus.Services.Identity;

public class PasswordHistoryValidator : IPasswordValidator<ApplicationUser>
{
    private readonly ApplicationDbContext _db;
    private readonly int _historyCount;

    public PasswordHistoryValidator(
        ApplicationDbContext db,
        IOptions<PasswordHistoryOptions> options)
    {
        _db = db;
        _historyCount = options.Value.HistoryCount;
    }

    public async Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user,
        string? password)
    {
        if (string.IsNullOrEmpty(password))
            return IdentityResult.Success;

        // Get recent password hashes
        var recentPasswords = await _db.PasswordHistories
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(_historyCount)
            .ToListAsync();

        foreach (var historical in recentPasswords)
        {
            var result = manager.PasswordHasher.VerifyHashedPassword(
                user, historical.PasswordHash, password);

            if (result != PasswordVerificationResult.Failed)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordReused",
                    Description = $"You cannot reuse any of your last {_historyCount} passwords."
                });
            }
        }

        return IdentityResult.Success;
    }
}

public class PasswordHistoryOptions
{
    public int HistoryCount { get; set; } = 12;
}
```

---

## 4. Account Lockout Handling

### 4.1 Enhanced Login Flow

```csharp
namespace VisualEditorOpus.Services.Identity;

public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly ISessionService _sessionService;

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            // Don't reveal that user doesn't exist
            await _auditService.LogAuthEventAsync(
                "user.login_failed",
                null,
                request.Email,
                false,
                "Invalid credentials");

            return LoginResult.Failed("Invalid email or password.");
        }

        // Check if user is active
        var profile = await _db.UserProfiles.FindAsync(user.Id);
        if (profile?.IsActive != true)
        {
            await _auditService.LogAuthEventAsync(
                "user.login_failed",
                user.Id,
                user.Email,
                false,
                "Account deactivated");

            return LoginResult.Failed("Your account has been deactivated. Contact your administrator.");
        }

        // Check lockout
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

            await _auditService.LogSecurityEventAsync(
                "user.login_locked_out",
                user.Id,
                $"Account locked until {lockoutEnd}",
                AuditSeverity.Warning);

            return LoginResult.LockedOut(lockoutEnd?.UtcDateTime);
        }

        // Attempt sign in
        var result = await _signInManager.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Check if MFA is required
            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return LoginResult.RequiresMfa(user.Id);
            }

            // Create session
            var session = await _sessionService.CreateSessionAsync(
                user.Id,
                request.DeviceInfo);

            await _auditService.LogAuthEventAsync(
                "user.login_success",
                user.Id,
                user.Email,
                true);

            // Update last login
            if (profile != null)
            {
                profile.LastLoginAt = DateTime.UtcNow;
                profile.LastLoginIp = request.IpAddress;
                await _db.SaveChangesAsync();
            }

            return LoginResult.Success(session, CheckMustChangePassword(profile));
        }

        if (result.IsLockedOut)
        {
            await _auditService.LogSecurityEventAsync(
                "user.account_locked",
                user.Id,
                "Account locked due to failed login attempts",
                AuditSeverity.Warning);

            return LoginResult.LockedOut(null);
        }

        // Log failed attempt
        await _auditService.LogAuthEventAsync(
            "user.login_failed",
            user.Id,
            user.Email,
            false,
            "Invalid password");

        return LoginResult.Failed("Invalid email or password.");
    }

    private bool CheckMustChangePassword(UserProfile? profile)
    {
        if (profile == null) return false;

        // Force change if flag is set
        if (profile.MustChangePassword) return true;

        // Force change if password is too old (90 days)
        if (profile.PasswordChangedAt.HasValue)
        {
            var passwordAge = DateTime.UtcNow - profile.PasswordChangedAt.Value;
            if (passwordAge.TotalDays > 90) return true;
        }

        return false;
    }
}

public record LoginResult
{
    public bool IsSuccess { get; init; }
    public bool IsLockedOut { get; init; }
    public bool RequiresMfa { get; init; }
    public bool MustChangePassword { get; init; }
    public string? Error { get; init; }
    public UserSession? Session { get; init; }
    public DateTime? LockoutEnd { get; init; }
    public string? MfaUserId { get; init; }

    public static LoginResult Success(UserSession session, bool mustChangePassword) =>
        new() { IsSuccess = true, Session = session, MustChangePassword = mustChangePassword };

    public static LoginResult Failed(string error) =>
        new() { IsSuccess = false, Error = error };

    public static LoginResult LockedOut(DateTime? until) =>
        new() { IsSuccess = false, IsLockedOut = true, LockoutEnd = until };

    public static LoginResult RequiresMfa(string userId) =>
        new() { IsSuccess = false, RequiresMfa = true, MfaUserId = userId };
}
```

---

## 5. Security Headers & CSRF

### 5.1 Security Headers Middleware

```csharp
namespace VisualEditorOpus.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Prevent MIME sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Enable XSS filter
        headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer policy
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content Security Policy
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // Required for Blazor/SurveyJS
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' wss:; " + // WebSocket for Blazor Server
            "frame-ancestors 'none';";

        // Permissions policy
        headers["Permissions-Policy"] =
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
            "magnetometer=(), microphone=(), payment=(), usb=()";

        await _next(context);
    }
}

// Extension method
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
```

---

## 6. Audit Log Viewer Component

### 6.1 AuditLogViewer.razor

```razor
@page "/admin/audit-logs"
@attribute [Authorize(Policy = "OrgAdmin")]
@inject IAuditService AuditService

<h3>Audit Logs</h3>

<div class="filters">
    <input type="date" @bind="startDate" />
    <input type="date" @bind="endDate" />
    <select @bind="actionFilter">
        <option value="">All Actions</option>
        <option value="user.login">Logins</option>
        <option value="form.">Form Changes</option>
        <option value="security.">Security Events</option>
    </select>
    <button @onclick="LoadLogs">Search</button>
</div>

<table class="audit-table">
    <thead>
        <tr>
            <th>Timestamp</th>
            <th>User</th>
            <th>Action</th>
            <th>Entity</th>
            <th>IP Address</th>
            <th>Severity</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var log in logs)
        {
            <tr class="severity-@log.Severity">
                <td>@log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")</td>
                <td>@log.UserEmail</td>
                <td>@log.Action</td>
                <td>@log.EntityType:@log.EntityId</td>
                <td>@log.IpAddress</td>
                <td>@GetSeverityBadge(log.Severity)</td>
                <td>
                    <button @onclick="() => ShowDetails(log)">View</button>
                </td>
            </tr>
        }
    </tbody>
</table>

<Pagination CurrentPage="currentPage" TotalPages="totalPages" OnPageChanged="ChangePage" />

@code {
    private List<AuditLog> logs = new();
    private DateTime startDate = DateTime.UtcNow.AddDays(-7);
    private DateTime endDate = DateTime.UtcNow;
    private string actionFilter = "";
    private int currentPage = 1;
    private int totalPages = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadLogs();
    }

    private async Task LoadLogs()
    {
        var result = await AuditService.QueryAsync(new AuditQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Action = string.IsNullOrEmpty(actionFilter) ? null : actionFilter,
            Page = currentPage,
            PageSize = 50
        });

        logs = result.Items;
        totalPages = result.TotalPages;
    }

    private string GetSeverityBadge(byte severity) => severity switch
    {
        0 => "INFO",
        1 => "WARNING",
        2 => "ERROR",
        3 => "CRITICAL",
        _ => "UNKNOWN"
    };
}
```

---

## 7. Security Checklist

### 7.1 Authentication Security

- [ ] Strong password policy enforced (12+ chars, complexity)
- [ ] Account lockout after 5 failed attempts
- [ ] Password history prevents reuse of last 12 passwords
- [ ] Session timeout after 2 hours of inactivity
- [ ] Maximum 5 concurrent sessions per user
- [ ] Secure session ID generation (32 bytes, crypto random)
- [ ] Session invalidation on password change
- [ ] MFA support for sensitive operations

### 7.2 Authorization Security

- [ ] All endpoints require authentication by default
- [ ] Policy-based authorization for all operations
- [ ] Server-side validation of all form changes
- [ ] Resource ownership verified before access
- [ ] UI restrictions backed by server enforcement
- [ ] Audit log for all authorization failures

### 7.3 Data Security

- [ ] HTTPS enforced in production
- [ ] Sensitive data encrypted at rest
- [ ] Connection strings in secure configuration
- [ ] No secrets in source code or logs
- [ ] SQL injection prevented via parameterized queries
- [ ] XSS prevented via output encoding

### 7.4 Audit & Compliance

- [ ] All authentication events logged
- [ ] All authorization decisions logged
- [ ] All data modifications logged with before/after
- [ ] Security events trigger alerts
- [ ] Audit logs retained per policy
- [ ] Audit log integrity protected

---

## Next Document

Proceed to **06-Implementation-Phases.md** for the phased delivery plan.
