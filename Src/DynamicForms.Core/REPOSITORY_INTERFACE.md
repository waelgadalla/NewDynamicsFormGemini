# IFormModuleRepository Interface

## Overview

I've successfully created the `IFormModuleRepository` interface specifically designed to work with `FormModule` instead of the base `FormModule`. This interface provides enhanced functionality with improved architectural patterns.

## ?? **Location**
- **File**: `src\DynamicForms.Core\Interfaces\IFormServices.cs`
- **Namespace**: `DynamicForms.Core.Interfaces`
- **Position**: Primary repository interface for enhanced module management

## ?? **Enhanced Methods**

### **Core Repository Methods**

| **Method** | **Purpose** |
|------------|-------------|
| `GetEnhancedMetadataAsync()` | Retrieve single enhanced module |
| `GetEnhancedMetadataForModulesAsync()` | Retrieve multiple enhanced modules |
| `SaveEnhancedModuleAsync()` | Save enhanced module |
| `DeleteEnhancedModuleAsync()` | Delete enhanced module |
| `EnhancedExistsAsync()` | Check enhanced module existence |
| `GetEnhancedModuleVersionsAsync()` | Get enhanced module versions |

### **Advanced Methods** (Enhanced functionality)

| **Method** | **Purpose** |
|------------|-------------|
| `GetEnhancedMetadataWithHierarchyAsync()` | Gets module with automatically built hierarchy |
| `SaveEnhancedModuleWithOptimizationAsync()` | Saves with field ordering optimization |
| `GetModuleStatisticsAsync()` | Returns detailed module analytics |
| `SearchModulesByComplexityAsync()` | Advanced search by hierarchy complexity |
| `CloneModuleAsync()` | Create module copies with optional field filtering |

## ?? **Key Features**

### **1. Enhanced Return Types**
- Returns `FormModule` with full hierarchy support
- Includes `ModuleVersionInfo` with complexity metrics
- Provides `ModuleStatistics` for analytics

### **2. Advanced Search Capabilities**
```csharp
Task<IEnumerable<ModuleSearchResult>> SearchModulesByComplexityAsync(
    ModuleSearchCriteria criteria, 
    CancellationToken cancellationToken = default);
```

**Search Criteria Include:**
- Field count ranges (min/max)
- Hierarchy depth ranges
- Complexity score ranges
- Specific field types
- Relationship types
- Date ranges
- Creator filtering

### **3. Automatic Optimization**
```csharp
Task<bool> SaveEnhancedModuleWithOptimizationAsync(
    FormModule module, 
    int moduleId, 
    int? opportunityId = null, 
    CancellationToken cancellationToken = default);
```
- Automatically optimizes field ordering
- Validates parent-child relationships
- Fixes circular references before saving

### **4. Built-in Hierarchy Support**
```csharp
Task<FormModule?> GetEnhancedMetadataWithHierarchyAsync(
    int moduleId, 
    int? opportunityId = null, 
    CancellationToken cancellationToken = default);
```
- Automatically calls `RebuildFieldHierarchy()`
- Returns fully navigable hierarchy structure

## ?? **Supporting Model Classes**

### **ModuleVersionInfo**
Enhanced version information with hierarchy metrics:
```csharp
public class ModuleVersionInfo
{
    // Base properties
    public int ModuleId { get; set; }
    public int? OpportunityId { get; set; }
    public float Version { get; set; }
    public DateTime DateCreated { get; set; }
    public string? CreatedBy { get; set; }
    public string? Description { get; set; }
    public bool IsCurrent { get; set; }
    
    // Enhanced metrics
    public int TotalFields { get; set; }
    public int RootFields { get; set; }
    public int MaxDepth { get; set; }
    public double ComplexityScore { get; set; }
    public string[] FieldTypes { get; set; }
    public string[] RelationshipTypes { get; set; }
}
```

### **ModuleSearchCriteria**
Advanced search parameters:
```csharp
public class ModuleSearchCriteria
{
    // Hierarchy metrics
    public int? MinFieldCount { get; set; }
    public int? MaxFieldCount { get; set; }
    public int? MinDepth { get; set; }
    public int? MaxDepth { get; set; }
    public double? MinComplexity { get; set; }
    public double? MaxComplexity { get; set; }
    
    // Content filters
    public string[]? FieldTypes { get; set; }
    public string[]? RelationshipTypes { get; set; }
    
    // Date and user filters
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? CreatedBy { get; set; }
    
    // Pagination
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public string SortBy { get; set; } = "DateCreated";
    public bool SortDescending { get; set; } = true;
}
```

