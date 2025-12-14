
````markdown
# Technical Specification: Native Blazor Theme Editor
**Version:** 1.0
**Architecture:** CSS Custom Properties (Zero JS Dependency)
**Target Framework:** .NET 8 / 9 (Blazor WASM or Server)

---

## 1. Architectural Overview

This solution enables a "No-Code" styling experience for non-technical users ("Citizen Developers") without requiring a paid license for SurveyJS or complex JavaScript build steps.

**Core Mechanism:**
1.  **State:** A C# Model (`ThemeSettings`) holds branding values (Colors, Fonts, Radius).
2.  **Injection:** A Blazor Wrapper (`ThemeScope`) injects these values into the HTML `style` attribute as standard CSS Variables (e.g., `--df-primary`).
3.  **Rendering:** Browser natively repaints the UI instantly when the `style` attribute changes.

---

## 2. Data Models

### A. The Theme Settings Model
*File: `Models/ThemeSettings.cs`*
This class is serializable to JSON for storage in your SQL Server.

```csharp
namespace DynamicForms.Models
{
    public class ThemeSettings
    {
        // --- Branding Colors ---
        public string PrimaryColor { get; set; } = "#007bff";      // Main Action Buttons
        public string SecondaryColor { get; set; } = "#6c757d";    // Secondary Actions
        public string BackgroundColor { get; set; } = "#ffffff";   // Form Canvas Background
        
        // --- Typography & Text ---
        public string TextColor { get; set; } = "#212529";         // Standard Text
        public string FontFamily { get; set; } = "Segoe UI, sans-serif";
        
        // --- UI Geometry ---
        public string BorderRadius { get; set; } = "4px";          // Corner Roundness
        
        // --- Enterprise Assets ---
        public string LogoUrl { get; set; } = "";                  // Organization Logo
    }
}
````

-----

## 3\. Core Engine Components

### B. The Theme Scope Wrapper

*File: `Components/Theming/ThemeScope.razor`*
Wrap your form renderer in this component. It acts as the "CSS Variable Injector".

```razor
@using DynamicForms.Models

<div class="df-theme-scope" style="@GetCssVariables()">
    
    @if (!string.IsNullOrEmpty(Theme.LogoUrl))
    {
        <div class="df-form-header">
            <img src="@Theme.LogoUrl" alt="Organization Logo" class="df-logo" />
        </div>
    }

    @ChildContent
</div>

@code {
    [Parameter] public ThemeSettings Theme { get; set; } = new();
    [Parameter] public RenderFragment ChildContent { get; set; }

    private string GetCssVariables()
    {
        // Maps C# properties to CSS Variables
        return string.Join(";", 
            $"--df-primary: {Theme.PrimaryColor}",
            $"--df-secondary: {Theme.SecondaryColor}",
            $"--df-bg: {Theme.BackgroundColor}",
            $"--df-text: {Theme.TextColor}",
            $"--df-radius: {Theme.BorderRadius}",
            $"--df-font: {Theme.FontFamily}"
        );
    }
}
```

-----

## 4\. UI Component Implementation (Example)

### C. Standard Form Button

*File: `Components/FormControls/FormButton.razor`*
This is how your components consume the theme variables.

```razor
<button class="df-btn" @onclick="OnClick">
    @Text
</button>

<style>
    /* Scoped CSS for the button */
    .df-btn {
        /* Consume variables */
        background-color: var(--df-primary);
        border-radius: var(--df-radius);
        font-family: var(--df-font);
        color: white; /* You might want to calculate contrast color dynamically later */
        
        /* Standard Layout */
        border: none;
        padding: 10px 24px;
        cursor: pointer;
        font-size: 1rem;
        transition: opacity 0.2s ease-in-out;
    }

    .df-btn:hover {
        opacity: 0.9; /* Simple hover effect */
    }
</style>

@code {
    [Parameter] public string Text { get; set; } = "Submit";
    [Parameter] public EventCallback OnClick { get; set; }
}
```

-----

## 5\. The Editor Interface (Citizen Developer UI)

### D. Theme Editor Page

*File: `Pages/Admin/ThemeEditor.razor`*
This interface allows users to design the theme with a live preview.

```razor
@page "/admin/design-theme"
@using DynamicForms.Models
@using DynamicForms.Components.Theming
@inject HttpClient Http

