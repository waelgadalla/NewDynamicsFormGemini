using DynamicForms.Core.V4.Schemas;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Extensions.Logging;

namespace VisualEditorOpus.Services;

/// <summary>
/// Implementation of IEditorPersistenceService.
/// Provides persistence operations for the Visual Editor.
/// </summary>
public class EditorPersistenceService : IEditorPersistenceService
{
    private readonly IModuleSchemaRepository _moduleRepository;
    private readonly IWorkflowSchemaRepository _workflowRepository;
    private readonly ILogger<EditorPersistenceService> _logger;

    public EditorPersistenceService(
        IModuleSchemaRepository moduleRepository,
        IWorkflowSchemaRepository workflowRepository,
        ILogger<EditorPersistenceService> logger)
    {
        _moduleRepository = moduleRepository;
        _workflowRepository = workflowRepository;
        _logger = logger;
    }

    #region Module Operations

    public async Task<SaveResult> SaveModuleAsync(FormModuleSchema module)
    {
        try
        {
            _logger.LogInformation("Saving module {ModuleId}: {Title}", module.Id, module.TitleEn);

            // Update the DateUpdated
            var updatedModule = module with
            {
                DateUpdated = DateTime.UtcNow
            };

            var success = await _moduleRepository.SaveAsync(updatedModule);

            if (success)
            {
                _logger.LogInformation("Module {ModuleId} saved successfully", module.Id);
                return SaveResult.Ok(module.Id);
            }
            else
            {
                _logger.LogWarning("Failed to save module {ModuleId}", module.Id);
                return SaveResult.Fail("Failed to save module to database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving module {ModuleId}", module.Id);
            return SaveResult.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<FormModuleSchema?> LoadModuleAsync(int moduleId)
    {
        try
        {
            _logger.LogInformation("Loading module {ModuleId}", moduleId);
            var module = await _moduleRepository.GetByIdAsync(moduleId);

            if (module != null)
            {
                _logger.LogInformation("Module {ModuleId} loaded with {FieldCount} fields",
                    moduleId, module.Fields.Length);
            }
            else
            {
                _logger.LogWarning("Module {ModuleId} not found", moduleId);
            }

            return module;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading module {ModuleId}", moduleId);
            throw;
        }
    }

    public async Task<IEnumerable<ModuleSchemaSummary>> GetAllModulesAsync()
    {
        try
        {
            return await _moduleRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all modules");
            throw;
        }
    }

    public async Task<bool> DeleteModuleAsync(int moduleId)
    {
        try
        {
            _logger.LogInformation("Deleting module {ModuleId}", moduleId);
            return await _moduleRepository.DeleteAsync(moduleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module {ModuleId}", moduleId);
            return false;
        }
    }

    public async Task<int> GetNextModuleIdAsync()
    {
        try
        {
            return await _moduleRepository.GetNextModuleIdAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next module ID");
            throw;
        }
    }

    #endregion

    #region Workflow Operations

    public async Task<SaveResult> SaveWorkflowAsync(FormWorkflowSchema workflow)
    {
        try
        {
            _logger.LogInformation("Saving workflow {WorkflowId}: {Title}", workflow.Id, workflow.TitleEn);

            var success = await _workflowRepository.SaveAsync(workflow);

            if (success)
            {
                _logger.LogInformation("Workflow {WorkflowId} saved successfully", workflow.Id);
                return SaveResult.Ok(workflow.Id);
            }
            else
            {
                _logger.LogWarning("Failed to save workflow {WorkflowId}", workflow.Id);
                return SaveResult.Fail("Failed to save workflow to database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow {WorkflowId}", workflow.Id);
            return SaveResult.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<FormWorkflowSchema?> LoadWorkflowAsync(int workflowId)
    {
        try
        {
            _logger.LogInformation("Loading workflow {WorkflowId}", workflowId);
            var workflow = await _workflowRepository.GetByIdAsync(workflowId);

            if (workflow != null)
            {
                _logger.LogInformation("Workflow {WorkflowId} loaded with {ModuleCount} modules",
                    workflowId, workflow.ModuleIds.Length);
            }
            else
            {
                _logger.LogWarning("Workflow {WorkflowId} not found", workflowId);
            }

            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkflowSchemaSummary>> GetAllWorkflowsAsync()
    {
        try
        {
            return await _workflowRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all workflows");
            throw;
        }
    }

    public async Task<bool> DeleteWorkflowAsync(int workflowId)
    {
        try
        {
            _logger.LogInformation("Deleting workflow {WorkflowId}", workflowId);
            return await _workflowRepository.DeleteAsync(workflowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId}", workflowId);
            return false;
        }
    }

    public async Task<int> GetNextWorkflowIdAsync()
    {
        try
        {
            return await _workflowRepository.GetNextWorkflowIdAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next workflow ID");
            throw;
        }
    }

    #endregion
}
