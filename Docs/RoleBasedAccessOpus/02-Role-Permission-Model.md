# Role-Based Access Control for Visual Editor Opus
# Part 2: Role & Permission Model

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Role Hierarchy

### 1.1 System-Level Roles

These roles operate across the entire platform.

| Role | Description | Scope |
|------|-------------|-------|
| **System Administrator** | Platform superuser. Manages organizations, system settings, and has emergency access to all resources. | Global |
| **Organization Admin** | Manages a single organization's users, workspaces, and settings. Cannot access other organizations. | Organization |

### 1.2 Workspace-Level Roles

These roles operate within a specific workspace (collection of forms).

| Role | Description | Form Design | Data Access | Member Management |
|------|-------------|-------------|-------------|-------------------|
| **Workspace Owner** | Full control of workspace. Multiple owners allowed per workspace. | Full | Full | Full |
| **Form Designer** | Creates and edits forms. Cannot view submission data. | Full | None | None |
| **Data Manager** | Views and manages form submissions. Cannot modify form design. | View Only | Full | None |
| **Reviewer** | Can view forms and data but cannot modify anything. | View Only | View Only | None |

### 1.3 Role Inheritance

```
System Administrator
    └── Can impersonate any Organization Admin (with audit log)

Organization Admin
    └── Automatically becomes Workspace Owner for all workspaces in their org

Workspace Owner
    └── Full permissions within their workspace only
```

---

## 2. Permission Definitions

### 2.1 Platform Permissions

| Permission Key | Description | Granted To |
|----------------|-------------|------------|
| `system.manage_organizations` | Create, edit, delete organizations | System Admin |
| `system.view_audit_logs` | View system-wide audit logs | System Admin |
| `system.manage_system_settings` | Configure platform settings | System Admin |
| `org.manage_users` | Invite, edit, deactivate users in organization | Org Admin |
| `org.manage_workspaces` | Create, delete workspaces | Org Admin |
| `org.view_org_audit` | View organization's audit logs | Org Admin |
| `workspace.manage_members` | Add/remove workspace members, assign roles | Workspace Owner |
| `workspace.delete` | Delete the entire workspace | Workspace Owner |
| `workspace.settings` | Edit workspace name, description | Workspace Owner |

### 2.2 Form Permissions

| Permission Key | Description | Designer | Data Manager | Reviewer |
|----------------|-------------|----------|--------------|----------|
| `form.create` | Create new forms | Yes | No | No |
| `form.edit_structure` | Add/remove/reorder questions | Yes | No | No |
| `form.edit_text` | Modify labels, descriptions, help text | Yes | No | No |
| `form.edit_logic` | Modify visibility rules, skip logic | Yes | No | No |
| `form.edit_validation` | Modify validation rules | Yes | No | No |
| `form.edit_theme` | Modify form styling | Yes | No | No |
| `form.edit_json` | Direct JSON editing | Yes | No | No |
| `form.delete` | Delete form definition | Yes | No | No |
| `form.publish` | Change form status to published | Yes | No | No |
| `form.view_design` | View form in editor (read-only) | Yes | Yes | Yes |
| `form.export_design` | Export form as JSON | Yes | No | No |
| `form.duplicate` | Create copy of form | Yes | No | No |

### 2.3 Data Permissions

| Permission Key | Description | Designer | Data Manager | Reviewer |
|----------------|-------------|----------|--------------|----------|
| `data.view_submissions` | View form responses | No | Yes | Yes |
| `data.export_submissions` | Export responses to CSV/Excel | No | Yes | No |
| `data.edit_submissions` | Modify submitted data | No | Yes | No |
| `data.delete_submissions` | Delete individual submissions | No | Yes | No |
| `data.view_analytics` | View response analytics/charts | No | Yes | Yes |

---

## 3. Form Access Model

### 3.1 Access Grant Types

Unlike simple "OwnerId" ownership, we use a flexible permission grant system:

```
FormPermission
├── FormId (which form)
├── PrincipalId (who - user or role)
├── PrincipalType (User, Role, or Organization)
├── PermissionLevel (what they can do)
├── GrantedBy (who granted this)
├── GrantedAt (when)
└── ExpiresAt (optional expiration)
```

### 3.2 Permission Levels

| Level | Name | Capabilities |
|-------|------|--------------|
| 0 | `None` | No access (explicit deny) |
| 10 | `View` | Can view form design only |
| 20 | `ViewData` | Can view form design + submissions |
| 30 | `EditData` | Can view + edit submissions |
| 40 | `Edit` | Can edit form design (not data) |
| 50 | `EditAll` | Can edit form + manage submissions |
| 60 | `Admin` | Full control including sharing |

### 3.3 Permission Resolution

When checking access, evaluate in this order:

```
1. Explicit User Deny → DENY
2. User Grant → Use highest level granted
3. Role Grant (via user's workspace role) → Use highest level
4. Workspace Default → Apply workspace-level default
5. No Grant Found → DENY
```

### 3.4 Example Scenarios

