using DynamicForms.Core.Entities;
using DynamicForms.Core.Entities.Data;
using DynamicForms.RazorPages.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace DynamicForms.RazorPages.Extensions;

/// <summary>
/// HTML helper extensions for rendering dynamic form fields
/// </summary>
public static class HtmlHelperExtensions
{
    /// <summary>
    /// Render a dynamic form field based on its type
    /// </summary>
    public static IHtmlContent DynamicField(this IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        return model.Field.FieldType.Type.ToLower() switch
        {
            "textbox" => RenderTextBox(htmlHelper, model),
            "textarea" => RenderTextArea(htmlHelper, model),
            "dropdown" or "dropdownlist" => RenderDropDown(htmlHelper, model),
            "checkbox" => RenderCheckBox(htmlHelper, model),
            "checkboxlist" => RenderCheckBoxList(htmlHelper, model),
            "radiobutton" => RenderRadioButton(htmlHelper, model),
            "datebox" => RenderDateBox(htmlHelper, model),
            "numberbox" or "numbertextbox" => RenderNumberBox(htmlHelper, model),
            "emailtextbox" => RenderEmailBox(htmlHelper, model),
            "telephonetextbox" => RenderTelephoneBox(htmlHelper, model),
            "urltextbox" => RenderUrlBox(htmlHelper, model),
            "fileupload" => RenderFileUpload(htmlHelper, model),
            "table" => RenderTable(htmlHelper, model),
            "infobox" => RenderInfoBox(htmlHelper, model),
            "label" => RenderLabel(htmlHelper, model),
            "speciesautocomplete" => RenderSpeciesAutoComplete(htmlHelper, model),
            _ => RenderTextBox(htmlHelper, model) // Default fallback
        };
    }

    /// <summary>
    /// Render a text input field
    /// </summary>
    public static IHtmlContent RenderTextBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{model.Field.Id}";
        var fieldName = $"Submission.Fields[{model.Index}].Value";
        
        // Container div
        sb.AppendLine($"<div class=\"form-group\" data-field-id=\"{model.Field.Id}\">");
        
