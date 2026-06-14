using System.Globalization;
using System.Text.RegularExpressions;

namespace LucideSharp.WinForms.Rendering;

internal static class SvgPreprocessor
{
    private static readonly Regex StrokeWidthRegex = new(
        @"stroke-width\s*=\s*""[^""]*""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StrokeCurrentColorRegex = new(
        @"stroke\s*=\s*""currentColor""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FillCurrentColorRegex = new(
        @"fill\s*=\s*""currentColor""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string ApplyTheme(string svg, Color color, float strokeWidth)
    {
        if (string.IsNullOrWhiteSpace(svg))
        {
            return svg;
        }

        var colorHex = ColorToHex(color);
        var themed = svg.Replace("currentColor", colorHex);

        themed = StrokeCurrentColorRegex.Replace(themed, $@"stroke=""{colorHex}""");
        themed = FillCurrentColorRegex.Replace(themed, $@"fill=""{colorHex}""");
        themed = StrokeWidthRegex.Replace(themed, $@"stroke-width=""{strokeWidth.ToString(CultureInfo.InvariantCulture)}""");

        return themed;
    }

    private static string ColorToHex(Color color) =>
        color.A < 255
            ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}