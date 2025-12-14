# Role-Based Access Control for Visual Editor Opus
# Part 6: Implementation Phases

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Phase Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        IMPLEMENTATION ROADMAP                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Phase 1: Identity Foundation                                               │
│  ════════════════════════                                                   │
│  • ASP.NET Core Identity setup                                              │
│  • Login/Logout/Password Reset                                              │
│  • Basic session management                                                 │
│  • Secure all routes                                                        │
│                                                                             │
│  Phase 2: Organization & User Management                                    │
│  ═══════════════════════════════════════                                    │
│  • Organization/Workspace entities                                          │
│  • User CRUD operations                                                     │
│  • Invitation system                                                        │
│  • Admin dashboard                                                          │
│                                                                             │
│  Phase 3: Permission System                                                 │
│  ═════════════════════════                                                  │
│  • Role templates                                                           │
│  • Form permission grants                                                   │
│  • Policy evaluator service                                                 │
│  • Authorization handlers                                                   │
│                                                                             │
│  Phase 4: SurveyJS Integration & Validation                                 │
│  ═══════════════════════════════════════════                                │
│  • Creator config generation                                                │
│  • JavaScript interop                                                       │
│  • Server-side form validation                                              │
│  • Change detection engine                                                  │
│                                                                             │
│  Phase 5: Audit & Compliance                                                │
│  ═══════════════════════════                                                │
│  • Audit logging service                                                    │
│  • Form version history                                                     │
│  • Audit log viewer                                                         │
│  • Security event alerts                                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Phase 1: Identity Foundation

### 2.1 Objectives
- Secure the application with authentication
- Implement login, logout, and password management
- Establish session management

### 2.2 Tasks

| Task | Description | Dependencies |
|------|-------------|--------------|
| **1.1** Install Identity packages | Add NuGet packages for Identity | None |
| **1.2** Create ApplicationDbContext | Extend IdentityDbContext with custom entities | 1.1 |
| **1.3** Configure Identity in Program.cs | Register services, configure options | 1.2 |
| **1.4** Create database migrations | Generate and run Identity migrations | 1.3 |
| **1.5** Implement Login page | Create Login.razor with form validation | 1.4 |
| **1.6** Implement Logout functionality | Sign out and clear session | 1.5 |
| **1.7** Implement Password Reset flow | Forgot password email + reset page | 1.5 |
| **1.8** Create CurrentUserService | Service to access current user context | 1.4 |
| **1.9** Implement SessionService | Session creation, validation, cleanup | 1.4 |
| **1.10** Secure all routes | Add [Authorize] to all admin pages | 1.5 |
| **1.11** Create Access Denied page | User-friendly unauthorized message | 1.10 |
| **1.12** Seed initial admin user | Create system admin via secure config | 1.4 |
| **1.13** Write unit tests | Test authentication flows | 1.5-1.9 |
| **1.14** Manual QA testing | End-to-end login/logout testing | 1.13 |

### 2.3 Deliverables
- [ ] Users can register (if enabled) and login
- [ ] Session persists across page refreshes
- [ ] Password reset via email works
- [ ] Unauthenticated users redirected to login
- [ ] Session expires after inactivity
- [ ] Initial admin user can login

### 2.4 Testing Checklist

```markdown
[ ] Login with valid credentials succeeds
[ ] Login with invalid credentials fails with generic message
[ ] Account locks after 5 failed attempts
[ ] Locked account shows appropriate message
[ ] Password reset email is sent
[ ] Password reset token works correctly
[ ] Session expires after configured timeout
[ ] Logout clears session completely
[ ] Protected routes redirect to login
[ ] Access denied page shows for insufficient permissions
```

---

## 3. Phase 2: Organization & User Management

### 3.1 Objectives
- Implement multi-tenant organization structure
- Enable admin user management
- Create invitation workflow

### 3.2 Tasks

