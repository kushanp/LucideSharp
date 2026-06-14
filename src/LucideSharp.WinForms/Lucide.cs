using LucideSharp.WinForms.Rendering;

namespace LucideSharp.WinForms;

/// <summary>
/// Static helper methods for rendering Lucide icons.
/// </summary>
public static class Lucide
{
    /// <summary>
    /// Renders a Lucide icon to a cached bitmap.
    /// </summary>
    public static Bitmap GetBitmap(
        LucideKind kind,
        int iconSize = 24,
        Color? foreColor = null,
        float strokeWidth = 2f,
        float rotation = 0f,
        FlipMode flip = FlipMode.None,
        RenderEngine renderEngine = RenderEngine.SvgSkia)
    {
        var options = new IconRenderOptions(kind, iconSize, foreColor, strokeWidth, rotation, flip, renderEngine);
        return (Bitmap)LucideRenderer.Render(options).Clone();
    }

    /// <summary>
    /// Renders a Lucide icon to an <see cref="Image"/>.
    /// </summary>
    public static Image GetImage(
        LucideKind kind,
        int iconSize = 24,
        Color? foreColor = null,
        float strokeWidth = 2f,
        float rotation = 0f,
        FlipMode flip = FlipMode.None,
        RenderEngine renderEngine = RenderEngine.SvgSkia) =>
        GetBitmap(kind, iconSize, foreColor, strokeWidth, rotation, flip, renderEngine);

    /// <summary>
    /// Creates a configured <see cref="LucideIcon"/> control instance.
    /// </summary>
    public static LucideIcon GetIcon(
        LucideKind kind,
        int iconSize = 24,
        Color? foreColor = null,
        float strokeWidth = 2f,
        float rotation = 0f,
        FlipMode flip = FlipMode.None,
        RenderEngine renderEngine = RenderEngine.SvgSkia,
        bool spin = false)
    {
        return new LucideIcon
        {
            Kind = kind,
            IconSize = iconSize,
            ForeColor = foreColor ?? Color.Black,
            StrokeWidth = strokeWidth,
            Rotation = rotation,
            Flip = flip,
            RenderEngine = renderEngine,
            Spin = spin
        };
    }

    /// <summary>
    /// Clears the internal bitmap cache.
    /// </summary>
    public static void ClearCache() => IconCache.Clear();
}