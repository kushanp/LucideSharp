namespace LucideSharp.WinForms.Rendering;

/// <summary>
/// Describes how an icon should be rendered.
/// </summary>
public readonly struct IconRenderOptions : IEquatable<IconRenderOptions>
{
    public LucideKind Kind { get; }
    public int IconSize { get; }
    public Color ForeColor { get; }
    public float StrokeWidth { get; }
    public float Rotation { get; }
    public FlipMode Flip { get; }
    public RenderEngine RenderEngine { get; }

    public IconRenderOptions(
        LucideKind kind = LucideKind.Heart,
        int iconSize = 24,
        Color? foreColor = null,
        float strokeWidth = 2f,
        float rotation = 0f,
        FlipMode flip = FlipMode.None,
        RenderEngine renderEngine = RenderEngine.SvgSkia)
    {
        Kind = kind;
        IconSize = iconSize;
        ForeColor = foreColor ?? Color.Black;
        StrokeWidth = strokeWidth;
        Rotation = rotation;
        Flip = flip;
        RenderEngine = renderEngine;
    }

    public bool Equals(IconRenderOptions other) =>
        Kind == other.Kind &&
        IconSize == other.IconSize &&
        ForeColor.ToArgb() == other.ForeColor.ToArgb() &&
        StrokeWidth.Equals(other.StrokeWidth) &&
        Rotation.Equals(other.Rotation) &&
        Flip == other.Flip &&
        RenderEngine == other.RenderEngine;

    public override bool Equals(object? obj) => obj is IconRenderOptions other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (int)Kind;
            hash = (hash * 397) ^ IconSize;
            hash = (hash * 397) ^ ForeColor.ToArgb();
            hash = (hash * 397) ^ StrokeWidth.GetHashCode();
            hash = (hash * 397) ^ Rotation.GetHashCode();
            hash = (hash * 397) ^ (int)Flip;
            hash = (hash * 397) ^ (int)RenderEngine;
            return hash;
        }
    }
}