| Task | Description | Dependencies |
|------|-------------|--------------|
| **2.1** Create Organization entity | DB table + EF Core entity | Phase 1 |
| **2.2** Create Workspace entity | DB table + EF Core entity | 2.1 |
| **2.3** Create OrganizationUser entity | Join table with role | 2.1 |
| **2.4** Create WorkspaceMember entity | Join table with role template | 2.2 |
| **2.5** Create UserProfile entity | Extended user data | Phase 1 |
| **2.6** Generate migrations | Create all new tables | 2.1-2.5 |
| **2.7** Implement IUserService | User CRUD operations | 2.6 |
| **2.8** Implement IOrganizationService | Org management | 2.6 |
| **2.9** Implement IWorkspaceService | Workspace CRUD | 2.6 |
| **2.10** Create Users list page | DataGrid with user list | 2.7 |
| **2.11** Create User edit modal | Edit user details and role | 2.10 |
| **2.12** Create Invitation entity | Invitation tracking table | 2.6 |
| **2.13** Implement IInvitationService | Create, send, accept invites | 2.12 |
| **2.14** Create Invite User modal | Email + role selection | 2.13 |
| **2.15** Create Accept Invitation page | Token validation + registration | 2.13 |
| **2.16** Create Workspace list page | List workspaces in org | 2.9 |
| **2.17** Create Workspace members UI | Manage workspace members | 2.16 |
| **2.18** Update navigation | Add admin menu items | 2.10, 2.16 |
| **2.19** Write unit tests | Test services | 2.7-2.13 |
| **2.20** Write integration tests | Test API flows | 2.19 |

### 3.3 Deliverables
- [ ] Organization admins can list all users
- [ ] Admins can create, edit, deactivate users
- [ ] Invitation emails are sent correctly
- [ ] Users can accept invitations and register
- [ ] Workspaces can be created and managed
- [ ] Members can be added to workspaces

### 3.4 Testing Checklist

```markdown
[ ] Admin can view list of all users in organization
[ ] Admin can create new user with role
[ ] Admin can deactivate a user
[ ] Deactivated user cannot login
[ ] Invitation email is sent with correct link
[ ] Invitation expires after 7 days
[ ] User can register via invitation
[ ] Workspace can be created
[ ] Members can be added to workspace
[ ] Member roles can be changed
```

---

## 4. Phase 3: Permission System

### 4.1 Objectives
- Implement role templates with permissions
- Create form-level permission grants
- Build policy evaluation engine

### 4.2 Tasks

| Task | Description | Dependencies |
|------|-------------|--------------|
| **3.1** Create RoleTemplate entity | Predefined role definitions | Phase 2 |
| **3.2** Create FormPermission entity | Form access grants | Phase 2 |
| **3.3** Generate migrations | Add permission tables | 3.1-3.2 |
| **3.4** Seed default role templates | Designer, Data Manager, etc. | 3.3 |
| **3.5** Implement IRoleTemplateService | Role template CRUD | 3.4 |
| **3.6** Implement IFormPermissionRepository | Permission data access | 3.3 |
| **3.7** Implement IFormPolicyEvaluator | Permission resolution logic | 3.6 |
| **3.8** Create authorization handlers | ASP.NET Core handlers | 3.7 |
| **3.9** Register authorization policies | Configure in Program.cs | 3.8 |
| **3.10** Update FormModuleSchema | Add WorkspaceId, CreatedBy | Phase 2 |
| **3.11** Migrate existing forms | Assign to default workspace | 3.10 |
| **3.12** Update form list queries | Filter by permissions | 3.7 |
| **3.13** Create Form Sharing modal | Grant access to users | 3.6 |
| **3.14** Create Role Templates admin page | View/edit role templates | 3.5 |
| **3.15** Write policy evaluator tests | Test permission resolution | 3.7 |
| **3.16** Write integration tests | Test end-to-end access | 3.15 |

