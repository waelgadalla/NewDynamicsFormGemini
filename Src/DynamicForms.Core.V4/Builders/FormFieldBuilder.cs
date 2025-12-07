using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Enums;

namespace DynamicForms.Core.V4.Builders;

public class FormFieldBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _type = "TextBox";
    private string? _labelEn;
    private string? _labelFr;
    private bool _isRequired;
    private int? _minLength;
    private int? _maxLength;
    private string? _parentId;
    private int _order = 1;
    private string? _ariaLabelEn;
    private string? _ariaLabelFr;
    private string? _ariaRole;
    private readonly List<FieldOption> _options = new();

    public FormFieldBuilder Id(string id)
    {
        _id = id;
        return this;
    }

    public FormFieldBuilder Type(string type)
    {
        _type = type;
        return this;
    }

    public FormFieldBuilder Label(string en, string? fr = null)
    {
        _labelEn = en;
        _labelFr = fr;
        return this;
    }

    public FormFieldBuilder Required(bool required = true)
    {
        _isRequired = required;
        return this;
    }

    public FormFieldBuilder Length(int? min, int? max)
    {
        _minLength = min;
        _maxLength = max;
        return this;
    }

    public FormFieldBuilder Parent(string parentId)
    {
        _parentId = parentId;
        return this;
    }

    public FormFieldBuilder Order(int order)
    {
        _order = order;
        return this;
    }

    public FormFieldBuilder WithAria(string role, string? labelEn = null, string? labelFr = null)
    {
        _ariaRole = role;
        _ariaLabelEn = labelEn;
        _ariaLabelFr = labelFr;
        return this;
    }

    public FormFieldBuilder AddOption(string value, string textEn, string? textFr = null)
    {
        _options.Add(new FieldOption(value, textEn, textFr ?? textEn));
        return this;
    }

    public FormFieldSchema Build()
    {
        return new FormFieldSchema
        {
            Id = _id,
            FieldType = _type,
            LabelEn = _labelEn,
            LabelFr = _labelFr,
            Order = _order,
            ParentId = _parentId,
            Options = _options.Any() ? _options.ToArray() : null,
            Validation = new FieldValidationConfig
            {
                IsRequired = _isRequired,
                MinLength = _minLength,
                MaxLength = _maxLength
            },
            Accessibility = (_ariaRole != null || _ariaLabelEn != null) ? new AccessibilityConfig
            {
                AriaRole = _ariaRole,
                AriaLabelEn = _ariaLabelEn,
                AriaLabelFr = _ariaLabelFr
            } : null
        };
    }
}
