using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace LucideSharp.WinForms.Design;

internal sealed class MultiSplitContainerDesigner : ParentControlDesigner
{
    private DesignerActionListCollection? _actionLists;

    public MultiSplitContainerDesigner()
    {
        AutoResizeHandles = true;
    }

    public override void Initialize(IComponent component)
    {
        base.Initialize(component);
        EnableDragDrop(true);
    }

    public override bool CanParent(Control control) => control is MultiSplitPanel;

    public override bool CanParent(ControlDesigner controlDesigner) =>
        controlDesigner.Control is MultiSplitPanel;

    protected override void OnDragEnter(DragEventArgs de)
    {
        if (de.Data is not null && de.Data.GetDataPresent(typeof(MultiSplitPanel)))
        {
            de.Effect = DragDropEffects.Copy;
            return;
        }

        base.OnDragEnter(de);
    }

    protected override bool GetHitTest(Point point)
    {
        if (Control is not MultiSplitContainer container)
        {
            return false;
        }

        foreach (MultiSplitPanel panel in container.Panels)
        {
            if (panel.Bounds.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    public override void InitializeNewComponent(IDictionary defaultValues)
    {
        base.InitializeNewComponent(defaultValues);

        if (Control is not MultiSplitContainer container || container.Panels.Count > 0)
        {
            return;
        }

        var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        var panelsMember = TypeDescriptor.GetProperties(container)[nameof(MultiSplitContainer.Panels)];

        componentChangeService?.OnComponentChanging(container, panelsMember);
        container.AddPanel();
        container.AddPanel();
        componentChangeService?.OnComponentChanged(container, panelsMember, null, null);
    }

    public override DesignerActionListCollection ActionLists =>
        _actionLists ??= new DesignerActionListCollection
        {
            new MultiSplitContainerActionList(this)
        };
}