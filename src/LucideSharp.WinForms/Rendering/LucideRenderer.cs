namespace LucideSharp.WinForms.Rendering;

internal static class LucideRenderer
{
    private static readonly IIconRenderer SkiaRenderer = SvgSkiaRenderer.Instance;
    private static readonly IIconRenderer ClassicRenderer = ClassicSvgRenderer.Instance;

    public static Bitmap Render(IconRenderOptions options)
    {
        var cacheOptions = options;
        return IconCache.GetOrAdd(cacheOptions, RenderCore);
    }

    public static Bitmap RenderWithoutCache(IconRenderOptions options) => RenderCore(options);

    private static Bitmap RenderCore(IconRenderOptions options)
    {
        var svg = LucideIconData.GetSvg(options.Kind);
        var themedSvg = SvgPreprocessor.ApplyTheme(svg, options.ForeColor, options.StrokeWidth);
        var renderer = options.RenderEngine == RenderEngine.ClassicSvg ? ClassicRenderer : SkiaRenderer;
        var bitmap = renderer.Render(themedSvg, options.IconSize);

        if (options.Flip == FlipMode.None && options.Rotation.Equals(0f))
        {
            return bitmap;
        }

        return ApplyTransform(bitmap, options.Rotation, options.Flip);
    }

    private static Bitmap ApplyTransform(Bitmap source, float rotation, FlipMode flip)
    {
        var result = new Bitmap(source.Width, source.Height, source.PixelFormat);
        result.SetResolution(source.HorizontalResolution, source.VerticalResolution);

        using var graphics = Graphics.FromImage(result);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        var centerX = source.Width / 2f;
        var centerY = source.Height / 2f;

        graphics.TranslateTransform(centerX, centerY);

        var scaleX = flip is FlipMode.Horizontal or FlipMode.Both ? -1f : 1f;
        var scaleY = flip is FlipMode.Vertical or FlipMode.Both ? -1f : 1f;
        if (scaleX < 0f || scaleY < 0f)
        {
            graphics.ScaleTransform(scaleX, scaleY);
        }

        if (!rotation.Equals(0f))
        {
            graphics.RotateTransform(rotation);
        }

        graphics.TranslateTransform(-centerX, -centerY);
        graphics.DrawImage(source, 0, 0, source.Width, source.Height);
        source.Dispose();

        return result;
    }
}