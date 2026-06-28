using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace LucideSharp.WinForms.Design;

internal sealed class MultiSplitContainerDesigner : ParentControlDesigner
{
    private DesignerActionListCollection? _actionLists;

    public MultiSplitContainerDesigner()
    {
        AutoResizeHandles = true;
    }

    private MultiSplitContainer Container => (MultiSplitContainer)Control!;

    public override void Initialize(IComponent component)
    {
        base.Initialize(component);
        EnableDragDrop(true);

        Container.ControlAdded += OnContainerControlAdded;
        Container.EnsureLayout();
        EnableDesignModeForAllPanels();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Container.ControlAdded -= OnContainerControlAdded;
        }

        base.Dispose(disposing);
    }

    public override void InitializeNewComponent(IDictionary defaultValues)
    {
        base.InitializeNewComponent(defaultValues);

        if (Container.Panels.Count > 0)
        {
            return;
        }

        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
        var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        var panelsMember = TypeDescriptor.GetProperties(Container)[nameof(MultiSplitContainer.Panels)];

        componentChangeService?.OnComponentChanging(Container, panelsMember);

        if (host is not null)
        {
            var panel1 = (MultiSplitPanel)host.CreateComponent(typeof(MultiSplitPanel));
            var panel2 = (MultiSplitPanel)host.CreateComponent(typeof(MultiSplitPanel));
            Container.InsertPanel(0, panel1);
            Container.InsertPanel(1, panel2);
        }
        else
        {
            Container.AddPanel();
            Container.AddPanel();
        }

        componentChangeService?.OnComponentChanged(Container, panelsMember, null, null);
        Container.EnsureLayout();
        EnableDesignModeForAllPanels();
    }

    public override void InitializeExistingComponent(IDictionary defaultValues)
    {
        base.InitializeExistingComponent(defaultValues);
        Container.EnsureLayout();
        EnableDesignModeForAllPanels();
    }

    public override bool CanParent(Control control) => control is MultiSplitPanel;

    public override bool CanParent(ControlDesigner controlDesigner) =>
        controlDesigner.Control is MultiSplitPanel;

    protected override void OnDragEnter(DragEventArgs de)
    {
        if (de.Data?.GetDataPresent(typeof(MultiSplitPanel)) == true)
        {
            de.Effect = DragDropEffects.Copy;
            return;
        }

        base.OnDragEnter(de);
    }

    protected override void OnDragOver(DragEventArgs de)
    {
        if (de.Data?.GetDataPresent(typeof(ToolboxItem)) == true)
        {
            de.Effect = ResolvePanelAtScreenPoint(new Point(de.X, de.Y)) is not null
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            return;
        }

        base.OnDragOver(de);
    }

    protected override void OnDragDrop(DragEventArgs de)
    {
        if (de.Data?.GetDataPresent(typeof(ToolboxItem)) == true &&
            de.Data.GetData(typeof(ToolboxItem)) is ToolboxItem tool)
        {
            CreateToolOnPanel(tool, new Point(de.X, de.Y));
            return;
        }

        base.OnDragDrop(de);
    }

    protected override IComponent[] CreateToolCore(
        ToolboxItem tool,
        int x,
        int y,
        int width,
        int height,
        bool hasLocation,
        bool hasSize)
    {
        CreateToolOnPanel(tool, new Point(x, y));
        return null!;
    }

    internal void RegisterPanel(MultiSplitPanel panel)
    {
        EnableDesignMode(panel, panel.Name);
        Container.EnsureLayout();
    }

    public override DesignerActionListCollection ActionLists =>
        _actionLists ??= new DesignerActionListCollection
        {
            new MultiSplitContainerActionList(this)
        };

    private void OnContainerControlAdded(object? sender, ControlEventArgs e)
    {
        if (e.Control is MultiSplitPanel panel)
        {
            RegisterPanel(panel);
        }
    }

    private void EnableDesignModeForAllPanels()
    {
        foreach (MultiSplitPanel panel in Container.Panels)
        {
            EnableDesignMode(panel, panel.Name);
        }
    }

    private void CreateToolOnPanel(ToolboxItem tool, Point screenPoint)
    {
        var panel = ResolvePanelAtScreenPoint(screenPoint);
        if (panel is null)
        {
            throw new InvalidOperationException(
                "Cannot add a control because no MultiSplit panel exists at this location. Add a panel first.");
        }

        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
        if (host?.GetDesigner(panel) is ParentControlDesigner panelDesigner)
        {
            ParentControlDesigner.InvokeCreateTool(panelDesigner, tool);
        }
    }

    private MultiSplitPanel? ResolvePanelAtScreenPoint(Point screenPoint)
    {
        Container.EnsureLayout();

        var clientPoint = Control.PointToClient(screenPoint);
        foreach (MultiSplitPanel panel in Container.Panels)
        {
            if (panel.Bounds.Contains(clientPoint))
            {
                return panel;
            }
        }

        return Container.Panels.Count > 0 ? Container.Panels[0] : null;
    }
}