# Role-Based Access Control (RBAC) for Visual Editor Opus
# Part 1: Executive Summary & Industry Research

**Document Version:** 2.0
**Date:** December 2025
**Target Platform:** Visual Editor Opus (Blazor Server / .NET 9)
**Compliance Target:** NIST 800-53, SOC 2

---

## 1. Executive Summary

### 1.1 Purpose

This document defines a comprehensive **Role-Based Access Control (RBAC)** and **Permission Management** system for Visual Editor Opus. The current application operates without authentication or authorization. This plan transforms it into an enterprise/government-grade solution with:

- Secure user authentication with MFA support
- Granular permission control at both platform and form levels
- Complete audit trail for compliance
- Flexible form sharing and collaboration model
- Server-side validation to prevent client-side bypasses

### 1.2 Business Drivers

| Driver | Impact |
|--------|--------|
| **Security Compliance** | Government agencies (NIST 800-53) require strict access controls, audit logs, and data isolation |
| **Data Protection** | Prevent unauthorized access to form designs and submission data |
| **Workflow Integrity** | Ensure users can only perform actions appropriate to their role |
| **Collaboration** | Enable teams to share forms with controlled permissions |
| **Accountability** | Track who changed what and when for compliance audits |

### 1.3 Current State Analysis

Based on analysis of `Program.cs` and the codebase:

```csharp
// Current state - NO security
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
// Missing: UseAuthentication(), UseAuthorization()
```

| Capability | Current Status | Required |
|------------|----------------|----------|
| Authentication | None | Login, MFA, Password Reset |
| Authorization | None | Role-based + Resource-based policies |
| User Management | None | CRUD, Invitation, Status management |
| Form Ownership | None | Creator tracking, Sharing model |
| Audit Logging | None | All actions logged with timestamps |
| Session Management | None | Timeout, Concurrent limits |

---

## 2. Industry Research: Competitor Analysis

### 2.1 SurveyJS Creator

**Architecture:** Client-side JavaScript library with NO built-in security.

**Key Finding:** SurveyJS provides UI customization hooks but **zero server-side enforcement**. All security must be implemented in the backend.

#### Available UI Restriction APIs

| API | Purpose | Server Enforcement Needed |
|-----|---------|---------------------------|
| `showJSONEditorTab` | Hide/show JSON tab | Yes - validate JSON changes server-side |
| `showLogicTab` | Hide/show Logic tab | Yes - validate logic changes |
| `showThemeTab` | Hide/show Theme tab | Yes - validate theme changes |
| `readOnly` | Lock entire editor | Yes - reject all modifications |
| `onElementAllowOperations` | Control per-element actions | Yes - validate element changes |
| `questionTypes[]` | Limit available question types | Yes - validate question types in schema |

**Critical Insight:** These are **cosmetic restrictions only**. A malicious user can bypass them via browser DevTools. Server-side validation is mandatory.

#### onElementAllowOperations Properties

```javascript
creator.onElementAllowOperations.add((_, options) => {
    options.allowDelete = false;      // Hide delete button
    options.allowCopy = false;        // Hide copy button
    options.allowDrag = false;        // Disable drag/drop
    options.allowEdit = false;        // Disable inline editing
    options.allowChangeType = false;  // Prevent type changes
    options.allowChangeRequired = false;
    options.allowExpandCollapse = true;
    options.allowShowSettings = false; // Hide property grid access
});
```

