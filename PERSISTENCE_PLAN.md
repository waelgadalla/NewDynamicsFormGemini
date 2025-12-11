# Persistence Layer Implementation Plan
## Saving Form and Workflow Schemas to LocalDB SQL Database

---

## 1. CURRENT STATE ANALYSIS

### 1.1 What Exists Today

#### Data Models (Ready to Use)
| Model | Location | Purpose |
|-------|----------|---------|
| `FormModuleSchema` | `DynamicForms.Core.V4/Schemas/` | Immutable form definition with fields, validations |
| `FormFieldSchema` | `DynamicForms.Core.V4/Schemas/` | Individual field definition |
| `FormWorkflowSchema` | `DynamicForms.Core.V4/Schemas/` | Multi-module workflow orchestration |
| `FormModule` | `DynamicForms.Core/Entities/` | Enhanced form module (used by SQL Server repo) |
| `FormField` | `DynamicForms.Core/Entities/` | Enhanced field with hierarchy support |

#### Repository Infrastructure (Ready but NOT Wired)
| Component | Location | Status |
|-----------|----------|--------|
| `IFormModuleRepository` | `DynamicForms.Core.V4/Services/` | Interface defined |
| `SqlServerFormModuleRepository` | `DynamicForms.SqlServer/Repositories/` | **Fully implemented** using Dapper |
| `SqlServerFormDataRepository` | `DynamicForms.SqlServer/Repositories/` | **Fully implemented** using Dapper |
| `ServiceCollectionExtensions` | `DynamicForms.SqlServer/Extensions/` | DI registration helpers ready |

#### Current VisualEditorOpus State
- **NO database persistence** - all data is in-memory
- **NO connection string** configured in `appsettings.json`
- **NO reference** to `DynamicForms.SqlServer` project
- Services registered are all in-memory implementations

### 1.2 Database Schema Required

The SQL Server repositories expect these tables:

```sql
-- Form/Module Schema Storage
CREATE TABLE ModuleSchemas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ModuleId INT NOT NULL,
    OpportunityId INT NULL,
    Version FLOAT NOT NULL DEFAULT 1.0,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- JSON serialized FormModule
    IsActive BIT NOT NULL DEFAULT 1,
    IsCurrent BIT NOT NULL DEFAULT 1,
    DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DateUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,

    INDEX IX_ModuleSchemas_ModuleId (ModuleId),
    INDEX IX_ModuleSchemas_OpportunityId (OpportunityId),
    INDEX IX_ModuleSchemas_Current (ModuleId, OpportunityId, IsCurrent) WHERE IsActive = 1
);

-- Form Submissions (for form data, not schemas)
CREATE TABLE FormSubmissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL UNIQUE,
    OpportunityId INT NOT NULL,
    ModuleId INT NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    Language NVARCHAR(10) NOT NULL DEFAULT 'EN',
    IsComplete BIT NOT NULL DEFAULT 0,
    IsValid BIT NOT NULL DEFAULT 0,
    SchemaVersion FLOAT NULL,
    DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DateUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DateSubmitted DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    INDEX IX_FormSubmissions_OpportunityId (OpportunityId),
    INDEX IX_FormSubmissions_ModuleId (ModuleId)
);

-- Field Data Storage
CREATE TABLE FieldData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL,
    FieldId NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    JsonValue NVARCHAR(MAX) NULL,  -- For complex values (arrays, objects)
    DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DateUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,

    INDEX IX_FieldData_ApplicationId (ApplicationId),
    CONSTRAINT FK_FieldData_FormSubmissions FOREIGN KEY (ApplicationId)
        REFERENCES FormSubmissions(ApplicationId) ON DELETE CASCADE
);

-- Modal/Popup Data Storage
CREATE TABLE ModalData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL,
    ModalId NVARCHAR(100) NOT NULL,
    RecordId NVARCHAR(100) NOT NULL,
    FormData NVARCHAR(MAX) NOT NULL,  -- JSON serialized modal records
    [Order] INT NOT NULL DEFAULT 0,
    DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,

    INDEX IX_ModalData_ApplicationId (ApplicationId),
    INDEX IX_ModalData_ModalId (ApplicationId, ModalId)
);

-- NEW: Workflow Schema Storage (needs to be added)
CREATE TABLE WorkflowSchemas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    OpportunityId INT NULL,
    Version FLOAT NOT NULL DEFAULT 1.0,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- JSON serialized FormWorkflowSchema
    IsActive BIT NOT NULL DEFAULT 1,
    IsCurrent BIT NOT NULL DEFAULT 1,
    DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DateUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    TitleEn NVARCHAR(200) NULL,
    TitleFr NVARCHAR(200) NULL,

    INDEX IX_WorkflowSchemas_WorkflowId (WorkflowId),
    INDEX IX_WorkflowSchemas_OpportunityId (OpportunityId),
    INDEX IX_WorkflowSchemas_Current (WorkflowId, OpportunityId, IsCurrent) WHERE IsActive = 1
);

-- Table-Valued Parameter Type for bulk operations
CREATE TYPE ModuleDataTableType AS TABLE (
    ModuleId INT,
    OpportunityId INT,
    Version FLOAT,
    SchemaJson NVARCHAR(MAX)
);
```