<div class="df-editor-layout">
    
    <div class="df-sidebar">
        <h3>🎨 Theme Designer</h3>
        <p class="text-muted">Customize your form appearance.</p>

        <div class="form-group">
            <label>Brand Color</label>
            <div class="input-group">
                <input type="color" class="form-control form-control-color" 
                       @bind="CurrentTheme.PrimaryColor" 
                       @bind:event="oninput" />
                <input type="text" class="form-control" 
                       @bind="CurrentTheme.PrimaryColor" 
                       @bind:event="oninput" />
            </div>
        </div>

        <div class="form-group">
            <label>Background Color</label>
            <input type="color" class="form-control form-control-color w-100" 
                   @bind="CurrentTheme.BackgroundColor" 
                   @bind:event="oninput" />
        </div>

        <div class="form-group">
            <label>Typography</label>
            <select @bind="CurrentTheme.FontFamily" class="form-select">
                <option value="Segoe UI, sans-serif">Standard (Segoe UI)</option>
                <option value="'Helvetica Neue', Helvetica, Arial, sans-serif">Clean (Helvetica)</option>
                <option value="'Times New Roman', serif">Formal (Serif)</option>
                <option value="'Courier New', monospace">Technical (Courier)</option>
            </select>
        </div>

        <div class="form-group">
            <label>Corner Style</label>
            <select @bind="CurrentTheme.BorderRadius" class="form-select">
                <option value="0px">Sharp (Square)</option>
                <option value="4px">Standard (4px)</option>
                <option value="8px">Soft (8px)</option>
                <option value="20px">Playful (20px)</option>
            </select>
        </div>

        <div class="form-group">
            <label>Logo URL</label>
            <input type="text" class="form-control" @bind="CurrentTheme.LogoUrl" placeholder="https://..." />
        </div>

        <hr />
        <button class="btn btn-success w-100" @onclick="SaveTheme">💾 Save Theme</button>
    </div>

    <div class="df-preview-stage">
        <div class="df-preview-container">
            <ThemeScope Theme="CurrentTheme">
                
                <div class="df-card">
                    <h2 style="color: var(--df-primary); font-family: var(--df-font);">Grant Application</h2>
                    <p style="font-family: var(--df-font); color: var(--df-text);">
                        Please review your details before submitting.
                    </p>

                    <div class="mb-3">
                        <label style="font-family: var(--df-font);">Applicant Name</label>
                        <input type="text" class="form-control" 
                               style="border-radius: var(--df-radius); border: 1px solid var(--df-secondary);" />
                    </div>

                    <div class="mb-3">
                        <label style="font-family: var(--df-font);">Department</label>
                        <select class="form-select" style="border-radius: var(--df-radius);">
                            <option>Finance</option>
                            <option>HR</option>
                        </select>
                    </div>

                    <FormButton Text="Submit Application" />
                </div>

            </ThemeScope>
        </div>
    </div>
</div>

<style>
    /* Editor Layout Styles */
    .df-editor-layout { display: flex; height: 100vh; width: 100%; }
    .df-sidebar { width: 320px; background: #f8f9fa; border-right: 1px solid #dee2e6; padding: 20px; overflow-y: auto; }
    .df-preview-stage { flex: 1; background: #e9ecef; display: flex; align-items: center; justify-content: center; }
    
    /* Preview Card Style (Simulates the Form Paper) */
    .df-card {
        background-color: var(--df-bg);
        padding: 40px;
        width: 100%;
        max-width: 600px;
        border-radius: var(--df-radius);
        box-shadow: 0 10px 30px rgba(0,0,0,0.1);
        color: var(--df-text);
    }
    
    .form-group { margin-bottom: 1rem; }
    .df-logo { max-height: 60px; margin-bottom: 20px; display: block; }
</style>

@code {
    private ThemeSettings CurrentTheme = new ThemeSettings();

    private async Task SaveTheme()
    {
        // Serialize and Send to API
        var response = await Http.PostAsJsonAsync("api/themes", CurrentTheme);
        if(response.IsSuccessStatusCode)
        {
            // Show toast notification
        }
    }
}
```

-----

## 6\. Backend Integration

### E. API Controller (Saving the Theme)

*File: `Controllers/ThemesController.cs`*

```csharp
[ApiController]
[Route("api/[controller]")]
public class ThemesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ThemesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SaveTheme([FromBody] ThemeSettings theme)
    {
        // 1. Serialize the settings to a simple JSON string
        string jsonContent = JsonSerializer.Serialize(theme);

        // 2. Save to database (Assuming a 'Themes' table exists)
        var entity = new ThemeEntity 
        { 
            Name = "Custom Theme", 
            JsonData = jsonContent,
            LastUpdated = DateTime.UtcNow 
        };

        _context.Themes.Add(entity);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
```

```
```