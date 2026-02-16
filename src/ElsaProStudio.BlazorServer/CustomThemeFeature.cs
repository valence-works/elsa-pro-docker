using Elsa.Studio.Abstractions;
using Elsa.Studio.Contracts;

namespace ElsaProStudio.BlazorServer;

/// <summary>
/// Registers the workflow designer feature with the application shell.
/// </summary>
public class CustomThemeFeature(IThemeService themeService) : FeatureBase
{
    public override ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        themeService.CurrentTheme = new()
        {
            LayoutProperties =
            {
                DefaultBorderRadius = "8px",
            },
            PaletteLight =
            {
                Primary = new("#1e40af"), // Deep professional blue
                Secondary = new("#7c3aed"), // Elegant purple
                Tertiary = new("#0f766e"), // Teal accent
                Info = new("#0284c7"), // Sky blue
                Success = new("#16a34a"), // Green
                Warning = new("#ea580c"), // Orange
                Error = new("#dc2626"), // Red
                Dark = new("#1e293b"),

                // App chrome
                AppbarBackground = new("#f1f5f9"),
                AppbarText = new("#0f172a"),
                DrawerBackground = new("#f1f5f9"),
                DrawerText = new("#0f172a"),

                // Surfaces
                Background = new("#ffffff"),
                BackgroundGray = new("#f8fafc"),
                Surface = new("#ffffff"),

                // Text
                TextPrimary = new("#0f172a"),
                TextSecondary = new("#475569"),
                TextDisabled = new("#cbd5e1"),

                // Borders & lines
                Divider = new("#e2e8f0"),
                DividerLight = new("#f1f5f9"),
                LinesDefault = new("#cbd5e1"),
                LinesInputs = new("#cbd5e1"),

                // Interactions
                ActionDefault = new("#334155"),
                ActionDisabled = new("#cbd5e1"),
                ActionDisabledBackground = new("#f1f5f9"),

                // Tables
                TableLines = new("#e2e8f0"),
                TableStriped = new("#f8fafc"),
                TableHover = new("#f1f5f9"),
            },
            PaletteDark =
            {
                Primary = new("#3b82f6"), // Brighter blue for dark mode
                Secondary = new("#a78bfa"), // Lighter purple
                Tertiary = new("#14b8a6"), // Bright teal
                Info = new("#38bdf8"), // Bright sky blue
                Success = new("#22c55e"), // Bright green
                Warning = new("#fb923c"), // Bright orange
                Error = new("#f87171"), // Bright red
                Dark = new("#f8fafc"),

                // App chrome
                AppbarBackground = new("rgba(11, 18, 32, 0.9)"),
                AppbarText = new("#f1f5f9"),
                DrawerBackground = new("#1c2738"),
                DrawerText = new("#f1f5f9"),

                // Surfaces
                Background = new("#0f172a"),
                BackgroundGray = new("#162235"),
                Surface = new("#223045"),

                // Text
                TextPrimary = new("#f1f5f9"),
                TextSecondary = new("#94a3b8"),
                TextDisabled = new("#475569"),

                // Borders & lines
                Divider = new("#334155"),
                DividerLight = new("#223045"),
                LinesDefault = new("#475569"),
                LinesInputs = new("#475569"),

                // Interactions
                ActionDefault = new("#94a3b8"),
                ActionDisabled = new("#475569"),
                ActionDisabledBackground = new("#223045"),

                // Tables
                TableLines = new("#334155"),
                TableStriped = new("#223045"),
                TableHover = new("#2a3a54"),
            }
        };
        
        return default;
    }
}