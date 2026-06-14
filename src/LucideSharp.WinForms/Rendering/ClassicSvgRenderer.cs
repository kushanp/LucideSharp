extern alias ClassicSvg;

namespace LucideSharp.WinForms.Rendering;

internal sealed class ClassicSvgRenderer : IIconRenderer
{
    public static ClassicSvgRenderer Instance { get; } = new();

    public RenderEngine Engine => RenderEngine.ClassicSvg;

    public Bitmap Render(string svgContent, int size)
    {
        try
        {
            var document = ClassicSvg::Svg.SvgDocument.FromSvg<ClassicSvg::Svg.SvgDocument>(svgContent);
            document.Width = size;
            document.Height = size;
            return document.Draw(size, size);
        }
        catch
        {
            var bitmap = new Bitmap(size, size);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            return bitmap;
        }
    }
}