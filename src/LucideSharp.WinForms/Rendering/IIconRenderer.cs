namespace LucideSharp.WinForms.Rendering;

internal interface IIconRenderer
{
    RenderEngine Engine { get; }

    Bitmap Render(string svgContent, int size);
}