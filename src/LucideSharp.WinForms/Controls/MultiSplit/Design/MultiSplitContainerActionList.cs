using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace LucideSharp.WinForms.Design;

internal sealed class MultiSplitContainerActionList : DesignerActionList
{
    private readonly MultiSplitContainerDesigner _designer;

    public MultiSplitContainerActionList(MultiSplitContainerDesigner designer)
        : base(designer.Component)
    {
        _designer = designer;
    }

    public Orientation Orientation
    {
        get => Container.Orientation;
        set => SetPropertyValue(nameof(MultiSplitContainer.Orientation), value);
    }

    private MultiSplitContainer Container => (MultiSplitContainer)Component!;

    public override DesignerActionItemCollection GetSortedActionItems()
    {
        return new DesignerActionItemCollection
        {
            new DesignerActionPropertyItem(nameof(Orientation), "Orientation", "Layout", "Split direction."),
            new DesignerActionMethodItem(this, nameof(AddPanel), "Add Panel", "Panels", true),
            new DesignerActionMethodItem(this, nameof(RemoveLastPanel), "Remove Last Panel", "Panels", Container.Panels.Count > 1),
            new DesignerActionMethodItem(this, nameof(ToggleOrientation), "Toggle Orientation", "Layout", true)
        };
    }

    public void AddPanel()
    {
        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
        var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        var panelsMember = TypeDescriptor.GetProperties(Container)[nameof(MultiSplitContainer.Panels)];
        componentChangeService?.OnComponentChanging(Container, panelsMember);

        if (host is not null)
        {
            var panel = (MultiSplitPanel)host.CreateComponent(typeof(MultiSplitPanel));
            Container.InsertPanel(Container.Panels.Count, panel);
            _designer.RegisterPanel(panel);
        }
        else
        {
            Container.AddPanel();
        }

        componentChangeService?.OnComponentChanged(Container, panelsMember, null, null);
    }

    public void RemoveLastPanel()
    {
        if (Container.Panels.Count <= 1)
        {
            return;
        }

        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
        var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        var panelsMember = TypeDescriptor.GetProperties(Container)[nameof(MultiSplitContainer.Panels)];
        var panel = Container.Panels[Container.Panels.Count - 1];
        componentChangeService?.OnComponentChanging(Container, panelsMember);

        if (host is not null)
        {
            Container.DetachPanel(panel);
            host.DestroyComponent(panel);
        }
        else
        {
            Container.RemovePanelAt(Container.Panels.Count - 1);
        }

        componentChangeService?.OnComponentChanged(Container, panelsMember, null, null);
    }

    public void ToggleOrientation()
    {
        Orientation = Orientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;
    }

    private void SetPropertyValue(string propertyName, object value)
    {
        var property = TypeDescriptor.GetProperties(Container)[propertyName];
        if (property is null)
        {
            return;
        }

        var componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        componentChangeService?.OnComponentChanging(Container, property);
        property.SetValue(Container, value);
        componentChangeService?.OnComponentChanged(Container, property, null, null);
    }
}