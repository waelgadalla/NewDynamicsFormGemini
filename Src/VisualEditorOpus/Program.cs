using VisualEditorOpus.Components;
using VisualEditorOpus.Services;
using VisualEditorOpus.Services.Theming;
using DynamicForms.Core.V4.Services;
using DynamicForms.SqlServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add memory cache for CodeSet caching
builder.Services.AddMemoryCache();

// Add HttpClient for CodeSet loader
builder.Services.AddHttpClient<ICodeSetLoader, CodeSetLoader>();

// Register DynamicForms Core services
builder.Services.AddScoped<IFormHierarchyService, FormHierarchyService>();
builder.Services.AddScoped<ICodeSetProvider, InMemoryCodeSetProvider>();

// Register Editor services
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IUndoRedoService, UndoRedoService>();
builder.Services.AddScoped<ISchemaValidationService, SchemaValidationService>();
builder.Services.AddScoped<IEditorStateService, EditorStateService>();
builder.Services.AddScoped<IJsonImportExportService, JsonImportExportService>();

// Register Theme Editor services
builder.Services.AddScoped<IThemeEditorStateService, ThemeEditorStateService>();
builder.Services.AddSingleton<IThemeCssGeneratorService, ThemeCssGeneratorService>();
builder.Services.AddScoped<IThemeImportExportService, ThemeImportExportService>();
builder.Services.AddScoped<IThemePersistenceService, ThemePersistenceService>();
builder.Services.AddSingleton<IThemeValidatorService, ThemeValidatorService>();

// Register CodeSet management services
builder.Services.AddScoped<ICodeSetCache, CodeSetCache>();
builder.Services.AddScoped<ICodeSetService, CodeSetService>();
builder.Services.AddSingleton<ITabStateService, TabStateService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();

// Register SQL Server persistence for Visual Editor (FormModuleSchema, FormWorkflowSchema)
builder.Services.AddVisualEditorSqlServer(builder.Configuration);
builder.Services.AddScoped<IEditorPersistenceService, EditorPersistenceService>();
builder.Services.AddScoped<IAutoSaveService, AutoSaveService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
