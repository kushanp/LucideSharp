using SkiaSharp;
using Svg.Skia;

namespace LucideSharp.WinForms.Rendering;

internal sealed class SvgSkiaRenderer : IIconRenderer
{
    public static SvgSkiaRenderer Instance { get; } = new();

    public RenderEngine Engine => RenderEngine.SvgSkia;

    public Bitmap Render(string svgContent, int size)
    {
        using var svg = new SKSvg();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
        if (svg.Load(stream) is null || svg.Picture is null)
        {
            return CreateFallbackBitmap(size);
        }

        var bounds = svg.Picture.CullRect;
        var sourceWidth = Math.Max(bounds.Width, 1f);
        var sourceHeight = Math.Max(bounds.Height, 1f);
        var scale = Math.Min(size / sourceWidth, size / sourceHeight);

        var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using var surface = SKSurface.Create(new SKImageInfo(size, size, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var offsetX = (size - sourceWidth * scale) / 2f - bounds.Left * scale;
        var offsetY = (size - sourceHeight * scale) / 2f - bounds.Top * scale;

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale);
        canvas.DrawPicture(svg.Picture);
        canvas.Restore();

        using var image = surface.Snapshot();
        using var data = image.PeekPixels();
        if (data is null)
        {
            return CreateFallbackBitmap(size);
        }

        var result = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        var rect = new Rectangle(0, 0, size, size);
        var bmpData = result.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);
        try
        {
            data.ReadPixels(new SKImageInfo(size, size, SKColorType.Bgra8888, SKAlphaType.Premul), bmpData.Scan0, bmpData.Stride);
        }
        finally
        {
            result.UnlockBits(bmpData);
        }

        return result;
    }

    private static Bitmap CreateFallbackBitmap(int size)
    {
        var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        return bitmap;
    }
}