### 4.3 Deliverables
- [ ] Default role templates are seeded
- [ ] Forms are assigned to workspaces
- [ ] Users see only forms they have access to
- [ ] Form sharing modal works
- [ ] Permission levels are correctly evaluated
- [ ] Authorization handlers enforce policies

### 4.4 Testing Checklist

```markdown
[ ] Workspace owner sees all workspace forms
[ ] Form Designer sees forms they created
[ ] Data Manager cannot see form design tab options
[ ] Reviewer cannot make any changes
[ ] User with explicit deny cannot access form
[ ] Sharing a form grants correct access
[ ] Expiring permissions expire correctly
[ ] System admin can access all forms
[ ] Org admin can access all org forms
```

---

## 5. Phase 4: SurveyJS Integration & Validation

### 5.1 Objectives
- Map permissions to SurveyJS Creator config
- Implement client-side restrictions
- Build server-side validation engine

### 5.2 Tasks

| Task | Description | Dependencies |
|------|-------------|--------------|
| **4.1** Define CreatorConfig record | C# config object | Phase 3 |
| **4.2** Implement config generation | Build config from permissions | 4.1, Phase 3 |
| **4.3** Create editor-security.js | JavaScript permission handler | 4.1 |
| **4.4** Update Editor.razor | Inject permissions | 4.2 |
| **4.5** Implement JS interop | Pass config to JavaScript | 4.4 |
| **4.6** Apply onElementAllowOperations | Restrict element actions | 4.3 |
| **4.7** Implement toolbox filtering | Hide restricted question types | 4.3 |
| **4.8** Implement IFormValidationService | Server-side validation | Phase 3 |
| **4.9** Implement JSON diff engine | Detect change types | 4.8 |
| **4.10** Implement dangerous content detection | XSS prevention | 4.8 |
| **4.11** Update save endpoint | Validate before saving | 4.8 |
| **4.12** Return validation errors | Show errors to user | 4.11 |
| **4.13** Implement form version tracking | Save versions on change | 4.11 |
| **4.14** Write validation tests | Test all change types | 4.8-4.10 |
| **4.15** Write JS tests | Test client restrictions | 4.3-4.7 |
| **4.16** Security testing | Attempt bypasses | 4.14-4.15 |

### 5.3 Deliverables
- [ ] Creator config reflects user permissions
- [ ] UI hides unauthorized features
- [ ] Server rejects unauthorized changes
- [ ] XSS attempts are blocked
- [ ] Form versions are saved
- [ ] Validation errors display correctly

### 5.4 Testing Checklist

```markdown
[ ] Designer cannot see JSON tab (if restricted)
[ ] Designer cannot see Logic tab (if restricted)
[ ] Viewer sees read-only editor
[ ] Add question button hidden when restricted
[ ] Server rejects structure change from text-only user
[ ] Server rejects logic change from no-logic user
[ ] XSS via HTML question type is detected
[ ] Form version is created on save
[ ] DevTools bypass attempt is blocked by server
```

---

## 6. Phase 5: Audit & Compliance

### 6.1 Objectives
- Implement comprehensive audit logging
- Create audit viewer for admins
- Add security event alerting

### 6.2 Tasks

| Task | Description | Dependencies |
|------|-------------|--------------|
| **5.1** Create AuditLog entity | Audit storage table | Phase 4 |
| **5.2** Create FormVersionHistory entity | Form version storage | Phase 4 |
| **5.3** Generate migrations | Add audit tables | 5.1-5.2 |
| **5.4** Implement IAuditService | Audit logging service | 5.3 |
| **5.5** Add auth event logging | Login, logout, failures | 5.4 |
| **5.6** Add form change logging | Create, update, delete | 5.4 |
| **5.7** Add security event logging | Unauthorized attempts | 5.4 |
| **5.8** Add permission change logging | Role assignments, grants | 5.4 |
| **5.9** Create Audit Log viewer page | Admin searchable list | 5.4 |
| **5.10** Add export functionality | Export logs to CSV | 5.9 |
| **5.11** Implement form version viewer | View/restore versions | 5.2 |
| **5.12** Add security headers middleware | CSP, X-Frame-Options | Phase 4 |
| **5.13** Implement alert service | Email/Slack for critical events | 5.7 |
| **5.14** Create security dashboard | Summary of events | 5.9 |
| **5.15** Write audit service tests | Verify logging | 5.4-5.8 |
| **5.16** Final security review | Penetration testing | All |

