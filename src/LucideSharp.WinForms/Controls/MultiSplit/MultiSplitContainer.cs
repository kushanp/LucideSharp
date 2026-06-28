using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace LucideSharp.WinForms;

/// <summary>
/// A container that hosts multiple resizable panels separated by draggable splitter bars.
/// </summary>
[Designer(typeof(Design.MultiSplitContainerDesigner))]
[DefaultProperty(nameof(Panels))]
[DefaultEvent(nameof(SplitterMoved))]
[ToolboxItem(true)]
public class MultiSplitContainer : ContainerControl
{
    private readonly MultiSplitPanelCollection _panels;
    private readonly List<Rectangle> _splitterRects = new();
    private readonly List<Rectangle[]> _buttonRects = new();

    private Orientation _orientation = Orientation.Vertical;
    private int _splitterWidth = 7;
    private Color _splitterBackColor = Color.FromArgb(236, 236, 236);
    private Color _splitterHoverBackColor = Color.FromArgb(210, 228, 250);
    private Color _splitterButtonBackColor = Color.FromArgb(245, 245, 245);
    private Color _splitterButtonHoverBackColor = Color.FromArgb(200, 220, 252);
    private Color _splitterButtonBorderColor = Color.FromArgb(160, 160, 160);
    private readonly Color _splitterButtonPressedBackColor = Color.FromArgb(0, 120, 215);

    private int _hoveredSplitterIndex = -1;
    private int _hoveredButton = -1;
    private int _pressedSplitterIndex = -1;
    private int _pressedButton = -1;
    private int _dragSplitterIndex = -1;
    private int _dragStartCoordinate;
    private int _dragStartBeforeSize;
    private int _dragStartAfterSize;
    private bool _layoutSuspended;

    public MultiSplitContainer()
    {
        _panels = new MultiSplitPanelCollection(this);
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
        TabStop = false;
        Size = new Size(400, 300);
    }

    /// <summary>Gets the hosted panels.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Category("Behavior")]
    public MultiSplitPanelCollection Panels => _panels;