### **ModuleSearchResult**
Search results with embedded statistics:
```csharp
public class ModuleSearchResult
{
    public int ModuleId { get; set; }
    public int? OpportunityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public float Version { get; set; }
    public DateTime DateCreated { get; set; }
    public string? CreatedBy { get; set; }
    public ModuleStatistics Statistics { get; set; }
}
```

## ?? **Usage Examples**

### **Basic Usage**
```csharp
public class MyService
{
    private readonly IFormModuleRepository _repository;
    
    public async Task<FormModule?> LoadEnhancedModule(int moduleId)
    {
        // Load module with automatic hierarchy building
        var module = await _repository.GetEnhancedMetadataWithHierarchyAsync(moduleId);
        
        // Module is ready to use with full hierarchy navigation
        var rootFields = module?.GetRootFields();
        var stats = module?.GetModuleStatistics();
        
        return module;
    }
}
```

### **Advanced Search**
```csharp
public async Task<IEnumerable<ModuleSearchResult>> FindComplexModules()
{
    var criteria = new ModuleSearchCriteria
    {
        MinFieldCount = 10,
        MaxDepth = 5,
        MinComplexity = 50.0,
        FieldTypes = new[] { "Section", "ConditionalGroup" },
        RelationshipTypes = new[] { "ConditionalShow", "Cascade" },
        SortBy = "ComplexityScore",
        SortDescending = true
    };
    
    return await _repository.SearchModulesByComplexityAsync(criteria);
}
```

### **Module Cloning**
```csharp
public async Task<FormModule?> CloneModuleTemplate(
    int sourceId, int newId)
{
    // Clone only non-required fields
    return await _repository.CloneModuleAsync(
        sourceModuleId: sourceId,
        sourceOpportunityId: null,
        newModuleId: newId,
        newOpportunityId: null,
        fieldFilter: field => !field.IsRequired
    );
}
```

### **Optimized Saving**
```csharp
public async Task<bool> SaveWithOptimization(FormModule module)
{
    // Automatically optimizes field ordering and validates relationships
    return await _repository.SaveEnhancedModuleWithOptimizationAsync(
        module, module.Id!.Value, module.OpportunityId);
}
```

## ??? **Implementation Guidelines**

### **For Repository Implementers**

1. **Use Conversion Patterns**: Follow patterns similar to `RazorPagesService`
2. **Hierarchy Management**: Always call `RebuildFieldHierarchy()` on loaded modules
3. **Optimization Logic**: Implement field ordering and validation in the optimization methods
4. **Statistics Calculation**: Use the built-in `GetModuleStatistics()` method
5. **Search Implementation**: Create efficient database queries for complexity-based searches

### **For Service Consumers**

1. **Use Enhanced Methods**: Prefer `GetEnhancedMetadataWithHierarchyAsync()` over basic retrieval
2. **Leverage Statistics**: Use module statistics for UI display and reporting
3. **Implement Search**: Use the advanced search for module management UIs
4. **Handle Conversions**: Storage may still use base `FormModule` format for compatibility

## ?? **Migration Benefits**

### **Enhanced Architecture**
1. **Direct Usage**: Work directly with `FormModule`
2. **Rich Functionality**: Full hierarchy, validation, and analysis features
3. **Better Performance**: Optimized operations with automatic enhancements
4. **Future-Proof**: Built for extensibility and maintainability

### **Storage Compatibility**
1. **Backward Compatible**: Maintains FormModule storage format compatibility
2. **Conversion Patterns**: Seamless conversion between formats when needed
3. **Migration Ready**: Can upgrade storage format in future versions
4. **Cross-Platform**: Works with Entity Framework and SQL Server repositories

## ? **Benefits**

1. **Type Safety**: Work directly with `FormModule`
2. **Enhanced Functionality**: Access to hierarchy, statistics, and advanced features
3. **Better Performance**: Fewer conversions and automatic optimization
4. **Rich Analytics**: Built-in complexity metrics and search capabilities
5. **Future-Proof**: Designed for advanced form management scenarios

## ?? **Implementation Status**

### **Available Implementations**
1. **Entity Framework**: `EfFormModuleRepository`
2. **SQL Server**: `SqlServerFormModuleRepository` 
3. **Service Integration**: `RazorPagesService`

### **Testing Coverage**
1. **Unit Tests**: Comprehensive test coverage for all methods
2. **Integration Tests**: End-to-end testing with real repositories
3. **Performance Tests**: Validation of conversion and hierarchy performance
4. **Migration Tests**: Compatibility and data preservation testing

---

The `IFormModuleRepository` interface provides a powerful, type-safe, and feature-rich repository architecture for working with enhanced form modules while maintaining compatibility with existing storage systems.The `IFormModuleRepository` interface provides a powerful, type-safe, and feature-rich alternative to the base `IFormModuleRepository` while maintaining architectural consistency and providing a clear upgrade path for applications using modules.