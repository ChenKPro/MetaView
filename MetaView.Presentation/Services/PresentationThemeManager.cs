using System;
using System.Linq;
using System.Windows;
using MetaView.Presentation.Core;

namespace MetaView.Presentation.Services;

/// <summary>
/// Applies WPF resource dictionaries that define the shell visual style.
/// </summary>
public sealed class PresentationThemeManager : IPresentationThemeManager
{
    private static readonly Uri IndustrialDarkUri = new(
        "/MetaView.Presentation;component/Themes/PresentationResources.xaml",
        UriKind.Relative);

    /// <inheritdoc />
    public void ApplyTheme(string themeName)
    {
        var themeUri = ResolveThemeUri(themeName);
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(dictionary => dictionary.Source == themeUri);

        if (existingTheme is not null)
        {
            dictionaries.Remove(existingTheme);
        }

        dictionaries.Add(new ResourceDictionary { Source = themeUri });
    }

    private static Uri ResolveThemeUri(string themeName)
    {
        return themeName switch
        {
            PresentationThemeNames.IndustrialDark => IndustrialDarkUri,
            _ => throw new ArgumentException($"Unknown presentation theme '{themeName}'.", nameof(themeName))
        };
    }
}
