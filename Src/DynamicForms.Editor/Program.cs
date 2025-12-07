using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
//using DynamicForms.Editor.Data;
using DynamicForms.Core.V4.Extensions;
using DynamicForms.Editor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDynamicFormsV4();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IUndoRedoService, UndoRedoService>();
builder.Services.AddScoped<ISchemaValidationService, SchemaValidationService>();
builder.Services.AddScoped<IWorkflowGraphService, WorkflowGraphService>();
builder.Services.AddScoped<ICodeSetManagerService, CodeSetManagerService>();
builder.Services.AddScoped<IEditorStateService, EditorStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