**Source:** [SurveyJS Customize Survey Creation](https://surveyjs.io/survey-creator/documentation/customize-survey-creation-process)

---

### 2.2 JotForm Enterprise

**Architecture:** SaaS with Teams/Workspaces model.

#### Role Hierarchy

| Role | Form Design | View Submissions | Manage Submissions | Manage Team |
|------|-------------|------------------|-------------------|-------------|
| **Team Admin** | Full | Full | Full | Full |
| **Creator** | Full | None | None | None |
| **Data Collaborator** | Full | Full | Full | None |
| **Data Viewer** | None | Full | None | None |

**Key Insight:** JotForm separates **form editing** from **data access**. A "Creator" can build forms but cannot see responses. This is valuable for scenarios where form designers shouldn't access sensitive submission data.

#### Team Features

- Multiple admins per team (no single point of failure)
- Privacy settings (team-visible vs. admin-only)
- Activity logging by date/user/type
- Asset organization within workspaces

**Source:** [JotForm Team Roles](https://www.jotform.com/help/how-to-give-roles-to-team-members/)

---

### 2.3 Typeform

**Architecture:** Organization > Workspaces > Forms hierarchy.

#### Role Model

| Level | Role | Capabilities |
|-------|------|--------------|
| **Organization** | Admin | Billing, all workspaces, user management |
| **Organization** | Editor | Access to assigned workspaces |
| **Organization** | Viewer | View-only across organization |
| **Workspace** | Owner | Full control, member management |
| **Workspace** | Can Edit | Create, edit, publish forms |
| **Workspace** | Can View | View forms and results only |

**Key Insights:**
1. **Multi-owner workspaces** - Prevents single point of failure
2. **Default to least privilege** - New members get "Can View" by default
3. **Separation of concerns** - Org-level vs. Workspace-level permissions

**Source:** [Typeform Workspace Roles](https://help.typeform.com/hc/en-us/articles/9500648519316-Workspace-roles-explained)

---

### 2.4 Zoho Forms

**Architecture:** Enterprise with granular field-level permissions.

#### Notable Features

- **Field-level permissions**: Control which fields specific users can view/edit
- **IP restrictions**: Limit access by network
- **Regional data centers**: Data residency compliance
- **SSO/SAML**: Enterprise identity integration
- **Audit logs**: Complete action history

**Source:** [Zapier Form Builder Comparison](https://zapier.com/blog/best-online-form-builder-software/)

---

## 3. Comparative Analysis

### 3.1 Feature Matrix

| Feature | SurveyJS | JotForm | Typeform | Zoho | **Opus (Target)** |
|---------|----------|---------|----------|------|-------------------|
| Built-in Auth | No | Yes | Yes | Yes | **Yes** |
| Role-Based Access | No | Yes | Yes | Yes | **Yes** |
| Form Sharing | No | Yes | Yes | Yes | **Yes** |
| Field-Level Permissions | No | No | No | Yes | **Phase 2** |
| Audit Logging | No | Yes | Limited | Yes | **Yes** |
| MFA Support | No | Yes | Yes | Yes | **Yes** |
| SSO/SAML | No | Enterprise | Enterprise | Yes | **Phase 2** |
| Data/Design Separation | N/A | Yes | No | Yes | **Yes** |

### 3.2 Recommended Model for Opus

Based on analysis, we recommend a **hybrid model** combining:

1. **JotForm's Role Separation**: Separate form design permissions from data access
2. **Typeform's Workspace Model**: Organize forms into shareable workspaces
3. **Zoho's Audit Approach**: Comprehensive logging for compliance
4. **Custom Server Validation**: Compensate for SurveyJS's client-only restrictions

---

## 4. Strategic Architecture Decision

### 4.1 Chosen Model: Resource-Based + Role-Based Hybrid

```
[System Level]
    └── System Administrator
         │
         ├── [Organization A] (e.g., "Dept of Health")
         │      ├── Organization Admin
         │      │
         │      ├── [Workspace: COVID Forms]
         │      │      ├── Workspace Owner (multiple allowed)
         │      │      ├── Form Designer (creates/edits forms)
         │      │      ├── Data Manager (views/manages submissions)
         │      │      └── Viewer (read-only)
         │      │
         │      └── [Workspace: HR Forms]
         │             └── ...
         │
         └── [Organization B] (e.g., "City Hall")
                └── ...
```

### 4.2 Key Design Principles

1. **Defense in Depth**: UI restrictions + API validation + Database constraints
2. **Least Privilege**: Default to minimal access, explicitly grant more
3. **Audit Everything**: Every state change is logged with context
4. **Fail Secure**: Deny access on any error or ambiguity
5. **No Single Point of Failure**: Multiple owners/admins per resource

---

## 5. Document Structure

This plan is organized into the following documents:

| Document | Purpose |
|----------|---------|
| **01-Executive-Summary.md** | This document - Overview and research |
| **02-Role-Permission-Model.md** | Detailed role definitions and permission matrix |
| **03-Database-Schema.md** | Complete SQL schema with migrations |
| **04-Technical-Architecture.md** | Services, validation, and security patterns |
| **05-Security-Audit.md** | Audit logging, session management, compliance |
| **06-Implementation-Phases.md** | Phased delivery plan with dependencies |
| **07-AI-Prompts.md** | Ready-to-use prompts for implementation |

---

## Next Document

Proceed to **02-Role-Permission-Model.md** for detailed role definitions and the permission matrix.
