using Elsa.Studio.Abstractions;
using Elsa.Studio.Contracts;

namespace ElsaProCombined.Client;

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
                Primary = new("#1e40af"),
                Secondary = new("#7c3aed"),
                Tertiary = new("#0f766e"),
                Info = new("#0284c7"),
                Success = new("#16a34a"),
                Warning = new("#ea580c"),
                Error = new("#dc2626"),
                Dark = new("#1e293b"),
                AppbarBackground = new("#f1f5f9"),
                AppbarText = new("#0f172a"),
                DrawerBackground = new("#f1f5f9"),
                DrawerText = new("#0f172a"),
                Background = new("#ffffff"),
                BackgroundGray = new("#f8fafc"),
                Surface = new("#ffffff"),
                TextPrimary = new("#0f172a"),
                TextSecondary = new("#475569"),
                TextDisabled = new("#cbd5e1"),
                Divider = new("#e2e8f0"),
                DividerLight = new("#f1f5f9"),
                LinesDefault = new("#cbd5e1"),
                LinesInputs = new("#cbd5e1"),
                ActionDefault = new("#334155"),
                ActionDisabled = new("#cbd5e1"),
                ActionDisabledBackground = new("#f1f5f9"),
                TableLines = new("#e2e8f0"),
                TableStriped = new("#f8fafc"),
                TableHover = new("#f1f5f9"),
            },
            PaletteDark =
            {
                Primary = new("#3b82f6"),
                Secondary = new("#a78bfa"),
                Tertiary = new("#14b8a6"),
                Info = new("#38bdf8"),
                Success = new("#22c55e"),
                Warning = new("#fb923c"),
                Error = new("#f87171"),
                Dark = new("#f8fafc"),
                AppbarBackground = new("rgba(11, 18, 32, 0.9)"),
                AppbarText = new("#f1f5f9"),
                DrawerBackground = new("#1c2738"),
                DrawerText = new("#f1f5f9"),
                Background = new("#0f172a"),
                BackgroundGray = new("#162235"),
                Surface = new("#223045"),
                TextPrimary = new("#f1f5f9"),
                TextSecondary = new("#94a3b8"),
                TextDisabled = new("#475569"),
                Divider = new("#334155"),
                DividerLight = new("#223045"),
                LinesDefault = new("#475569"),
                LinesInputs = new("#475569"),
                ActionDefault = new("#94a3b8"),
                ActionDisabled = new("#475569"),
                ActionDisabledBackground = new("#223045"),
                TableLines = new("#334155"),
                TableStriped = new("#223045"),
                TableHover = new("#2a3a54"),
            }
        };

        return default;
    }
}