### 6.3 Deliverables
- [ ] All auth events are logged
- [ ] All form changes are logged
- [ ] Security events trigger alerts
- [ ] Admin can search audit logs
- [ ] Audit logs can be exported
- [ ] Form versions can be viewed/restored

### 6.4 Testing Checklist

```markdown
[ ] Login success creates audit entry
[ ] Login failure creates audit entry
[ ] Form create creates audit entry
[ ] Form update creates audit entry with diff
[ ] Unauthorized access attempt is logged
[ ] Critical events trigger email alert
[ ] Audit log search returns correct results
[ ] Audit log export works
[ ] Form version history shows all changes
[ ] Form can be restored to previous version
```

---

## 7. Dependencies & Risk Mitigation

### 7.1 Technical Dependencies

```
Phase 1 ──┬──► Phase 2 ──┬──► Phase 3 ──┬──► Phase 4 ──► Phase 5
          │              │              │
          │              │              └── Requires permission
          │              │                  system for validation
          │              │
          │              └── Requires Identity
          │                  for user management
          │
          └── Foundation for everything
```

### 7.2 Risk Matrix

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Identity configuration issues | High | Medium | Follow MS best practices, use proven patterns |
| Permission logic bugs | High | Medium | Comprehensive unit tests, security review |
| SurveyJS version incompatibility | Medium | Low | Pin versions, test thoroughly |
| Performance with large audit logs | Medium | Medium | Implement pagination, archiving strategy |
| Migration data loss | High | Low | Backup before migration, test on staging |

### 7.3 Rollback Strategy

Each phase should be deployable independently with rollback capability:

1. **Database migrations**: Use EF Core migrations with down scripts
2. **Feature flags**: Wrap new features in configuration flags
3. **Staged rollout**: Deploy to staging first, then production
4. **Backup strategy**: Full database backup before each phase

---

## 8. Quality Gates

### 8.1 Phase Completion Criteria

Each phase must meet these criteria before moving to the next:

- [ ] All tasks completed
- [ ] Unit test coverage > 80% for new code
- [ ] Integration tests pass
- [ ] Manual QA checklist complete
- [ ] No critical/high severity bugs
- [ ] Performance acceptable (response time < 200ms)
- [ ] Security review complete
- [ ] Documentation updated

### 8.2 Definition of Done

A task is complete when:
- Code is written and reviewed
- Unit tests are written and passing
- Integration tests are written and passing
- Documentation is updated
- No linting/build warnings
- Deployed to staging and verified

---

## 9. Post-Implementation

### 9.1 Phase 6: Future Enhancements (Optional)

| Feature | Description | Priority |
|---------|-------------|----------|
| **SSO/SAML** | Azure AD, Okta integration | High |
| **MFA** | TOTP authenticator support | High |
| **Field-level permissions** | Control access to specific fields | Medium |
| **API keys** | Service account authentication | Medium |
| **Bulk operations** | Import/export users via CSV | Low |
| **Advanced analytics** | Usage dashboards | Low |

### 9.2 Maintenance Tasks

- Weekly: Review security event logs
- Monthly: Audit log cleanup (archive old entries)
- Quarterly: Security review and penetration testing
- Annually: Permission audit (review who has access to what)

---

## Next Document

Proceed to **07-AI-Prompts.md** for ready-to-use implementation prompts.
