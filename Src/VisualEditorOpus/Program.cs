using VisualEditorOpus.Components;
using VisualEditorOpus.Services;
using DynamicForms.Core.V4.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register DynamicForms Core services
builder.Services.AddScoped<IFormHierarchyService, FormHierarchyService>();
builder.Services.AddScoped<ICodeSetProvider, InMemoryCodeSetProvider>();

// Register Editor services
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IUndoRedoService, UndoRedoService>();
builder.Services.AddScoped<ISchemaValidationService, SchemaValidationService>();
builder.Services.AddScoped<IEditorStateService, EditorStateService>();

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
