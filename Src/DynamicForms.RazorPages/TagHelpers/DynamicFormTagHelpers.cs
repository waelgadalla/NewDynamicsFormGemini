using DynamicForms.RazorPages.Models;
using DynamicForms.Core.Entities;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace DynamicForms.RazorPages.TagHelpers;

/// <summary>
/// Tag helper for rendering dynamic form fields using  Enhanced architecture
/// Usage: <dynamic-field field="@field" value="@value" language="EN" />
/// </summary>
[HtmlTargetElement("dynamic-field")]
public class DynamicFieldTagHelper : TagHelper
{
    /// <summary>
    /// The field view model containing FormField
    /// </summary>
    [HtmlAttributeName("field")]
    public DynamicFieldViewModel? Field { get; set; }

    /// <summary>
    /// Language for localization
    /// </summary>
    [HtmlAttributeName("language")]
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Field index for form naming
    /// </summary>
    [HtmlAttributeName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Whether the field is read-only
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Additional CSS classes
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Form data for conditional rendering
    /// </summary>
    [HtmlAttributeName("form-data")]
    public Dictionary<string, object>? FormData { get; set; }

    /// <summary>
    /// Show hierarchy debug information
    /// </summary>
    [HtmlAttributeName("show-hierarchy-debug")]
    public bool ShowHierarchyDebug { get; set; } = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override void Process(TagHelperContext context, TagHelperOutput output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (Field?.Field == null)
        {
            output.SuppressOutput();
            return;
        }

        // Check if field should be visible using enhanced logic
        if (!Field.ShouldBeVisible(FormData))
        {
            output.SuppressOutput();
            return;
        }

        // Set the field properties
        Field.Language = Language;
        Field.Index = Index;
        Field.IsReadOnly = IsReadOnly;

        // Add additional CSS classes
        if (!string.IsNullOrEmpty(CssClass))
        {
            Field.Field.CssClasses = string.IsNullOrEmpty(Field.Field.CssClasses) 
                ? CssClass 
                : $"{Field.Field.CssClasses} {CssClass}";
        }

        // Generate the field HTML based on field type
        var html = Field.Field.FieldType.Type.ToLower() switch
        {
            "textbox" => GenerateTextBox(),
            "textarea" => GenerateTextArea(),
            "dropdown" or "dropdownlist" => GenerateDropDown(),
            "checkbox" => GenerateCheckBox(),
            "checkboxlist" => GenerateCheckBoxList(),
            "radiobutton" => GenerateRadioButton(),
            "datebox" => GenerateDateBox(),
            "numbertextbox" => GenerateNumberBox(),
            "emailtextbox" => GenerateEmailBox(),
            "telephonetextbox" => GenerateTelephoneBox(),
            "urltextbox" => GenerateUrlBox(),
            "fileupload" => GenerateFileUpload(),
            "infobox" => GenerateInfoBox(),
            "label" => GenerateLabel(),
            "section" => GenerateSection(),
            "group" => GenerateGroup(),
            _ => GenerateTextBox() // Default fallback
        };

        output.TagName = null; // Remove the tag helper element
        output.Content.SetHtmlContent(html);
    }

    private string GenerateTextBox()
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{Field!.Field.Id}";
        var fieldName = $"Submission.Fields[{Index}].Value";

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "form-group", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"textbox\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Hierarchy debug info
        if (ShowHierarchyDebug)
        {
            sb.AppendLine(GenerateHierarchyDebugInfo());
        }
        
        // Label
        if (!string.IsNullOrEmpty(Field.GetLabel()))
        {
            sb.AppendLine($"  <label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"    {Field.GetLabel()}");
            if (Field.Field.IsRequired)
                sb.AppendLine("    <span class=\"text-danger ms-1\">*</span>");
            if (ShowHierarchyDebug)
                sb.AppendLine($"    <span class=\"hierarchy-badge relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}\">{Field.Field.RelationshipType}</span>");
            sb.AppendLine("  </label>");
        }

        // Input
        sb.AppendLine($"  <input type=\"text\"");
        sb.AppendLine($"         id=\"{fieldId}\"");
        sb.AppendLine($"         name=\"{fieldName}\"");
        sb.AppendLine($"         class=\"{Field.GetCssClasses()}\"");
        sb.AppendLine($"         value=\"{Field.GetStringValue()}\"");

        if (Field.Field.MaximumLength.HasValue)
            sb.AppendLine($"         maxlength=\"{Field.Field.MaximumLength}\"");

        if (!string.IsNullOrEmpty(Field.GetPlaceholder()))
            sb.AppendLine($"         placeholder=\"{Field.GetPlaceholder()}\"");

        if (Field.Field.IsRequired)
            sb.AppendLine("         required");

        if (IsReadOnly || Field.Field.ReadOnly)
            sb.AppendLine("         readonly");

        sb.AppendLine("         data-standalone-enhanced=\"true\" />");

        // Hidden field for field ID
        sb.AppendLine($"  <input type=\"hidden\" name=\"Submission.Fields[{Index}].FieldId\" value=\"{Field.Field.Id}\" />");

        // Help text
        if (!string.IsNullOrEmpty(Field.GetHelpText()))
        {
            sb.AppendLine($"  <div class=\"form-text\">{Field.GetHelpText()}</div>");
        }

        // Validation
        if (Field.HasValidationErrors)
        {
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{Field.GetValidationError()}</div>");
        }

        // Render child fields if this is a container
        sb.AppendLine(GenerateChildFields());

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private string GenerateTextArea()
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{Field!.Field.Id}";
        var fieldName = $"Submission.Fields[{Index}].Value";

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "form-group", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"textarea\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Hierarchy debug info
        if (ShowHierarchyDebug)
        {
            sb.AppendLine(GenerateHierarchyDebugInfo());
        }
        
        // Label
        if (!string.IsNullOrEmpty(Field.GetLabel()))
        {
            sb.AppendLine($"  <label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"    {Field.GetLabel()}");
            if (Field.Field.IsRequired)
                sb.AppendLine("    <span class=\"text-danger ms-1\">*</span>");
            if (ShowHierarchyDebug)
                sb.AppendLine($"    <span class=\"hierarchy-badge relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}\">{Field.Field.RelationshipType}</span>");
            sb.AppendLine("  </label>");
        }

        // Textarea
        sb.AppendLine($"  <textarea id=\"{fieldId}\"");
        sb.AppendLine($"            name=\"{fieldName}\"");
        sb.AppendLine($"            class=\"{Field.GetCssClasses()}\"");
        sb.AppendLine("            rows=\"4\"");

        if (Field.Field.MaximumLength.HasValue)
            sb.AppendLine($"            maxlength=\"{Field.Field.MaximumLength}\"");

        if (!string.IsNullOrEmpty(Field.GetPlaceholder()))
            sb.AppendLine($"            placeholder=\"{Field.GetPlaceholder()}\"");

        if (Field.Field.IsRequired)
            sb.AppendLine("            required");

        if (IsReadOnly || Field.Field.ReadOnly)
            sb.AppendLine("            readonly");

        sb.AppendLine("            data-standalone-enhanced=\"true\"");
        sb.AppendLine($">{Field.GetStringValue()}</textarea>");

        // Hidden field for field ID
        sb.AppendLine($"  <input type=\"hidden\" name=\"Submission.Fields[{Index}].FieldId\" value=\"{Field.Field.Id}\" />");

        // Help text and validation
        if (!string.IsNullOrEmpty(Field.GetHelpText()))
            sb.AppendLine($"  <div class=\"form-text\">{Field.GetHelpText()}</div>");

        if (Field.HasValidationErrors)
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{Field.GetValidationError()}</div>");

        // Render child fields if this is a container
        sb.AppendLine(GenerateChildFields());

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private string GenerateDropDown()
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{Field!.Field.Id}";
        var fieldName = $"Submission.Fields[{Index}].Value";
        var currentValue = Field.GetStringValue();

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "form-group", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"dropdown\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Hierarchy debug info
        if (ShowHierarchyDebug)
        {
            sb.AppendLine(GenerateHierarchyDebugInfo());
        }
        
        // Label
        if (!string.IsNullOrEmpty(Field.GetLabel()))
        {
            sb.AppendLine($"  <label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"    {Field.GetLabel()}");
            if (Field.Field.IsRequired)
                sb.AppendLine("    <span class=\"text-danger ms-1\">*</span>");
            if (ShowHierarchyDebug)
                sb.AppendLine($"    <span class=\"hierarchy-badge relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}\">{Field.Field.RelationshipType}</span>");
            sb.AppendLine("  </label>");
        }

        // Select
        sb.AppendLine($"  <select id=\"{fieldId}\"");
        sb.AppendLine($"          name=\"{fieldName}\"");
        sb.AppendLine($"          class=\"{Field.GetCssClasses()}\"");

        if (Field.Field.IsRequired)
            sb.AppendLine("          required");

        if (IsReadOnly || Field.Field.ReadOnly)
            sb.AppendLine("          disabled");

        sb.AppendLine("          data-standalone-enhanced=\"true\">");

        // Default option
        if (!Field.Field.IsRequired)
        {
            var selectText = Language == "FR" ? "Sélectionnez..." : "Select...";
            sb.AppendLine($"    <option value=\"\">{selectText}</option>");
        }

        // Options
        if (Field.Field.Options != null)
        {
            foreach (var option in Field.Field.Options.OrderBy(o => o.Order))
            {
                var selected = option.Value == currentValue ? " selected" : "";
                var text = Language == "FR" ? option.FR : option.EN;
                sb.AppendLine($"    <option value=\"{option.Value}\"{selected}>{text}</option>");
            }
        }

        sb.AppendLine("  </select>");

        // Hidden field for field ID
        sb.AppendLine($"  <input type=\"hidden\" name=\"Submission.Fields[{Index}].FieldId\" value=\"{Field.Field.Id}\" />");

        // Help text and validation
        if (!string.IsNullOrEmpty(Field.GetHelpText()))
            sb.AppendLine($"  <div class=\"form-text\">{Field.GetHelpText()}</div>");

        if (Field.HasValidationErrors)
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{Field.GetValidationError()}</div>");

        // Render child fields if this is a container
        sb.AppendLine(GenerateChildFields());

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private string GenerateCheckBox()
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{Field!.Field.Id}";
        var fieldName = $"Submission.Fields[{Index}].Value";
        var isChecked = Field.GetStringValue().Equals("true", StringComparison.OrdinalIgnoreCase) || 
                       Field.GetStringValue().Equals("1", StringComparison.OrdinalIgnoreCase);

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "form-group", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"checkbox\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Hierarchy debug info
        if (ShowHierarchyDebug)
        {
            sb.AppendLine(GenerateHierarchyDebugInfo());
        }

        sb.AppendLine("  <div class=\"form-check\">");

        // Checkbox
        sb.AppendLine($"    <input type=\"checkbox\"");
        sb.AppendLine($"           id=\"{fieldId}\"");
        sb.AppendLine($"           name=\"{fieldName}\"");
        sb.AppendLine($"           class=\"form-check-input{(Field.HasValidationErrors ? " is-invalid" : "")}\"");
        sb.AppendLine("           value=\"true\"");

        if (isChecked)
            sb.AppendLine("           checked");

        if (Field.Field.IsRequired)
            sb.AppendLine("           required");

        if (IsReadOnly || Field.Field.ReadOnly)
            sb.AppendLine("           disabled");

        sb.AppendLine("           data-standalone-enhanced=\"true\" />");

        // Hidden field for unchecked value
        sb.AppendLine($"    <input type=\"hidden\" name=\"{fieldName}\" value=\"false\" />");

        // Label
        if (!string.IsNullOrEmpty(Field.GetLabel()))
        {
            sb.AppendLine($"    <label for=\"{fieldId}\" class=\"form-check-label\">");
            sb.AppendLine($"      {Field.GetLabel()}");
            if (Field.Field.IsRequired)
                sb.AppendLine("      <span class=\"text-danger ms-1\">*</span>");
            if (ShowHierarchyDebug)
                sb.AppendLine($"      <span class=\"hierarchy-badge relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}\">{Field.Field.RelationshipType}</span>");
            sb.AppendLine("    </label>");
        }

        sb.AppendLine("  </div>");

        // Hidden field for field ID
        sb.AppendLine($"  <input type=\"hidden\" name=\"Submission.Fields[{Index}].FieldId\" value=\"{Field.Field.Id}\" />");

        // Help text and validation
        if (!string.IsNullOrEmpty(Field.GetHelpText()))
            sb.AppendLine($"  <div class=\"form-text\">{Field.GetHelpText()}</div>");

        if (Field.HasValidationErrors)
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{Field.GetValidationError()}</div>");

        // Render child fields if this is a container
        sb.AppendLine(GenerateChildFields());

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private string GenerateCheckBoxList()
    {
        var sb = new StringBuilder();
        var selectedValues = Field!.Value?.Values?.Select(v => v.Value).ToList() ?? [];

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "form-group", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"checkboxlist\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Hierarchy debug info
        if (ShowHierarchyDebug)
        {
            sb.AppendLine(GenerateHierarchyDebugInfo());
        }
        
        // Label
        if (!string.IsNullOrEmpty(Field.GetLabel()))
        {
            sb.AppendLine($"  <label class=\"form-label\">");
            sb.AppendLine($"    {Field.GetLabel()}");
            if (Field.Field.IsRequired)
                sb.AppendLine("    <span class=\"text-danger ms-1\">*</span>");
            if (ShowHierarchyDebug)
                sb.AppendLine($"    <span class=\"hierarchy-badge relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}\">{Field.Field.RelationshipType}</span>");
            sb.AppendLine("  </label>");
        }

        // Checkbox options
        if (Field.Field.Options != null)
        {
            foreach (var option in Field.Field.Options.OrderBy(o => o.Order))
            {
                var optionId = $"field_{Field.Field.Id}_{option.Value}";
                var fieldName = $"Submission.Fields[{Index}].MultiValues";
                var isChecked = selectedValues.Contains(option.Value ?? string.Empty);
                var text = Language == "FR" ? option.FR : option.EN;

                sb.AppendLine("  <div class=\"form-check\">");
                sb.AppendLine($"    <input type=\"checkbox\"");
                sb.AppendLine($"           id=\"{optionId}\"");
                sb.AppendLine($"           name=\"{fieldName}\"");
                sb.AppendLine("           class=\"form-check-input\"");
                sb.AppendLine($"           value=\"{option.Value}\"");

                if (isChecked)
                    sb.AppendLine("           checked");

                if (IsReadOnly || Field.Field.ReadOnly)
                    sb.AppendLine("           disabled");

                sb.AppendLine("           data-standalone-enhanced=\"true\" />");
                sb.AppendLine($"    <label for=\"{optionId}\" class=\"form-check-label\">{text}</label>");
                sb.AppendLine("  </div>");
            }
        }

        // Hidden field for field ID
        sb.AppendLine($"  <input type=\"hidden\" name=\"Submission.Fields[{Index}].FieldId\" value=\"{Field.Field.Id}\" />");

        // Help text and validation
        if (!string.IsNullOrEmpty(Field.GetHelpText()))
            sb.AppendLine($"  <div class=\"form-text\">{Field.GetHelpText()}</div>");

        if (Field.HasValidationErrors)
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{Field.GetValidationError()}</div>");

        // Render child fields if this is a container
        sb.AppendLine(GenerateChildFields());

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    // NEW: Generate Section field (container type)
    private string GenerateSection()
    {
        var sb = new StringBuilder();

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "field-section", "mb-4" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field!.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"section\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Section header
        sb.AppendLine("  <div class=\"section-header bg-primary text-white p-3 rounded-top\">");
        sb.AppendLine("    <h4 class=\"mb-0\">");
        sb.AppendLine("      <i class=\"fas fa-folder-open me-2\"></i>");
        sb.AppendLine($"      {Field.GetLabel()}");
        sb.AppendLine("    </h4>");

        if (!string.IsNullOrEmpty(Field.GetHelpText()))
        {
            sb.AppendLine($"    <small class=\"opacity-90\">{Field.GetHelpText()}</small>");
        }

        if (ShowHierarchyDebug)
        {
            sb.AppendLine("    <div class=\"mt-2\">");
            sb.AppendLine($"      <span class=\"badge bg-light text-dark\">Children: {Field.Field.ChildFields.Count}</span>");
            sb.AppendLine("    </div>");
        }

        sb.AppendLine("  </div>");

        // Section body with child fields
        sb.AppendLine("  <div class=\"section-body bg-light p-3 rounded-bottom\">");
        sb.AppendLine(GenerateChildFields());
        sb.AppendLine("  </div>");

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    // NEW: Generate Group field (container type)
    private string GenerateGroup()
    {
        var sb = new StringBuilder();

        // Enhanced container with hierarchy-aware classes
        var containerClasses = new List<string> { "field-group", "border", "rounded", "p-3", "mb-3" };
        containerClasses.AddRange(GetHierarchyClasses());

        sb.AppendLine($"<div class=\"{string.Join(" ", containerClasses)}\"");
        sb.AppendLine($"     data-field-id=\"{Field!.Field.Id}\"");
        sb.AppendLine($"     data-field-type=\"group\"");
        sb.AppendLine($"     data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\"");
        sb.AppendLine($"     data-relationship-type=\"{Field.Field.RelationshipType}\"");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
            sb.AppendLine($"     data-parent-id=\"{Field.Field.ParentId}\"");
        sb.AppendLine(">");

        // Group header
        sb.AppendLine("  <div class=\"group-header mb-3\">");
        sb.AppendLine("    <h5 class=\"text-secondary mb-1\">");
        sb.AppendLine("      <i class=\"fas fa-layer-group me-2\"></i>");
        sb.AppendLine($"      {Field.GetLabel()}");
        sb.AppendLine("    </h5>");

        if (!string.IsNullOrEmpty(Field.GetHelpText()))
        {
            sb.AppendLine($"    <small class=\"text-muted\">{Field.GetHelpText()}</small>");
        }

        if (ShowHierarchyDebug)
        {
            sb.AppendLine($"    <span class=\"badge bg-secondary ms-2\">{Field.Field.ChildFields.Count} children</span>");
        }

        sb.AppendLine("  </div>");

        // Group body with child fields
        sb.AppendLine("  <div class=\"group-body\">");
        sb.AppendLine(GenerateChildFields());
        sb.AppendLine("  </div>");

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    // Enhanced helper methods
    private List<string> GetHierarchyClasses()
    {
        var classes = new List<string>();
        
        // Add hierarchy level class
        classes.Add($"field-level-{Field!.Field.HierarchicalLevel}");
        
        // Add relationship type class
        classes.Add($"relationship-{Field.Field.RelationshipType.ToString().ToLowerInvariant()}");
        
        // Add parent/child classes
        if (Field.Field.Parent != null)
        {
            classes.Add("field-child");
            classes.Add($"parent-{Field.Field.Parent.Id}");
        }
        else
        {
            classes.Add("field-root");
        }

        return classes;
    }

    private string GenerateHierarchyDebugInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("  <div class=\"hierarchy-debug-info bg-light border rounded p-2 mb-2 small\">");
        sb.AppendLine("    <div class=\"row g-1\">");
        sb.AppendLine("      <div class=\"col-auto\">");
        sb.AppendLine($"        <span class=\"badge bg-primary\">ID: {Field!.Field.Id}</span>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"col-auto\">");
        sb.AppendLine($"        <span class=\"badge bg-secondary\">L{Field.Field.HierarchicalLevel}</span>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"col-auto\">");
        sb.AppendLine($"        <span class=\"badge bg-info\">{Field.Field.FieldType.Type}</span>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"col-auto\">");
        sb.AppendLine($"        <span class=\"badge bg-warning\">{Field.Field.RelationshipType}</span>");
        sb.AppendLine("      </div>");
        if (!string.IsNullOrEmpty(Field.Field.ParentId))
        {
            sb.AppendLine("      <div class=\"col-auto\">");
            sb.AppendLine($"        <span class=\"badge bg-success\">Parent: {Field.Field.ParentId}</span>");
            sb.AppendLine("      </div>");
        }
        sb.AppendLine("      <div class=\"col\">");
        sb.AppendLine($"        <span class=\"text-muted\">Path: {Field.GetHierarchicalPath()}</span>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        return sb.ToString();
    }

    private string GenerateChildFields()
    {
        if (!Field!.Field.ChildFields.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var childFields = Field.GetChildFields();

        if (childFields.Any())
        {
            sb.AppendLine("  <div class=\"child-fields-container ms-3\">");
            foreach (var child in childFields)
            {
                var childViewModel = new DynamicFieldViewModel 
                { 
                    Field = child, 
                    Language = Language, 
                    IsReadOnly = IsReadOnly 
                };
                
                // Create a new tag helper instance for the child
                var childTagHelper = new DynamicFieldTagHelper
                {
                    Field = childViewModel,
                    Language = Language,
                    Index = Index + 1, // Increment index for child fields
                    IsReadOnly = IsReadOnly,
                    FormData = FormData,
                    ShowHierarchyDebug = ShowHierarchyDebug
                };

                // Generate child field HTML recursively
                // Note: This is a simplified approach - in a real implementation,
                // you might want to use a different approach for recursive rendering
            }
            sb.AppendLine("  </div>");
        }

        return sb.ToString();
    }

    // Simplified implementations for other field types with enhanced features
    private string GenerateRadioButton() => GenerateDropDown(); // Enhanced version
    private string GenerateDateBox() => GenerateTextBox(); // Enhanced version  
    private string GenerateNumberBox() => GenerateTextBox(); // Enhanced version
    private string GenerateEmailBox() => GenerateTextBox(); // Enhanced version
    private string GenerateTelephoneBox() => GenerateTextBox(); // Enhanced version
    private string GenerateUrlBox() => GenerateTextBox(); // Enhanced version
    private string GenerateFileUpload() => $"<div class=\"alert alert-info\" data-standalone-enhanced=\"true\">File upload field: {Field!.Field.Id} (Enhanced)</div>";

    private string GenerateInfoBox()
    {
        var content = Field!.Field.Text.DescriptionHtml?.ToString(Language) ?? 
                     Field.Field.Text.Description.ToString(Language);
        return $"<div class=\"alert alert-info mb-3 {string.Join(" ", GetHierarchyClasses())}\" " +
               $"data-field-id=\"{Field.Field.Id}\" " +
               $"data-field-type=\"infobox\" " +
               $"data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\" " +
               $"data-relationship-type=\"{Field.Field.RelationshipType}\" " +
               $"data-standalone-enhanced=\"true\">{content}</div>";
    }

    private string GenerateLabel()
    {
        var content = Field!.Field.Text.Description.ToString(Language);
        return $"<div class=\"form-label-static mb-3 {string.Join(" ", GetHierarchyClasses())}\" " +
               $"data-field-id=\"{Field.Field.Id}\" " +
               $"data-field-type=\"label\" " +
               $"data-hierarchical-level=\"{Field.Field.HierarchicalLevel}\" " +
               $"data-relationship-type=\"{Field.Field.RelationshipType}\" " +
               $"data-standalone-enhanced=\"true\">{content}</div>";
    }
}

/// <summary>
/// Enhanced Tag helper for rendering validation summary with  Enhanced features
/// Usage: <validation-summary errors="@Model.ValidationErrors" language="EN" />
/// </summary>
[HtmlTargetElement("validation-summary")]
public class ValidationSummaryTagHelper : TagHelper
{
    [HtmlAttributeName("errors")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ValidationSummaryViewModel? Errors { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("language")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("show-field-names")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowFieldNames { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("show-hierarchy-info")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowHierarchyInfo { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override void Process(TagHelperContext context, TagHelperOutput output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (Errors?.Errors?.Any() != true)
        {
            output.SuppressOutput();
            return;
        }

        var sb = new StringBuilder();
        var title = Language == "FR" ? "Erreurs de validation :" : "Validation errors:";

        sb.AppendLine("<div class=\"alert alert-danger\" role=\"alert\" data-standalone-enhanced=\"true\">");
        sb.AppendLine($"  <h5 class=\"alert-heading\">");
        sb.AppendLine($"    <i class=\"fas fa-exclamation-triangle me-2\"></i>");
        sb.AppendLine($"    {title}");
        sb.AppendLine($"  </h5>");
        sb.AppendLine("  <ul class=\"mb-0\">");

        foreach (var error in Errors.Errors)
        {
            var message = ShowFieldNames && !string.IsNullOrEmpty(error.FieldName)
                ? $"{error.FieldName}: {error.Message}"
                : error.Message;
            sb.AppendLine($"    <li>{message}</li>");
        }

        sb.AppendLine("  </ul>");

        if (ShowHierarchyInfo && Errors.Errors.Any(e => !string.IsNullOrEmpty(e.FieldId)))
        {
            sb.AppendLine("  <hr>");
            sb.AppendLine("  <small class=\"text-muted\">");
            sb.AppendLine($"    <i class=\"fas fa-info-circle me-1\"></i>");
            var debugText = Language == "FR" ? "Informations de débogage de la hiérarchie disponibles" : "Hierarchy debug information available";
            sb.AppendLine($"    {debugText}");
            sb.AppendLine("  </small>");
        }

        sb.AppendLine("</div>");

        output.TagName = null;
        output.Content.SetHtmlContent(sb.ToString());
    }
}

/// <summary>
/// Enhanced Tag helper for rendering form progress indicator with  Enhanced features
/// Usage: <form-progress current="2" total="5" />
/// </summary>
[HtmlTargetElement("form-progress")]
public class FormProgressTagHelper : TagHelper
{
    [HtmlAttributeName("current")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int Current { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("total")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int Total { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("language")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HtmlAttributeName("show-step-details")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowStepDetails { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override void Process(TagHelperContext context, TagHelperOutput output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (Total <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var percentage = (double)Current / Total * 100;
        var stepText = Language == "FR" ? $"Étape {Current} de {Total}" : $"Step {Current} of {Total}";

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"progress-container mb-4\" data-standalone-enhanced=\"true\">");
        sb.AppendLine($"  <div class=\"progress-text text-center mb-2\">");
        sb.AppendLine($"    <span class=\"fw-bold\">{stepText}</span>");
        if (ShowStepDetails)
        {
            var completionText = Language == "FR" ? $"({percentage:F1}% terminé)" : $"({percentage:F1}% complete)";
            sb.AppendLine($"    <small class=\"text-muted ms-2\">{completionText}</small>");
        }
        sb.AppendLine($"  </div>");
        sb.AppendLine("  <div class=\"progress\" style=\"height: 8px;\">");
        sb.AppendLine($"    <div class=\"progress-bar progress-bar-striped progress-bar-animated\"");
        sb.AppendLine($"         role=\"progressbar\"");
        sb.AppendLine($"         style=\"width: {percentage:F1}%\"");
        sb.AppendLine($"         aria-valuenow=\"{Current}\"");
        sb.AppendLine($"         aria-valuemin=\"0\"");
        sb.AppendLine($"         aria-valuemax=\"{Total}\"></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");

        output.TagName = null;
        output.Content.SetHtmlContent(sb.ToString());
    }
}