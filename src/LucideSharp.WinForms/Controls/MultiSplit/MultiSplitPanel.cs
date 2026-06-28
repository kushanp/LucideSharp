using System.ComponentModel;

namespace LucideSharp.WinForms;

/// <summary>
/// A panel hosted inside a <see cref="MultiSplitContainer"/>.
/// </summary>
[Designer(typeof(Design.MultiSplitPanelDesigner))]
[ToolboxItem(false)]
public class MultiSplitPanel : Panel
{
    private bool _collapsed;
    private int _collapsedSize;
    private int _splitSize = 100;
    private int _minimumSplitSize = 25;
    internal int LastNonCollapsedSplitSize { get; private set; } = 100;
    internal MultiSplitContainer? Owner { get; set; }
    internal bool SuppressOwnerNotification { get; set; }

    public MultiSplitPanel()
    {
        TabStop = false;
        LastNonCollapsedSplitSize = _splitSize;
    }

    /// <summary>Gets or sets whether the panel is collapsed along the split axis.</summary>
    [DefaultValue(false)]
    [Category("Layout")]
    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            if (_collapsed == value)
            {
                return;
            }

            _collapsed = value;
            if (!SuppressOwnerNotification && Owner is not null)
            {
                if (value)
                {
                    var index = Owner.Panels.IndexOf(this);
                    if (index >= 0)
                    {
                        Owner.CollapsePanel(index);
                    }
                }
                else
                {
                    var index = Owner.Panels.IndexOf(this);
                    if (index >= 0)
                    {
                        Owner.RestorePanel(index);
                    }
                }
            }
        }
    }

    /// <summary>Gets or sets the pixel size used when collapsed.</summary>
    [DefaultValue(0)]
    [Category("Layout")]
    public int CollapsedSize
    {
        get => _collapsedSize;
        set => _collapsedSize = Math.Max(0, value);
    }

    /// <summary>Gets or sets the preferred size along the split axis.</summary>
    [DefaultValue(100)]
    [Category("Layout")]
    public int SplitSize
    {
        get => _splitSize;
        set => _splitSize = Math.Max(0, value);
    }

    /// <summary>Gets or sets the minimum size along the split axis.</summary>
    [DefaultValue(25)]
    [Category("Layout")]
    public int MinimumSplitSize
    {
        get => _minimumSplitSize;
        set
        {
            _minimumSplitSize = Math.Max(0, value);
            ApplyMinimumSize();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override DockStyle Dock
    {
        get => base.Dock;
        set => base.Dock = DockStyle.None;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override AnchorStyles Anchor
    {
        get => base.Anchor;
        set => base.Anchor = AnchorStyles.None;
    }

    internal void RememberSplitSize()
    {
        if (!_collapsed && _splitSize > 0)
        {
            LastNonCollapsedSplitSize = _splitSize;
        }
    }

    internal int GetMinimumSplitSize(Orientation orientation) =>
        orientation == Orientation.Vertical ? MinimumSplitSize : MinimumSplitSize;

    internal void ApplyMinimumSize()
    {
        if (Owner is null)
        {
            return;
        }

        if (Owner.Orientation == Orientation.Vertical)
        {
            MinimumSize = new Size(_minimumSplitSize, 0);
        }
        else
        {
            MinimumSize = new Size(0, _minimumSplitSize);
        }
    }
}