**Scenario 1: Designer shares form with Data Manager**
```
FormId: "covid-intake-form"
Grants:
  - User: alice@health.gov, Level: Admin (creator)
  - User: bob@health.gov, Level: EditData (data manager)
  - Role: "Reviewer", Level: ViewData (all reviewers)
```

**Scenario 2: Temporary contractor access**
```
FormId: "budget-form"
Grants:
  - User: contractor@external.com, Level: View, ExpiresAt: 2025-03-01
```

---

## 4. SurveyJS Creator Mapping

### 4.1 Permission → UI Configuration

| Permission Level | SurveyJS Configuration |
|------------------|------------------------|
| `View` / `ViewData` | `readOnly: true`, `toolboxLocation: "none"` |
| `EditData` | `readOnly: true` (form design locked) |
| `Edit` / `EditAll` | Based on granular permissions below |
| `Admin` | Full access, all features enabled |

### 4.2 Granular Editor Restrictions

For `Edit` level users, apply additional restrictions based on role configuration:

```javascript
// CreatorConfig from server
{
    "readOnly": false,
    "showJSONEditorTab": false,      // Only for Admin
    "showLogicTab": true,
    "showThemeTab": true,
    "showPreviewTab": true,
    "allowAddQuestions": true,
    "allowDeleteQuestions": true,
    "allowDragDrop": true,
    "allowChangeType": false,        // Prevent type changes
    "restrictedQuestionTypes": [],   // Empty = all allowed
    "hiddenToolboxItems": ["html", "expression"]  // Security: hide script-capable types
}
```

### 4.3 onElementAllowOperations Mapping

```javascript
creator.onElementAllowOperations.add((_, options) => {
    const config = window.creatorConfig;

    options.allowDelete = config.allowDeleteQuestions;
    options.allowCopy = config.allowAddQuestions;  // Copy creates new
    options.allowDrag = config.allowDragDrop;
    options.allowChangeType = config.allowChangeType;
    options.allowAddToToolbox = false;  // Always disabled for security

    // Lock specific elements if needed
    if (options.obj?.getPropertyValue("isLocked")) {
        options.allowDelete = false;
        options.allowDrag = false;
        options.allowEdit = false;
    }
});
```

---

## 5. Default Role Templates

### 5.1 Pre-configured Role Templates

Organizations can use these templates or create custom roles:

**Template: "Form Builder"**
```json
{
    "name": "Form Builder",
    "permissions": {
        "form.create": true,
        "form.edit_structure": true,
        "form.edit_text": true,
        "form.edit_logic": true,
        "form.edit_validation": true,
        "form.edit_theme": true,
        "form.edit_json": false,
        "form.delete": true,
        "form.publish": true,
        "data.view_submissions": false
    },
    "surveyJsConfig": {
        "showJSONEditorTab": false,
        "hiddenToolboxItems": ["html"]
    }
}
```

**Template: "Data Analyst"**
```json
{
    "name": "Data Analyst",
    "permissions": {
        "form.view_design": true,
        "data.view_submissions": true,
        "data.export_submissions": true,
        "data.view_analytics": true
    },
    "surveyJsConfig": {
        "readOnly": true
    }
}
```

**Template: "Power User"**
```json
{
    "name": "Power User",
    "permissions": {
        "form.*": true,
        "data.*": true
    },
    "surveyJsConfig": {
        "showJSONEditorTab": true,
        "allowChangeType": true
    }
}
```

---

## 6. Permission Inheritance & Override

### 6.1 Workspace Defaults

Each workspace can define default permissions for new forms:

```json
{
    "workspaceId": "ws-123",
    "defaultFormPermissions": {
        "Designer": "Edit",
        "DataManager": "EditData",
        "Reviewer": "ViewData"
    },
    "autoGrantCreator": "Admin"  // Form creator gets Admin
}
```

### 6.2 Form-Level Overrides

Individual forms can override workspace defaults:

```json
{
    "formId": "form-456",
    "inheritWorkspacePermissions": true,
    "overrides": [
        { "userId": "user-789", "level": "Admin" },  // Additional admin
        { "userId": "user-blocked", "level": "None" }  // Explicit deny
    ]
}
```

---

## 7. User Stories

### 7.1 System Administrator
> "As a System Administrator, I need to create new organizations for client agencies and assign their initial Organization Admins, so each agency can manage their own users independently."

### 7.2 Organization Admin
> "As an Organization Admin, I need to invite team members via email and assign them to appropriate workspaces with the right roles, so they can start working without delay."

### 7.3 Form Designer
> "As a Form Designer, I need to create and edit survey forms without seeing any response data, so I can focus on form quality without privacy concerns about citizen data."

### 7.4 Data Manager
> "As a Data Manager, I need to view and export form submissions without being able to modify the form structure, so I can process data without risk of breaking the form."

### 7.5 Reviewer
> "As a Reviewer, I need read-only access to both form designs and submission data, so I can audit work without accidentally making changes."

### 7.6 Temporary Access
> "As a Workspace Owner, I need to grant a contractor temporary access to a specific form that automatically expires after their engagement ends."

---

## Next Document

Proceed to **03-Database-Schema.md** for the complete SQL schema implementing this model.