---

## 2. IMPLEMENTATION PLAN

### Phase 1: Infrastructure Setup

#### 1.1 Add Project Reference
**File:** `VisualEditorOpus.csproj`
```xml
<ItemGroup>
  <ProjectReference Include="..\DynamicForms.Core.V4\DynamicForms.Core.V4.csproj" />
  <ProjectReference Include="..\DynamicForms.SqlServer\DynamicForms.SqlServer.csproj" />
</ItemGroup>
```

#### 1.2 Add Connection String
**File:** `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DynamicFormsDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 1.3 Register SQL Server Services
**File:** `Program.cs`
```csharp
using DynamicForms.SqlServer.Extensions;

// After existing service registrations, add:
builder.Services.AddDynamicFormsSqlServer(builder.Configuration);

// Optional: Add health checks
builder.Services.AddDynamicFormsSqlServerMonitoring(builder.Configuration);
```

### Phase 2: Database Creation

#### 2.1 Create LocalDB Database
Run in Package Manager Console or SQL Server Object Explorer:
```sql
CREATE DATABASE DynamicFormsDB;
```

#### 2.2 Execute Schema Script
Run the full schema script from Section 1.2 above to create all tables.

### Phase 3: Workflow Repository (New Development Required)

#### 3.1 Create Interface
**File:** `DynamicForms.Core.V4/Services/IWorkflowRepository.cs`
```csharp
public interface IWorkflowRepository
{
    Task<bool> SaveAsync(FormWorkflowSchema schema, CancellationToken ct = default);
    Task<FormWorkflowSchema?> GetByIdAsync(int workflowId, int? opportunityId = null, CancellationToken ct = default);
    Task<FormWorkflowSchema[]> GetAllAsync(int? opportunityId = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(int workflowId, int? opportunityId = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(int workflowId, int? opportunityId = null, CancellationToken ct = default);
}
```

#### 3.2 Create SQL Server Implementation
**File:** `DynamicForms.SqlServer/Repositories/SqlServerWorkflowRepository.cs`
- Follow same Dapper pattern as `SqlServerFormModuleRepository`
- Store `FormWorkflowSchema` as JSON in `WorkflowSchemas` table

#### 3.3 Register in Service Extensions
**File:** `DynamicForms.SqlServer/Extensions/ServiceCollectionExtensions.cs`
```csharp
services.AddScoped<IWorkflowRepository>(provider =>
    new SqlServerWorkflowRepository(connectionString,
        provider.GetRequiredService<ILogger<SqlServerWorkflowRepository>>()));
```

### Phase 4: Integration with VisualEditorOpus

#### 4.1 Create Editor Persistence Service
**File:** `VisualEditorOpus/Services/IEditorPersistenceService.cs`
```csharp
public interface IEditorPersistenceService
{
    // Form Module operations
    Task<bool> SaveFormModuleAsync(FormModule module);
    Task<FormModule?> LoadFormModuleAsync(int moduleId, int? opportunityId = null);
    Task<IEnumerable<ModuleSearchResult>> GetAllModulesAsync();
    Task<bool> DeleteFormModuleAsync(int moduleId);

    // Workflow operations
    Task<bool> SaveWorkflowAsync(FormWorkflowSchema workflow);
    Task<FormWorkflowSchema?> LoadWorkflowAsync(int workflowId, int? opportunityId = null);
    Task<IEnumerable<FormWorkflowSchema>> GetAllWorkflowsAsync();
    Task<bool> DeleteWorkflowAsync(int workflowId);
}
```

#### 4.2 Implement Persistence Service
**File:** `VisualEditorOpus/Services/EditorPersistenceService.cs`
- Wraps `IFormModuleRepository` and `IWorkflowRepository`
- Handles conversion between editor models and persistence models
- Provides toast notifications on save/load success/failure

#### 4.3 Update Editor Components
Modify existing editor components to use `IEditorPersistenceService`:
- Add Save button handlers
- Add Load/Open dialogs
- Auto-save functionality (optional)

---

## 3. GAPS ANALYSIS

### What's Missing (Must Build)

| Component | Description | Effort |
|-----------|-------------|--------|
| `IWorkflowRepository` | Interface for workflow persistence | Low |
| `SqlServerWorkflowRepository` | Dapper implementation for workflows | Medium |
| `WorkflowSchemas` table | Database table for workflows | Low |
| `IEditorPersistenceService` | Bridge between UI and repositories | Medium |
| `EditorPersistenceService` | Implementation with error handling | Medium |
| UI Integration | Save/Load buttons, dialogs | Medium |

### What's Ready (Just Wire Up)

| Component | Status |
|-----------|--------|
| `SqlServerFormModuleRepository` | Fully implemented, tested |
| `SqlServerFormDataRepository` | Fully implemented, tested |
| `ServiceCollectionExtensions` | Ready for registration |
| `FormModule` / `FormField` entities | Complete with serialization |
| `FormWorkflowSchema` | Complete record definition |

---

## 4. INTERFACE COMPATIBILITY ANALYSIS

### Current Issue: Interface Mismatch

The `IFormModuleRepository` interface in `DynamicForms.Core.V4` uses `FormModuleSchema`:
```csharp
Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken ct);
Task<FormModuleSchema?> GetByIdAsync(int moduleId, ...);
```

But `SqlServerFormModuleRepository` uses `FormModule`:
```csharp
Task<bool> SaveEnhancedModuleAsync(FormModule module, int moduleId, ...);
Task<FormModule?> GetEnhancedMetadataAsync(int moduleId, ...);
```

### Resolution Options

**Option A: Adapter Pattern (Recommended)**
Create an adapter that converts between `FormModuleSchema` and `FormModule`:
- Pros: No changes to existing code, clean separation
- Cons: Small conversion overhead

**Option B: Update Interface**
Change `IFormModuleRepository` to use `FormModule` directly:
- Pros: Direct, no conversion
- Cons: Breaking change, affects all consumers

**Option C: Dual Support**
Repository supports both types with separate methods:
- Pros: Maximum flexibility
- Cons: Larger API surface

---

## 5. RECOMMENDED IMPLEMENTATION ORDER

1. **Phase 1: Infrastructure** (Day 1)
   - Add project reference
   - Add connection string
   - Create LocalDB database
   - Run schema scripts

2. **Phase 2: Wire Up Existing** (Day 1-2)
   - Register `SqlServerFormModuleRepository` in DI
   - Create/update adapter if needed
   - Test basic save/load of FormModule

3. **Phase 3: Workflow Repository** (Day 2-3)
   - Create `IWorkflowRepository` interface
   - Implement `SqlServerWorkflowRepository`
   - Create `WorkflowSchemas` table
   - Register in DI

4. **Phase 4: UI Integration** (Day 3-4)
   - Create `IEditorPersistenceService`
   - Implement service
   - Add Save/Load UI elements
   - Test end-to-end

5. **Phase 5: Polish** (Day 4-5)
   - Error handling improvements
   - Auto-save functionality
   - Recent files list
   - Export/Import integration

---

## 6. RISK ASSESSMENT

| Risk | Impact | Mitigation |
|------|--------|------------|
| Interface mismatch | High | Use adapter pattern |
| LocalDB not available | Medium | Provide fallback or clear error |
| Data loss on schema evolution | High | Version tracking already built-in |
| Performance with large forms | Low | Dapper is highly optimized |
| Transaction failures | Medium | Already has rollback handling |

---

## 7. FILES TO MODIFY/CREATE

### Modify
1. `VisualEditorOpus.csproj` - Add project reference
2. `appsettings.json` - Add connection string
3. `Program.cs` - Register SQL Server services
4. `ServiceCollectionExtensions.cs` - Add workflow repository registration

### Create
1. `DynamicForms.Core.V4/Services/IWorkflowRepository.cs`
2. `DynamicForms.SqlServer/Repositories/SqlServerWorkflowRepository.cs`
3. `VisualEditorOpus/Services/IEditorPersistenceService.cs`
4. `VisualEditorOpus/Services/EditorPersistenceService.cs`
5. `DatabaseSchema.sql` - Complete schema script

---

## 8. APPROVAL CHECKLIST

Before implementation, please confirm:

- [ ] LocalDB SQL Server is available on your machine
- [ ] Preferred option for interface mismatch (A, B, or C)
- [ ] Auto-save feature desired? (Yes/No)
- [ ] Recent files list desired? (Yes/No)
- [ ] Health check endpoints desired? (Yes/No)
- [ ] Any specific naming conventions to follow?

---

*Generated: December 11, 2025*
