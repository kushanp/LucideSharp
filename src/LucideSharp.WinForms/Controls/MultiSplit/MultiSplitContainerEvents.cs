namespace LucideSharp.WinForms;

/// <summary>Event data raised while a splitter is being dragged.</summary>
public sealed class SplitterMovingEventArgs : EventArgs
{
    public int SplitterIndex { get; }
    public int Delta { get; }

    public SplitterMovingEventArgs(int splitterIndex, int delta)
    {
        SplitterIndex = splitterIndex;
        Delta = delta;
    }
}

/// <summary>Event data raised after a splitter drag completes.</summary>
public sealed class SplitterMovedEventArgs : EventArgs
{
    public int SplitterIndex { get; }
    public MultiSplitPanel BeforePanel { get; }
    public MultiSplitPanel AfterPanel { get; }

    public SplitterMovedEventArgs(int splitterIndex, MultiSplitPanel beforePanel, MultiSplitPanel afterPanel)
    {
        SplitterIndex = splitterIndex;
        BeforePanel = beforePanel;
        AfterPanel = afterPanel;
    }
}

/// <summary>Event data raised when a panel is collapsed.</summary>
public sealed class PanelCollapsedEventArgs : EventArgs
{
    public int PanelIndex { get; }
    public MultiSplitPanel Panel { get; }

    public PanelCollapsedEventArgs(int panelIndex, MultiSplitPanel panel)
    {
        PanelIndex = panelIndex;
        Panel = panel;
    }
}

/// <summary>Event data raised when a panel is restored.</summary>
public sealed class PanelRestoredEventArgs : EventArgs
{
    public int PanelIndex { get; }
    public MultiSplitPanel Panel { get; }

    public PanelRestoredEventArgs(int panelIndex, MultiSplitPanel panel)
    {
        PanelIndex = panelIndex;
        Panel = panel;
    }
}