using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Service for generating CSS from FormTheme configurations.
/// Converts theme settings into CSS custom properties (variables) and complete stylesheets.
/// </summary>
public interface IThemeCssGeneratorService
{
    /// <summary>
    /// Generates CSS custom properties (variables) as an inline style string.
    /// Use this for the style attribute of a ThemeScope wrapper element.
    /// </summary>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>CSS variables string suitable for a style attribute (e.g., "--df-primary: #6366F1; --df-bg: #FFFFFF;").</returns>
    string GenerateCssVariables(FormTheme theme);

    /// <summary>
    /// Generates a complete CSS stylesheet including variables and component styles.
    /// Use this for generating downloadable CSS files or style blocks.
    /// </summary>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>Complete CSS stylesheet with :root variables and component styles.</returns>
    string GenerateStylesheet(FormTheme theme);

    /// <summary>
    /// Generates a minified version of the complete stylesheet.
    /// Removes whitespace and comments for production use.
    /// </summary>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>Minified CSS stylesheet.</returns>
    string GenerateMinifiedCss(FormTheme theme);

    /// <summary>
    /// Gets the value of a specific CSS variable for a theme.
    /// </summary>
    /// <param name="theme">The theme configuration.</param>
    /// <param name="variableName">The CSS variable name (e.g., "--df-primary").</param>
    /// <returns>The variable value, or empty string if not found.</returns>
    string GetVariable(FormTheme theme, string variableName);

    /// <summary>
    /// Gets all CSS variable names and their values as a dictionary.
    /// Useful for debugging or displaying variable lists.
    /// </summary>
    /// <param name="theme">The theme configuration.</param>
    /// <returns>Dictionary of variable names to values.</returns>
    IReadOnlyDictionary<string, string> GetAllVariables(FormTheme theme);

    /// <summary>
    /// Clears the internal CSS generation cache.
    /// Call this if theme presets are modified at runtime.
    /// </summary>
    void ClearCache();
}
