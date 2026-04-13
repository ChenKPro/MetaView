namespace MetaView.Presentation.Services;

/// <summary>
/// Applies presentation resource dictionaries for the current shell theme.
/// </summary>
public interface IPresentationThemeManager
{
    /// <summary>
    /// Applies the specified presentation theme.
    /// </summary>
    /// <param name="themeName">The theme identifier from <c>PresentationThemeNames</c>.</param>
    void ApplyTheme(string themeName);
}
