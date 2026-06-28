namespace LucideSharp.WinForms;

internal readonly struct SplitterHitTestResult
{
    public static SplitterHitTestResult None { get; } = new(-1, SplitterButtonKind.None, false);

    public int SplitterIndex { get; }
    public SplitterButtonKind ButtonKind { get; }
    public bool IsDragRegion { get; }

    public bool IsSplitter => SplitterIndex >= 0;

    public SplitterHitTestResult(int splitterIndex, SplitterButtonKind buttonKind, bool isDragRegion)
    {
        SplitterIndex = splitterIndex;
        ButtonKind = buttonKind;
        IsDragRegion = isDragRegion;
    }
}