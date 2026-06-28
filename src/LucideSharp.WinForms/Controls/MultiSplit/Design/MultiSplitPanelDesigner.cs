using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace LucideSharp.WinForms.Design;

internal sealed class MultiSplitPanelDesigner : ParentControlDesigner
{
    public override SelectionRules SelectionRules
    {
        get
        {
            var rules = base.SelectionRules;
            rules &= ~(SelectionRules.AllSizeable | SelectionRules.Moveable);
            return rules;
        }
    }

    public override void Initialize(IComponent component)
    {
        base.Initialize(component);
        EnableDragDrop(true);

        if (component is MultiSplitPanel panel)
        {
            panel.BackColor = panel.BackColor == Color.Transparent
                ? SystemColors.Control
                : panel.BackColor;
        }
    }

    public override bool CanBeParentedTo(IDesigner? parentDesigner) =>
        parentDesigner?.Component is MultiSplitContainer;

    public override bool CanParent(Control control) =>
        control is not MultiSplitPanel && control is not MultiSplitContainer;

    public override bool CanParent(ControlDesigner controlDesigner) =>
        controlDesigner.Control is not MultiSplitPanel && controlDesigner.Control is not MultiSplitContainer;

    protected override void OnDragEnter(DragEventArgs de)
    {
        if (de.Data?.GetDataPresent(typeof(ToolboxItem)) == true)
        {
            de.Effect = DragDropEffects.Copy;
            return;
        }

        base.OnDragEnter(de);
    }

    protected override void PostFilterAttributes(IDictionary attributes)
    {
        base.PostFilterAttributes(attributes);
        attributes[typeof(DockingAttribute)] = new DockingAttribute(DockingBehavior.Never);
    }

    protected override void PostFilterProperties(IDictionary properties)
    {
        base.PostFilterProperties(properties);

        string[] hidden =
        [
            nameof(Control.Dock),
            nameof(Control.Anchor),
            nameof(Control.Location),
            nameof(Control.Size),
            nameof(Control.Width),
            nameof(Control.Height),
            nameof(Control.MinimumSize),
            nameof(Control.MaximumSize),
        ];

        foreach (var name in hidden)
        {
            if (properties[name] is PropertyDescriptor property)
            {
                properties[name] = TypeDescriptor.CreateProperty(
                    Component.GetType(),
                    property,
                    new BrowsableAttribute(false));
            }
        }
    }
}