using LucideSharp.WinForms;

namespace LucideSharp.MultiSplitDemo;

public partial class MainForm : Form
{
    private static readonly Color[] PanelPalette =
    [
        Color.FromArgb(214, 234, 248),
        Color.FromArgb(225, 245, 225),
        Color.FromArgb(255, 244, 214),
        Color.FromArgb(245, 225, 245),
        Color.FromArgb(230, 230, 250),
        Color.FromArgb(255, 228, 225)
    ];

    private int _collapseCycleIndex;

    public MainForm()
    {
        InitializeComponent();
        WireEvents();
        UpdateStatus("Ready.");
    }

    private void WireEvents()
    {
        multiSplitContainer1.SplitterMoved += (_, e) =>
            UpdateStatus($"Splitter {e.SplitterIndex} moved between panels.");

        multiSplitContainer1.PanelCollapsed += (_, e) =>
            UpdateStatus($"Panel {e.PanelIndex} collapsed.");

        multiSplitContainer1.PanelRestored += (_, e) =>
            UpdateStatus($"Panel {e.PanelIndex} restored.");

        menuToggleOrientation.Click += (_, _) =>
        {
            multiSplitContainer1.Orientation = multiSplitContainer1.Orientation == Orientation.Vertical
                ? Orientation.Horizontal
                : Orientation.Vertical;
            UpdateStatus($"Orientation set to {multiSplitContainer1.Orientation}.");
        };

        menuAddPanel.Click += (_, _) =>
        {
            multiSplitContainer1.AddPanel();
            DecoratePanels();
            UpdateStatus($"Added panel. Total: {multiSplitContainer1.Panels.Count}.");
        };

        menuRemovePanel.Click += (_, _) =>
        {
            if (multiSplitContainer1.Panels.Count <= 1)
            {
                UpdateStatus("At least one panel must remain.");
                return;
            }

            multiSplitContainer1.RemovePanelAt(multiSplitContainer1.Panels.Count - 1);
            UpdateStatus($"Removed last panel. Total: {multiSplitContainer1.Panels.Count}.");
        };

        menuCollapseNext.Click += (_, _) =>
        {
            if (multiSplitContainer1.Panels.Count == 0)
            {
                return;
            }

            var index = _collapseCycleIndex % multiSplitContainer1.Panels.Count;
            multiSplitContainer1.CollapsePanel(index);
            _collapseCycleIndex++;
            UpdateStatus($"Collapsed panel {index}.");
        };

        menuRestoreAll.Click += (_, _) =>
        {
            multiSplitContainer1.RestoreAll();
            UpdateStatus("Restored all panels.");
        };
    }

    private void DecoratePanels()
    {
        for (var index = 0; index < multiSplitContainer1.Panels.Count; index++)
        {
            multiSplitContainer1.Panels[index].BackColor = PanelPalette[index % PanelPalette.Length];
        }
    }

    private void UpdateStatus(string message) => statusLabel.Text = message;
}