        // Label
        if (!string.IsNullOrEmpty(model.GetLabel()))
        {
            sb.AppendLine($"<label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"  {model.GetLabel()}");
            if (model.Field.IsRequired)
                sb.AppendLine("  <span class=\"text-danger\">*</span>");
            sb.AppendLine("</label>");
        }
        
        // Input field
        sb.AppendLine($"<input type=\"text\" id=\"{fieldId}\" name=\"{fieldName}\"");
        sb.AppendLine($"       class=\"{model.GetCssClasses()}\"");
        sb.AppendLine($"       value=\"{model.GetStringValue()}\"");
        
        if (model.Field.MaximumLength.HasValue)
            sb.AppendLine($"       maxlength=\"{model.Field.MaximumLength}\"");
            
        if (!string.IsNullOrEmpty(model.GetPlaceholder()))
            sb.AppendLine($"       placeholder=\"{model.GetPlaceholder()}\"");
            
        if (model.Field.IsRequired)
            sb.AppendLine("       required");
            
        if (model.IsReadOnly || model.Field.ReadOnly)
            sb.AppendLine("       readonly");
            
        sb.AppendLine("       />");
        
        // Hidden field for field ID
        sb.AppendLine($"<input type=\"hidden\" name=\"Submission.Fields[{model.Index}].FieldId\" value=\"{model.Field.Id}\" />");
        
        // Help text
        if (!string.IsNullOrEmpty(model.GetHelpText()))
        {
            sb.AppendLine($"<div class=\"form-text\">{model.GetHelpText()}</div>");
        }
        
        // Validation errors
        if (model.HasValidationErrors)
        {
            sb.AppendLine($"<div class=\"invalid-feedback\">{model.GetValidationError()}</div>");
        }
        
        sb.AppendLine("</div>");
        
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Render a textarea field
    /// </summary>
    public static IHtmlContent RenderTextArea(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{model.Field.Id}";
        var fieldName = $"Submission.Fields[{model.Index}].Value";
        
        sb.AppendLine($"<div class=\"form-group\" data-field-id=\"{model.Field.Id}\">");
        
        // Label
        if (!string.IsNullOrEmpty(model.GetLabel()))
        {
            sb.AppendLine($"<label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"  {model.GetLabel()}");
            if (model.Field.IsRequired)
                sb.AppendLine("  <span class=\"text-danger\">*</span>");
            sb.AppendLine("</label>");
        }
        
        // Textarea
        sb.AppendLine($"<textarea id=\"{fieldId}\" name=\"{fieldName}\"");
        sb.AppendLine($"          class=\"{model.GetCssClasses()}\"");
        sb.AppendLine("          rows=\"4\"");
        
        if (model.Field.MaximumLength.HasValue)
            sb.AppendLine($"          maxlength=\"{model.Field.MaximumLength}\"");
            
        if (!string.IsNullOrEmpty(model.GetPlaceholder()))
            sb.AppendLine($"          placeholder=\"{model.GetPlaceholder()}\"");
            
        if (model.Field.IsRequired)
            sb.AppendLine("          required");
            
        if (model.IsReadOnly || model.Field.ReadOnly)
            sb.AppendLine("          readonly");
            
        sb.AppendLine($">{model.GetStringValue()}</textarea>");
        
        // Hidden field for field ID
        sb.AppendLine($"<input type=\"hidden\" name=\"Submission.Fields[{model.Index}].FieldId\" value=\"{model.Field.Id}\" />");
        
        // Help text and validation
        if (!string.IsNullOrEmpty(model.GetHelpText()))
            sb.AppendLine($"<div class=\"form-text\">{model.GetHelpText()}</div>");
            
        if (model.HasValidationErrors)
            sb.AppendLine($"<div class=\"invalid-feedback\">{model.GetValidationError()}</div>");
        
        sb.AppendLine("</div>");
        
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Render a dropdown/select field
    /// </summary>
    public static IHtmlContent RenderDropDown(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{model.Field.Id}";
        var fieldName = $"Submission.Fields[{model.Index}].Value";
        var currentValue = model.GetStringValue();
        
        sb.AppendLine($"<div class=\"form-group\" data-field-id=\"{model.Field.Id}\">");
        
        // Label
        if (!string.IsNullOrEmpty(model.GetLabel()))
        {
            sb.AppendLine($"<label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"  {model.GetLabel()}");
            if (model.Field.IsRequired)
                sb.AppendLine("  <span class=\"text-danger\">*</span>");
            sb.AppendLine("</label>");
        }
        
        // Select element
        sb.AppendLine($"<select id=\"{fieldId}\" name=\"{fieldName}\"");
        sb.AppendLine($"        class=\"{model.GetCssClasses()}\"");
        
        if (model.Field.IsRequired)
            sb.AppendLine("        required");
            
        if (model.IsReadOnly || model.Field.ReadOnly)
            sb.AppendLine("        disabled");
            
        sb.AppendLine(">");
        
        // Default option
        if (!model.Field.IsRequired)
        {
            sb.AppendLine($"  <option value=\"\">{(model.Language == "FR" ? "Sélectionnez..." : "Select...")}</option>");
        }
        
        // Options
        foreach (var option in model.Field.Options.OrderBy(o => o.Order))
        {
            var selected = option.Value == currentValue ? " selected" : "";
            var text = model.Language == "FR" ? option.FR : option.EN;
            sb.AppendLine($"  <option value=\"{option.Value}\"{selected}>{text}</option>");
        }
        
        sb.AppendLine("</select>");
        
        // Hidden field for field ID
        sb.AppendLine($"<input type=\"hidden\" name=\"Submission.Fields[{model.Index}].FieldId\" value=\"{model.Field.Id}\" />");
        
        // Help text and validation
        if (!string.IsNullOrEmpty(model.GetHelpText()))
            sb.AppendLine($"<div class=\"form-text\">{model.GetHelpText()}</div>");
            
        if (model.HasValidationErrors)
            sb.AppendLine($"<div class=\"invalid-feedback\">{model.GetValidationError()}</div>");
        
        sb.AppendLine("</div>");
        
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Render a checkbox field
    /// </summary>
    public static IHtmlContent RenderCheckBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var sb = new StringBuilder();
        var fieldId = $"field_{model.Field.Id}";
        var fieldName = $"Submission.Fields[{model.Index}].Value";
        var isChecked = model.GetStringValue().Equals("true", StringComparison.OrdinalIgnoreCase) || 
                       model.GetStringValue().Equals("1", StringComparison.OrdinalIgnoreCase);
        
        sb.AppendLine($"<div class=\"form-group form-check\" data-field-id=\"{model.Field.Id}\">");
        
        // Checkbox input
        sb.AppendLine($"<input type=\"checkbox\" id=\"{fieldId}\" name=\"{fieldName}\"");
        sb.AppendLine($"       class=\"form-check-input{(model.HasValidationErrors ? " is-invalid" : "")}\"");
        sb.AppendLine("       value=\"true\"");
        
        if (isChecked)
            sb.AppendLine("       checked");
            
        if (model.Field.IsRequired)
            sb.AppendLine("       required");
            
        if (model.IsReadOnly || model.Field.ReadOnly)
            sb.AppendLine("       disabled");
            
        sb.AppendLine("       />");
        
        // Hidden field for unchecked value
        sb.AppendLine($"<input type=\"hidden\" name=\"{fieldName}\" value=\"false\" />");
        
        // Label
        if (!string.IsNullOrEmpty(model.GetLabel()))
        {
            sb.AppendLine($"<label for=\"{fieldId}\" class=\"form-check-label\">");
            sb.AppendLine($"  {model.GetLabel()}");
            if (model.Field.IsRequired)
                sb.AppendLine("  <span class=\"text-danger\">*</span>");
            sb.AppendLine("</label>");
        }
        
        // Hidden field for field ID
        sb.AppendLine($"<input type=\"hidden\" name=\"Submission.Fields[{model.Index}].FieldId\" value=\"{model.Field.Id}\" />");
        
        // Help text and validation
        if (!string.IsNullOrEmpty(model.GetHelpText()))
            sb.AppendLine($"<div class=\"form-text\">{model.GetHelpText()}</div>");
            
        if (model.HasValidationErrors)
            sb.AppendLine($"<div class=\"invalid-feedback\">{model.GetValidationError()}</div>");
        
        sb.AppendLine("</div>");
        
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Render a checkbox list field
    /// </summary>
    public static IHtmlContent RenderCheckBoxList(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var sb = new StringBuilder();
        var selectedValues = model.Value?.Values?.Select(v => v.Value).ToList() ?? [];
        
        sb.AppendLine($"<div class=\"form-group\" data-field-id=\"{model.Field.Id}\">");
        
        // Label
        if (!string.IsNullOrEmpty(model.GetLabel()))
        {
            sb.AppendLine($"<label class=\"form-label\">");
            sb.AppendLine($"  {model.GetLabel()}");
            if (model.Field.IsRequired)
                sb.AppendLine("  <span class=\"text-danger\">*</span>");
            sb.AppendLine("</label>");
        }
        
        // Checkbox options
        foreach (var option in model.Field.Options.OrderBy(o => o.Order))
        {
            var optionId = $"field_{model.Field.Id}_{option.Value}";
            var fieldName = $"Submission.Fields[{model.Index}].MultiValues";
            var isChecked = selectedValues.Contains(option.Value);
            var text = model.Language == "FR" ? option.FR : option.EN;
            
            sb.AppendLine("  <div class=\"form-check\">");
            sb.AppendLine($"    <input type=\"checkbox\" id=\"{optionId}\" name=\"{fieldName}\"");
            sb.AppendLine($"           class=\"form-check-input\"");
            sb.AppendLine($"           value=\"{option.Value}\"");
            
            if (isChecked)
                sb.AppendLine("           checked");
                
            if (model.IsReadOnly || model.Field.ReadOnly)
                sb.AppendLine("           disabled");
                
            sb.AppendLine("           />");
            
            sb.AppendLine($"    <label for=\"{optionId}\" class=\"form-check-label\">{text}</label>");
            sb.AppendLine("  </div>");
        }
        
        // Hidden field for field ID
        sb.AppendLine($"<input type=\"hidden\" name=\"Submission.Fields[{model.Index}].FieldId\" value=\"{model.Field.Id}\" />");
        
        // Help text and validation
        if (!string.IsNullOrEmpty(model.GetHelpText()))
            sb.AppendLine($"<div class=\"form-text\">{model.GetHelpText()}</div>");
            
        if (model.HasValidationErrors)
            sb.AppendLine($"<div class=\"invalid-feedback\">{model.GetValidationError()}</div>");
        
        sb.AppendLine("</div>");
        
        return new HtmlString(sb.ToString());
    }

    // Additional render methods for other field types...
    
    private static IHtmlContent RenderDateBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for date input
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderNumberBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for number input
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderEmailBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for email input
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderTelephoneBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for telephone input
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderUrlBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for URL input
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderRadioButton(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for radio button group
        return RenderDropDown(htmlHelper, model); // Simplified for now
    }

    private static IHtmlContent RenderFileUpload(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for file upload
        return new HtmlString($"<div>File upload for field {model.Field.Id}</div>");
    }

    private static IHtmlContent RenderTable(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for table/modal
        return new HtmlString($"<div>Table/Modal for field {model.Field.Id}</div>");
    }

    private static IHtmlContent RenderInfoBox(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var content = model.Field.Text.DescriptionHtml?.ToString(model.Language) ?? 
                     model.Field.Text.Description.ToString(model.Language);
        return new HtmlString($"<div class=\"alert alert-info\">{content}</div>");
    }

    private static IHtmlContent RenderLabel(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        var content = model.Field.Text.Description.ToString(model.Language);
        return new HtmlString($"<div class=\"form-label-static\">{content}</div>");
    }

    private static IHtmlContent RenderSpeciesAutoComplete(IHtmlHelper htmlHelper, DynamicFieldViewModel model)
    {
        // Implementation for species autocomplete
        return RenderTextBox(htmlHelper, model); // Simplified for now
    }
}