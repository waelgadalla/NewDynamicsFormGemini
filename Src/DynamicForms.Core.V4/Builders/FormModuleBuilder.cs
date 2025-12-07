using DynamicForms.Core.V4.Schemas;
using System.Text.Json;

namespace DynamicForms.Core.V4.Builders;

/// <summary>
/// Fluent builder for creating FormModuleSchema instances.
/// </summary>
public class FormModuleBuilder
{
    private readonly int _id;
    private string _titleEn;
    private string? _titleFr;
    private int? _opportunityId;
    private string? _descriptionEn;
    private string? _descriptionFr;
    private string? _instructionsEn;
    private string? _instructionsFr;
    private readonly List<FormFieldSchema> _fields = new();
    private readonly List<FieldSetValidation> _crossFieldValidations = new();

    private FormModuleBuilder(int id, string titleEn)
    {
        _id = id;
        _titleEn = titleEn;
    }

    public static FormModuleBuilder Create(int id, string titleEn)
    {
        return new FormModuleBuilder(id, titleEn);
    }

    public FormModuleBuilder WithTitle(string en, string? fr = null)
    {
        _titleEn = en;
        _titleFr = fr;
        return this;
    }

    public FormModuleBuilder WithDescription(string en, string? fr = null)
    {
        _descriptionEn = en;
        _descriptionFr = fr;
        return this;
    }

    public FormModuleBuilder WithInstructions(string en, string? fr = null)
    {
        _instructionsEn = en;
        _instructionsFr = fr;
        return this;
    }

    public FormModuleBuilder ForOpportunity(int opportunityId)
    {
        _opportunityId = opportunityId;
        return this;
    }

    /// <summary>
    /// Adds a field using a builder.
    /// </summary>
    public FormModuleBuilder AddField(Func<FormFieldBuilder, FormFieldBuilder> builder)
    {
        var fieldBuilder = new FormFieldBuilder();
        var result = builder(fieldBuilder);
        _fields.Add(result.Build());
        return this;
    }

    /// <summary>
    /// Adds a section (and optionally configures its children).
    /// </summary>
    public FormModuleBuilder AddSection(string id, string titleEn, Action<SectionBuilder>? childBuilder = null)
    {
        var section = FormFieldSchema.CreateSection(id, titleEn);
        _fields.Add(section);

        if (childBuilder != null)
        {
            var sb = new SectionBuilder(id);
            childBuilder(sb);
            _fields.AddRange(sb.Build());
        }

        return this;
    }

    /// <summary>
    /// Adds a "One of these is required" validation rule.
    /// </summary>
    public FormModuleBuilder RequireOneOf(string[] fieldIds, string? messageEn = null, string? messageFr = null)
    {
        _crossFieldValidations.Add(new FieldSetValidation
        {
            Type = "AtLeastOne",
            FieldIds = fieldIds,
            ErrorMessageEn = messageEn,
            ErrorMessageFr = messageFr
        });
        return this;
    }

    public FormModuleSchema Build()
    {
        return new FormModuleSchema
        {
            Id = _id,
            TitleEn = _titleEn,
            TitleFr = _titleFr,
            OpportunityId = _opportunityId,
            DescriptionEn = _descriptionEn,
            DescriptionFr = _descriptionFr,
            InstructionsEn = _instructionsEn,
            InstructionsFr = _instructionsFr,
            Fields = _fields.ToArray(),
            CrossFieldValidations = _crossFieldValidations.Any() ? _crossFieldValidations.ToArray() : null
        };
    }
}

/// <summary>
/// Helper builder specifically for adding children to a section.
/// </summary>
public class SectionBuilder
{
    private readonly string _parentId;
    private readonly List<FormFieldSchema> _children = new();

    public SectionBuilder(string parentId)
    {
        _parentId = parentId;
    }

    public SectionBuilder AddText(string id, string labelEn, bool required = false)
    {
        var field = FormFieldSchema.CreateTextField(id, labelEn, isRequired: required);
        _children.Add(field with { ParentId = _parentId });
        return this;
    }

    public SectionBuilder AddField(Func<FormFieldBuilder, FormFieldBuilder> builder)
    {
        var fieldBuilder = new FormFieldBuilder();
        var result = builder(fieldBuilder);
        var field = result.Build();
        _children.Add(field with { ParentId = _parentId });
        return this;
    }

    public IEnumerable<FormFieldSchema> Build() => _children;
}