    /// <summary>Gets or sets the split direction.</summary>
    [DefaultValue(Orientation.Vertical)]
    [Category("Layout")]
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value)
            {
                return;
            }

            _orientation = value;
            foreach (MultiSplitPanel panel in _panels)
            {
                panel.ApplyMinimumSize();
            }

            PerformLayoutAndInvalidate();
        }
    }

    /// <summary>Gets or sets the splitter bar thickness in pixels.</summary>
    [DefaultValue(7)]
    [Category("Appearance")]
    public int SplitterWidth
    {
        get => _splitterWidth;
        set
        {
            _splitterWidth = Math.Max(4, value);
            PerformLayoutAndInvalidate();
        }
    }

    /// <summary>Gets or sets the splitter bar background color.</summary>
    [Category("Appearance")]
    public Color SplitterBackColor
    {
        get => _splitterBackColor;
        set
        {
            _splitterBackColor = value;
            Invalidate();
        }
    }

    /// <summary>Gets or sets the splitter bar hover color.</summary>
    [Category("Appearance")]
    public Color SplitterHoverBackColor
    {
        get => _splitterHoverBackColor;
        set
        {
            _splitterHoverBackColor = value;
            Invalidate();
        }
    }

    /// <summary>Gets or sets the splitter button background color.</summary>
    [Category("Appearance")]
    public Color SplitterButtonBackColor
    {
        get => _splitterButtonBackColor;
        set
        {
            _splitterButtonBackColor = value;
            Invalidate();
        }
    }

    /// <summary>Gets or sets the splitter button hover color.</summary>
    [Category("Appearance")]
    public Color SplitterButtonHoverBackColor
    {
        get => _splitterButtonHoverBackColor;
        set
        {
            _splitterButtonHoverBackColor = value;
            Invalidate();
        }
    }

    /// <summary>Gets or sets the splitter button border color.</summary>
    [Category("Appearance")]
    public Color SplitterButtonBorderColor
    {
        get => _splitterButtonBorderColor;
        set
        {
            _splitterButtonBorderColor = value;
            Invalidate();
        }
    }

    /// <summary>Raised while a splitter is being dragged.</summary>
    [Category("Action")]
    public event EventHandler<SplitterMovingEventArgs>? SplitterMoving;

    /// <summary>Raised after a splitter drag completes.</summary>
    [Category("Action")]
    public event EventHandler<SplitterMovedEventArgs>? SplitterMoved;

    /// <summary>Raised when a panel is collapsed.</summary>
    [Category("Action")]
    public event EventHandler<PanelCollapsedEventArgs>? PanelCollapsed;

    /// <summary>Raised when a panel is restored.</summary>
    [Category("Action")]
    public event EventHandler<PanelRestoredEventArgs>? PanelRestored;

    public void AddPanel() => InsertPanel(_panels.Count, new MultiSplitPanel());

    public void InsertPanel(int index) => InsertPanel(index, new MultiSplitPanel());

    public void RemovePanelAt(int index)
    {
        if (_panels.Count <= 1 || index < 0 || index >= _panels.Count)
        {
            return;
        }

        var panel = _panels[index];
        panel.Owner = null;
        panel.SuppressOwnerNotification = true;
        panel.Collapsed = false;
        panel.SuppressOwnerNotification = false;
        _panels.RemoveInternal(panel);
        Controls.Remove(panel);
        panel.Dispose();
        NormalizePanelSizes();
        PerformLayoutAndInvalidate();
    }

    public void CollapsePanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= _panels.Count)
        {
            return;
        }

        CollapsePanelInternal(_panels[panelIndex], panelIndex);
    }

    public void RestorePanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= _panels.Count)
        {
            return;
        }

        RestorePanelInternal(_panels[panelIndex], panelIndex, fireEvent: true);
    }

    public void RestoreAll()
    {
        var restored = false;
        for (var index = 0; index < _panels.Count; index++)
        {
            var panel = _panels[index];
            if (!panel.Collapsed)
            {
                continue;
            }

            RestorePanelInternal(panel, index, fireEvent: true);
            restored = true;
        }

        if (restored)
        {
            PerformLayoutAndInvalidate();
        }
    }

    internal void InsertPanel(int index, MultiSplitPanel panel)
    {
        index = Math.Max(0, Math.Min(index, _panels.Count));
        panel.Owner = this;
        panel.Dock = DockStyle.None;
        panel.Anchor = AnchorStyles.None;
        panel.TabStop = false;
        panel.ApplyMinimumSize();

        if (panel.SplitSize <= 0)
        {
            panel.SplitSize = 100;
        }

        panel.RememberSplitSize();
        _panels.InsertInternal(index, panel);
        Controls.Add(panel);
        Controls.SetChildIndex(panel, index);
        NormalizePanelSizes();
        PerformLayoutAndInvalidate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (_panels.Count == 0 && !DesignMode)
        {
            EnsureMinimumPanels();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (!_layoutSuspended)
        {
            ApplyLayout(updateSplitSizes: true);
        }
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (!_layoutSuspended && IsHandleCreated)
        {
            ApplyLayout(updateSplitSizes: true);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_splitterRects.Count == 0)
        {
            ApplyLayout(updateSplitSizes: false);
        }

        for (var index = 0; index < _splitterRects.Count; index++)
        {
            DrawSplitter(e.Graphics, index);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_dragSplitterIndex >= 0)
        {
            var delta = GetPrimaryCoordinate(e.Location) - _dragStartCoordinate;
            ApplySplitterDrag(_dragSplitterIndex, delta, finalize: false);
            return;
        }

        UpdateHoverState(e.Location);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var hit = HitTest(e.Location);
        if (!hit.IsSplitter)
        {
            return;
        }

        if (hit.ButtonKind != SplitterButtonKind.None)
        {
            _pressedSplitterIndex = hit.SplitterIndex;
            _pressedButton = (int)hit.ButtonKind;
            InvalidateSplitter(hit.SplitterIndex);
            return;
        }

        if (hit.IsDragRegion)
        {
            _dragSplitterIndex = hit.SplitterIndex;
            _dragStartCoordinate = GetPrimaryCoordinate(e.Location);
            _dragStartBeforeSize = GetPanelPrimarySize(_panels[hit.SplitterIndex]);
            _dragStartAfterSize = GetPanelPrimarySize(_panels[hit.SplitterIndex + 1]);
            _layoutSuspended = true;
            Capture = true;
            Cursor = _orientation == Orientation.Vertical ? Cursors.VSplit : Cursors.HSplit;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (_dragSplitterIndex >= 0)
        {
            var delta = GetPrimaryCoordinate(e.Location) - _dragStartCoordinate;
            ApplySplitterDrag(_dragSplitterIndex, delta, finalize: true);
            _dragSplitterIndex = -1;
            _layoutSuspended = false;
            Capture = false;
            UpdateHoverState(e.Location);
            return;
        }

        if (_pressedSplitterIndex >= 0 && _pressedButton >= 0)
        {
            var hit = HitTest(e.Location);
            if (hit.SplitterIndex == _pressedSplitterIndex && (int)hit.ButtonKind == _pressedButton)
            {
                HandleButtonClick(_pressedSplitterIndex, (SplitterButtonKind)_pressedButton);
            }

            _pressedSplitterIndex = -1;
            _pressedButton = -1;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_dragSplitterIndex >= 0)
        {
            return;
        }

        _hoveredSplitterIndex = -1;
        _hoveredButton = -1;
        Cursor = Cursors.Default;
        Invalidate();
    }

    private void EnsureMinimumPanels()
    {
        if (_panels.Count > 0)
        {
            return;
        }

        InsertPanel(0, new MultiSplitPanel { SplitSize = 150 });
        InsertPanel(1, new MultiSplitPanel { SplitSize = 150 });
    }

    private void PerformLayoutAndInvalidate()
    {
        ApplyLayout(updateSplitSizes: true);
        Invalidate();
    }

    private void ApplyLayout(bool updateSplitSizes)
    {
        _splitterRects.Clear();
        _buttonRects.Clear();

        if (_panels.Count == 0)
        {
            return;
        }

        var available = GetPrimaryClientSize() - Math.Max(0, _panels.Count - 1) * _splitterWidth;
        available = Math.Max(0, available);
        var sizes = CalculatePanelSizes(available);
        var primary = 0;
        var secondary = GetSecondaryClientSize();

        for (var index = 0; index < _panels.Count; index++)
        {
            var panel = _panels[index];
            var size = sizes[index];

            if (_orientation == Orientation.Vertical)
            {
                panel.SetBounds(primary, 0, size, secondary);
            }
            else
            {
                panel.SetBounds(0, primary, secondary, size);
            }

            if (updateSplitSizes && !panel.Collapsed)
            {
                panel.SplitSize = size;
                panel.RememberSplitSize();
            }

            primary += size;

            if (index < _panels.Count - 1)
            {
                var splitterRect = _orientation == Orientation.Vertical
                    ? new Rectangle(primary, 0, _splitterWidth, secondary)
                    : new Rectangle(0, primary, secondary, _splitterWidth);

                _splitterRects.Add(splitterRect);
                _buttonRects.Add(CreateButtonRects(splitterRect));
                primary += _splitterWidth;
            }
        }
    }

    private int[] CalculatePanelSizes(int available)
    {
        var count = _panels.Count;
        var sizes = new int[count];
        var collapsedTotal = 0;
        var flexibleTotal = 0d;

        for (var index = 0; index < count; index++)
        {
            var panel = _panels[index];
            if (panel.Collapsed)
            {
                sizes[index] = panel.CollapsedSize;
                collapsedTotal += panel.CollapsedSize;
            }
            else
            {
                flexibleTotal += Math.Max(1, panel.SplitSize);
            }
        }

        var flexibleAvailable = Math.Max(0, available - collapsedTotal);
        var assigned = 0;

        var flexibleCount = CountFlexiblePanels();
        for (var index = 0; index < count; index++)
        {
            var panel = _panels[index];
            if (panel.Collapsed)
            {
                continue;
            }

            var minimum = panel.GetMinimumSplitSize(_orientation);
            var proportional = flexibleTotal <= 0
                ? (double)flexibleAvailable / flexibleCount
                : panel.SplitSize * flexibleAvailable / flexibleTotal;

            sizes[index] = Math.Max(minimum, (int)Math.Round(proportional));
            assigned += sizes[index];
        }

        var remainder = flexibleAvailable - assigned;
        var lastFlexible = FindLastFlexibleIndex();
        if (lastFlexible >= 0 && remainder != 0)
        {
            sizes[lastFlexible] = Math.Max(_panels[lastFlexible].GetMinimumSplitSize(_orientation), sizes[lastFlexible] + remainder);
        }

        EnforceMinimumSizes(sizes, flexibleAvailable);
        return sizes;
    }

    private int CountFlexiblePanels()
    {
        var count = 0;
        foreach (MultiSplitPanel panel in _panels)
        {
            if (!panel.Collapsed)
            {
                count++;
            }
        }

        return Math.Max(1, count);
    }

    private int FindLastFlexibleIndex()
    {
        for (var index = _panels.Count - 1; index >= 0; index--)
        {
            if (!_panels[index].Collapsed)
            {
                return index;
            }
        }

        return -1;
    }

    private void EnforceMinimumSizes(int[] sizes, int flexibleAvailable)
    {
        for (var pass = 0; pass < _panels.Count; pass++)
        {
            var changed = false;
            for (var index = 0; index < _panels.Count; index++)
            {
                if (_panels[index].Collapsed)
                {
                    continue;
                }

                var minimum = _panels[index].GetMinimumSplitSize(_orientation);
                if (sizes[index] >= minimum)
                {
                    continue;
                }

                var deficit = minimum - sizes[index];
                if (!TryBorrowSpace(sizes, index, deficit))
                {
                    sizes[index] = Math.Min(minimum, flexibleAvailable);
                }
                else
                {
                    sizes[index] = minimum;
                }

                changed = true;
            }

            if (!changed)
            {
                break;
            }
        }
    }

    private bool TryBorrowSpace(int[] sizes, int needyIndex, int deficit)
    {
        for (var index = _panels.Count - 1; index >= 0 && deficit > 0; index--)
        {
            if (index == needyIndex || _panels[index].Collapsed)
            {
                continue;
            }

            var minimum = _panels[index].GetMinimumSplitSize(_orientation);
            var available = sizes[index] - minimum;
            if (available <= 0)
            {
                continue;
            }

            var take = Math.Min(available, deficit);
            sizes[index] -= take;
            deficit -= take;
        }

        return deficit == 0;
    }

    private void ApplySplitterDrag(int splitterIndex, int delta, bool finalize)
    {
        if (splitterIndex < 0 || splitterIndex >= _panels.Count - 1)
        {
            return;
        }

        var beforePanel = _panels[splitterIndex];
        var afterPanel = _panels[splitterIndex + 1];
        var beforeSize = _dragStartBeforeSize + delta;
        var afterSize = _dragStartAfterSize - delta;

        var beforeMin = beforePanel.Collapsed ? beforePanel.CollapsedSize : beforePanel.GetMinimumSplitSize(_orientation);
        var afterMin = afterPanel.Collapsed ? afterPanel.CollapsedSize : afterPanel.GetMinimumSplitSize(_orientation);

        beforeSize = Math.Max(beforeMin, beforeSize);
        afterSize = Math.Max(afterMin, afterSize);

        var total = _dragStartBeforeSize + _dragStartAfterSize;
        if (beforeSize + afterSize != total)
        {
            if (beforeSize + afterSize > total)
            {
                if (beforeSize > beforeMin)
                {
                    beforeSize = total - afterSize;
                }
                else
                {
                    afterSize = total - beforeSize;
                }
            }
            else
            {
                var extra = total - beforeSize - afterSize;
                if (afterPanel.Collapsed || afterSize > afterMin)
                {
                    afterSize += extra;
                }
                else
                {
                    beforeSize += extra;
                }
            }
        }

        beforeSize = Math.Max(beforeMin, Math.Min(beforeSize, total - afterMin));
        afterSize = total - beforeSize;

        var sizes = new int[_panels.Count];
        for (var index = 0; index < _panels.Count; index++)
        {
            sizes[index] = GetPanelPrimarySize(_panels[index]);
        }

        sizes[splitterIndex] = beforeSize;
        sizes[splitterIndex + 1] = afterSize;
        ApplyExplicitSizes(sizes, updateSplitSizes: finalize);

        SplitterMoving?.Invoke(this, new SplitterMovingEventArgs(splitterIndex, delta));

        if (finalize)
        {
            if (!beforePanel.Collapsed)
            {
                beforePanel.SplitSize = beforeSize;
                beforePanel.RememberSplitSize();
            }

            if (!afterPanel.Collapsed)
            {
                afterPanel.SplitSize = afterSize;
                afterPanel.RememberSplitSize();
            }

            _layoutSuspended = false;
            SplitterMoved?.Invoke(this, new SplitterMovedEventArgs(splitterIndex, beforePanel, afterPanel));
        }
    }

    private void ApplyExplicitSizes(int[] sizes, bool updateSplitSizes)
    {
        _splitterRects.Clear();
        _buttonRects.Clear();

        var primary = 0;
        var secondary = GetSecondaryClientSize();

        for (var index = 0; index < _panels.Count; index++)
        {
            var panel = _panels[index];
            var size = sizes[index];

            if (_orientation == Orientation.Vertical)
            {
                panel.SetBounds(primary, 0, size, secondary);
            }
            else
            {
                panel.SetBounds(0, primary, secondary, size);
            }

            if (updateSplitSizes && !panel.Collapsed)
            {
                panel.SplitSize = size;
            }

            primary += size;

            if (index < _panels.Count - 1)
            {
                var splitterRect = _orientation == Orientation.Vertical
                    ? new Rectangle(primary, 0, _splitterWidth, secondary)
                    : new Rectangle(0, primary, secondary, _splitterWidth);

                _splitterRects.Add(splitterRect);
                _buttonRects.Add(CreateButtonRects(splitterRect));
                primary += _splitterWidth;
            }
        }

        Invalidate();
    }

    private void CollapsePanelInternal(MultiSplitPanel panel, int index)
    {
        if (panel.Collapsed)
        {
            return;
        }

        panel.RememberSplitSize();
        panel.SuppressOwnerNotification = true;
        panel.Collapsed = true;
        panel.SuppressOwnerNotification = false;
        panel.SplitSize = panel.CollapsedSize;
        PerformLayoutAndInvalidate();
        PanelCollapsed?.Invoke(this, new PanelCollapsedEventArgs(index, panel));
    }

    private void RestorePanelInternal(MultiSplitPanel panel, int index, bool fireEvent)
    {
        if (!panel.Collapsed)
        {
            return;
        }

        panel.SuppressOwnerNotification = true;
        panel.Collapsed = false;
        panel.SuppressOwnerNotification = false;
        panel.SplitSize = panel.LastNonCollapsedSplitSize > 0 ? panel.LastNonCollapsedSplitSize : 100;
        panel.RememberSplitSize();
        PerformLayoutAndInvalidate();

        if (fireEvent)
        {
            PanelRestored?.Invoke(this, new PanelRestoredEventArgs(index, panel));
        }
    }

    private void HandleButtonClick(int splitterIndex, SplitterButtonKind buttonKind)
    {
        switch (buttonKind)
        {
            case SplitterButtonKind.CollapseBefore:
                CollapsePanelInternal(_panels[splitterIndex], splitterIndex);
                break;
            case SplitterButtonKind.CollapseAfter:
                CollapsePanelInternal(_panels[splitterIndex + 1], splitterIndex + 1);
                break;
            case SplitterButtonKind.Restore:
                RestoreAdjacentPanels(splitterIndex);
                break;
        }
    }

    private void RestoreAdjacentPanels(int splitterIndex)
    {
        var before = _panels[splitterIndex];
        var after = _panels[splitterIndex + 1];

        if (!before.Collapsed && !after.Collapsed)
        {
            return;
        }

        var available = before.Collapsed && after.Collapsed
            ? before.CollapsedSize + after.CollapsedSize
            : GetPrimaryClientSize();

        if (before.Collapsed && after.Collapsed)
        {
            var beforeSize = before.LastNonCollapsedSplitSize > 0 ? before.LastNonCollapsedSplitSize : available / 2;
            var afterSize = after.LastNonCollapsedSplitSize > 0 ? after.LastNonCollapsedSplitSize : available - beforeSize;
            RestorePanelInternal(before, splitterIndex, fireEvent: true);
            RestorePanelInternal(after, splitterIndex + 1, fireEvent: true);
            before.SplitSize = beforeSize;
            after.SplitSize = afterSize;
            before.RememberSplitSize();
            after.RememberSplitSize();
            PerformLayoutAndInvalidate();
            return;
        }

        if (before.Collapsed)
        {
            RestorePanelInternal(before, splitterIndex, fireEvent: true);
            before.SplitSize = before.LastNonCollapsedSplitSize > 0 ? before.LastNonCollapsedSplitSize : available / 2;
            before.RememberSplitSize();
            PerformLayoutAndInvalidate();
            return;
        }

        RestorePanelInternal(after, splitterIndex + 1, fireEvent: true);
        after.SplitSize = after.LastNonCollapsedSplitSize > 0 ? after.LastNonCollapsedSplitSize : available / 2;
        after.RememberSplitSize();
        PerformLayoutAndInvalidate();
    }

    private void NormalizePanelSizes()
    {
        foreach (MultiSplitPanel panel in _panels)
        {
            if (panel.SplitSize <= 0)
            {
                panel.SplitSize = 100;
            }
        }
    }

    private SplitterHitTestResult HitTest(Point location)
    {
        for (var index = 0; index < _buttonRects.Count; index++)
        {
            var buttons = _buttonRects[index];
            if (buttons.Length > 0 && buttons[0].Contains(location))
            {
                return new SplitterHitTestResult(index, SplitterButtonKind.CollapseBefore, false);
            }

            if (buttons.Length > 1 && buttons[1].Contains(location))
            {
                return new SplitterHitTestResult(index, SplitterButtonKind.Restore, false);
            }

            if (buttons.Length > 2 && buttons[2].Contains(location))
            {
                return new SplitterHitTestResult(index, SplitterButtonKind.CollapseAfter, false);
            }
        }

        for (var index = 0; index < _splitterRects.Count; index++)
        {
            if (_splitterRects[index].Contains(location))
            {
                return new SplitterHitTestResult(index, SplitterButtonKind.None, true);
            }
        }

        return SplitterHitTestResult.None;
    }

    private void UpdateHoverState(Point location)
    {
        var hit = HitTest(location);
        var oldSplitter = _hoveredSplitterIndex;
        var oldButton = _hoveredButton;

        if (hit.ButtonKind != SplitterButtonKind.None)
        {
            _hoveredSplitterIndex = hit.SplitterIndex;
            _hoveredButton = (int)hit.ButtonKind;
            Cursor = Cursors.Default;
        }
        else if (hit.IsDragRegion)
        {
            _hoveredSplitterIndex = hit.SplitterIndex;
            _hoveredButton = -1;
            Cursor = _orientation == Orientation.Vertical ? Cursors.VSplit : Cursors.HSplit;
        }
        else
        {
            _hoveredSplitterIndex = -1;
            _hoveredButton = -1;
            Cursor = Cursors.Default;
        }

        if (oldSplitter != _hoveredSplitterIndex || oldButton != _hoveredButton)
        {
            Invalidate();
        }
    }

    private void InvalidateSplitter(int splitterIndex)
    {
        if (splitterIndex >= 0 && splitterIndex < _splitterRects.Count)
        {
            Invalidate(_splitterRects[splitterIndex]);
        }
        else
        {
            Invalidate();
        }
    }

    private void DrawSplitter(Graphics graphics, int splitterIndex)
    {
        var splitterRect = _splitterRects[splitterIndex];
        var isHovered = _hoveredSplitterIndex == splitterIndex && _hoveredButton < 0;
        var backColor = isHovered ? _splitterHoverBackColor : _splitterBackColor;

        using var brush = new SolidBrush(backColor);
        graphics.FillRectangle(brush, splitterRect);
        DrawGripDots(graphics, splitterRect);

        var buttons = _buttonRects[splitterIndex];
        DrawButton(graphics, buttons[0], splitterIndex, SplitterButtonKind.CollapseBefore);
        DrawButton(graphics, buttons[1], splitterIndex, SplitterButtonKind.Restore);
        DrawButton(graphics, buttons[2], splitterIndex, SplitterButtonKind.CollapseAfter);
    }

    private void DrawButton(Graphics graphics, Rectangle bounds, int splitterIndex, SplitterButtonKind kind)
    {
        var isHovered = _hoveredSplitterIndex == splitterIndex && _hoveredButton == (int)kind;
        var isPressed = _pressedSplitterIndex == splitterIndex && _pressedButton == (int)kind;

        Color backColor;
        Color glyphColor;
        if (isPressed)
        {
            backColor = _splitterButtonPressedBackColor;
            glyphColor = Color.White;
        }
        else if (isHovered)
        {
            backColor = _splitterButtonHoverBackColor;
            glyphColor = Color.Black;
        }
        else
        {
            backColor = _splitterButtonBackColor;
            glyphColor = Color.Black;
        }

        using var brush = new SolidBrush(backColor);
        graphics.FillRectangle(brush, bounds);
        using var borderPen = new Pen(_splitterButtonBorderColor);
        graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        DrawButtonGlyph(graphics, bounds, kind, glyphColor);
    }

    private void DrawButtonGlyph(Graphics graphics, Rectangle bounds, SplitterButtonKind kind, Color color)
    {
        var extent = Math.Max(2, Math.Min(bounds.Width, bounds.Height) / 3);
        var penWidth = Math.Max(1f, extent / 4f);
        using var pen = new Pen(color, penWidth)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        var centerX = bounds.Left + bounds.Width / 2f;
        var centerY = bounds.Top + bounds.Height / 2f;

        switch (kind)
        {
            case SplitterButtonKind.CollapseBefore:
                if (_orientation == Orientation.Vertical)
                {
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX - extent / 2f, centerY);
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX - extent / 4f, centerY - extent / 4f);
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX - extent / 4f, centerY + extent / 4f);
                }
                else
                {
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX, centerY - extent / 2f);
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX - extent / 4f, centerY - extent / 4f);
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX + extent / 4f, centerY - extent / 4f);
                }

                break;

            case SplitterButtonKind.CollapseAfter:
                if (_orientation == Orientation.Vertical)
                {
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX + extent / 2f, centerY);
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX + extent / 4f, centerY - extent / 4f);
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX + extent / 4f, centerY + extent / 4f);
                }
                else
                {
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX, centerY + extent / 2f);
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX - extent / 4f, centerY + extent / 4f);
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX + extent / 4f, centerY + extent / 4f);
                }

                break;

            case SplitterButtonKind.Restore:
                if (_orientation == Orientation.Vertical)
                {
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX - extent / 6f, centerY);
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX - extent / 3f, centerY - extent / 5f);
                    graphics.DrawLine(pen, centerX - extent / 2f, centerY, centerX - extent / 3f, centerY + extent / 5f);
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX + extent / 6f, centerY);
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX + extent / 3f, centerY - extent / 5f);
                    graphics.DrawLine(pen, centerX + extent / 2f, centerY, centerX + extent / 3f, centerY + extent / 5f);
                }
                else
                {
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX, centerY - extent / 6f);
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX - extent / 5f, centerY - extent / 3f);
                    graphics.DrawLine(pen, centerX, centerY - extent / 2f, centerX + extent / 5f, centerY - extent / 3f);
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX, centerY + extent / 6f);
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX - extent / 5f, centerY + extent / 3f);
                    graphics.DrawLine(pen, centerX, centerY + extent / 2f, centerX + extent / 5f, centerY + extent / 3f);
                }

                break;
        }
    }

    private void DrawGripDots(Graphics graphics, Rectangle splitterRect)
    {
        var dotSize = Math.Max(1, _splitterWidth / 5);
        var spacing = dotSize + 1;
        using var brush = new SolidBrush(Color.FromArgb(140, _splitterButtonBorderColor));

        if (_orientation == Orientation.Vertical)
        {
            var centerX = splitterRect.Left + splitterRect.Width / 2;
            var startY = splitterRect.Top + splitterRect.Height / 2 - spacing;
            for (var row = 0; row < 3; row++)
            {
                graphics.FillEllipse(brush, centerX - dotSize / 2, startY + row * spacing - dotSize / 2, dotSize, dotSize);
            }
        }
        else
        {
            var centerY = splitterRect.Top + splitterRect.Height / 2;
            var startX = splitterRect.Left + splitterRect.Width / 2 - spacing;
            for (var column = 0; column < 3; column++)
            {
                graphics.FillEllipse(brush, startX + column * spacing - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
            }
        }
    }

    private Rectangle[] CreateButtonRects(Rectangle splitterRect)
    {
        var buttonSize = GetButtonSize();
        var spacing = GetButtonSpacing();
        var totalPrimary = buttonSize * 3 + spacing * 2;
        var rects = new Rectangle[3];

        if (_orientation == Orientation.Vertical)
        {
            var startY = splitterRect.Top + (splitterRect.Height - totalPrimary) / 2;
            var x = splitterRect.Left + (splitterRect.Width - buttonSize) / 2;
            for (var index = 0; index < 3; index++)
            {
                var y = startY + index * (buttonSize + spacing);
                rects[index] = new Rectangle(x, y, buttonSize, buttonSize);
            }
        }
        else
        {
            var startX = splitterRect.Left + (splitterRect.Width - totalPrimary) / 2;
            var y = splitterRect.Top + (splitterRect.Height - buttonSize) / 2;
            for (var index = 0; index < 3; index++)
            {
                var x = startX + index * (buttonSize + spacing);
                rects[index] = new Rectangle(x, y, buttonSize, buttonSize);
            }
        }

        return rects;
    }

    private int GetButtonSize() => Math.Max(4, _splitterWidth - Math.Max(1, _splitterWidth / 8) * 2);

    private int GetButtonSpacing() => Math.Max(1, _splitterWidth / 6);

    private int GetPrimaryClientSize() =>
        _orientation == Orientation.Vertical ? ClientSize.Width : ClientSize.Height;

    private int GetSecondaryClientSize() =>
        _orientation == Orientation.Vertical ? ClientSize.Height : ClientSize.Width;

    private int GetPrimaryCoordinate(Point point) =>
        _orientation == Orientation.Vertical ? point.X : point.Y;

    private int GetPanelPrimarySize(MultiSplitPanel panel) =>
        _orientation == Orientation.Vertical ? panel.Width : panel.Height;
}