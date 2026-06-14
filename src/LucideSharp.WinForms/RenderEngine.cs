namespace LucideSharp.WinForms;

/// <summary>
/// Specifies which SVG rendering backend to use.
/// </summary>
public enum RenderEngine
{
    /// <summary>
    /// High-quality rendering using Svg.Skia and SkiaSharp (recommended).
    /// </summary>
    SvgSkia = 0,

    /// <summary>
    /// Classic rendering using the Svg (Svg.NET) package.
    /// </summary>
    ClassicSvg = 1
}