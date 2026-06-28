using SplitContainerControl = LucideSharp.WinForms.MultiSplitContainer;
using SplitPanelControl = LucideSharp.WinForms.MultiSplitPanel;

namespace LucideSharp.MultiSplitDemo;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem menuLayout;
    private ToolStripMenuItem menuToggleOrientation;
    private ToolStripMenuItem menuPanels;
    private ToolStripMenuItem menuAddPanel;
    private ToolStripMenuItem menuRemovePanel;
    private ToolStripMenuItem menuCollapseNext;
    private ToolStripMenuItem menuRestoreAll;
    private StatusStrip statusStrip1;
    private ToolStripStatusLabel statusLabel;
    private SplitContainerControl multiSplitContainer1;
    private SplitPanelControl panel1;
    private SplitPanelControl panel2;
    private SplitPanelControl panel3;
    private SplitPanelControl panel4;
    private Label label1;
    private TreeView treeView1;
    private Label label2;
    private PropertyGrid propertyGrid1;
    private Label label3;
    private RichTextBox richTextBox1;
    private Label label4;
    private DataGridView dataGridView1;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStrip1 = new MenuStrip();
        menuLayout = new ToolStripMenuItem();
        menuToggleOrientation = new ToolStripMenuItem();
        menuPanels = new ToolStripMenuItem();
        menuAddPanel = new ToolStripMenuItem();
        menuRemovePanel = new ToolStripMenuItem();
        menuCollapseNext = new ToolStripMenuItem();
        menuRestoreAll = new ToolStripMenuItem();
        statusStrip1 = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        multiSplitContainer1 = new SplitContainerControl();
        panel1 = new SplitPanelControl();
        label1 = new Label();
        treeView1 = new TreeView();
        panel2 = new SplitPanelControl();
        label2 = new Label();
        propertyGrid1 = new PropertyGrid();
        panel3 = new SplitPanelControl();
        label3 = new Label();
        richTextBox1 = new RichTextBox();
        panel4 = new SplitPanelControl();
        label4 = new Label();
        dataGridView1 = new DataGridView();
        menuStrip1.SuspendLayout();
        statusStrip1.SuspendLayout();
        multiSplitContainer1.SuspendLayout();
        panel1.SuspendLayout();
        panel2.SuspendLayout();
        panel3.SuspendLayout();
        panel4.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
        SuspendLayout();

        menuStrip1.Items.AddRange([menuLayout, menuPanels]);
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(1100, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";

        menuLayout.DropDownItems.Add(menuToggleOrientation);
        menuLayout.Name = "menuLayout";
        menuLayout.Text = "Layout";
        menuToggleOrientation.Name = "menuToggleOrientation";
        menuToggleOrientation.Text = "Toggle Orientation";

        menuPanels.DropDownItems.AddRange([menuAddPanel, menuRemovePanel, menuCollapseNext, menuRestoreAll]);
        menuPanels.Name = "menuPanels";
        menuPanels.Text = "Panels";
        menuAddPanel.Name = "menuAddPanel";
        menuAddPanel.Text = "Add Panel";
        menuRemovePanel.Name = "menuRemovePanel";
        menuRemovePanel.Text = "Remove Last Panel";
        menuCollapseNext.Name = "menuCollapseNext";
        menuCollapseNext.Text = "Collapse Next Panel";
        menuRestoreAll.Name = "menuRestoreAll";
        menuRestoreAll.Text = "Restore All";

        statusStrip1.Items.Add(statusLabel);
        statusStrip1.Location = new Point(0, 628);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(1100, 22);
        statusStrip1.TabIndex = 2;
        statusStrip1.Text = "statusStrip1";
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
        statusLabel.Text = "Ready";

        multiSplitContainer1.Dock = DockStyle.Fill;
        multiSplitContainer1.Location = new Point(0, 24);
        multiSplitContainer1.Name = "multiSplitContainer1";
        multiSplitContainer1.Orientation = Orientation.Vertical;
        multiSplitContainer1.Panels.Add(panel1);
        multiSplitContainer1.Panels.Add(panel2);
        multiSplitContainer1.Panels.Add(panel3);
        multiSplitContainer1.Panels.Add(panel4);
        multiSplitContainer1.Size = new Size(1100, 604);
        multiSplitContainer1.SplitterWidth = 24;
        multiSplitContainer1.TabIndex = 1;

        panel1.BackColor = Color.FromArgb(214, 234, 248);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(treeView1);
        panel1.Name = "panel1";
        panel1.SplitSize = 220;
        panel1.TabIndex = 0;

        label1.Dock = DockStyle.Top;
        label1.Name = "label1";
        label1.Padding = new Padding(4, 0, 0, 0);
        label1.Size = new Size(220, 24);
        label1.TabIndex = 0;
        label1.Text = "Solution Explorer";
        label1.TextAlign = ContentAlignment.MiddleLeft;

        treeView1.Dock = DockStyle.Fill;
        treeView1.Location = new Point(0, 24);
        treeView1.Name = "treeView1";
        treeView1.Size = new Size(220, 556);
        treeView1.TabIndex = 1;
        treeView1.Nodes.Add("LucideSharp");
        treeView1.Nodes[0].Nodes.Add("src/LucideSharp.WinForms");
        treeView1.Nodes[0].Nodes.Add("samples/MultiSplitContainer.Demo");

        panel2.BackColor = Color.FromArgb(225, 245, 225);
        panel2.Controls.Add(label2);
        panel2.Controls.Add(propertyGrid1);
        panel2.Name = "panel2";
        panel2.SplitSize = 260;
        panel2.TabIndex = 1;

        label2.Dock = DockStyle.Top;
        label2.Name = "label2";
        label2.Padding = new Padding(4, 0, 0, 0);
        label2.Size = new Size(260, 24);
        label2.TabIndex = 0;
        label2.Text = "Properties";
        label2.TextAlign = ContentAlignment.MiddleLeft;

        propertyGrid1.Dock = DockStyle.Fill;
        propertyGrid1.Location = new Point(0, 24);
        propertyGrid1.Name = "propertyGrid1";
        propertyGrid1.SelectedObject = multiSplitContainer1;
        propertyGrid1.Size = new Size(260, 556);
        propertyGrid1.TabIndex = 1;

        panel3.BackColor = Color.FromArgb(255, 244, 214);
        panel3.Controls.Add(label3);
        panel3.Controls.Add(richTextBox1);
        panel3.Name = "panel3";
        panel3.SplitSize = 320;
        panel3.TabIndex = 2;

        label3.Dock = DockStyle.Top;
        label3.Name = "label3";
        label3.Padding = new Padding(4, 0, 0, 0);
        label3.Size = new Size(320, 24);
        label3.TabIndex = 0;
        label3.Text = "Usage";
        label3.TextAlign = ContentAlignment.MiddleLeft;

        richTextBox1.Dock = DockStyle.Fill;
        richTextBox1.Location = new Point(0, 24);
        richTextBox1.Name = "richTextBox1";
        richTextBox1.ReadOnly = true;
        richTextBox1.Size = new Size(320, 556);
        richTextBox1.TabIndex = 1;
        richTextBox1.Text = "Drag splitters to resize adjacent panels.\r\n\r\nUse splitter buttons to collapse/restore panels.\r\n\r\nTry the Layout and Panels menus for orientation and panel management.";

        panel4.BackColor = Color.FromArgb(245, 225, 245);
        panel4.Controls.Add(label4);
        panel4.Controls.Add(dataGridView1);
        panel4.Name = "panel4";
        panel4.SplitSize = 280;
        panel4.TabIndex = 3;

        label4.Dock = DockStyle.Top;
        label4.Name = "label4";
        label4.Padding = new Padding(4, 0, 0, 0);
        label4.Size = new Size(280, 24);
        label4.TabIndex = 0;
        label4.Text = "Panel Data";
        label4.TextAlign = ContentAlignment.MiddleLeft;

        dataGridView1.AllowUserToAddRows = false;
        dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridView1.Dock = DockStyle.Fill;
        dataGridView1.Location = new Point(0, 24);
        dataGridView1.Name = "dataGridView1";
        dataGridView1.ReadOnly = true;
        dataGridView1.Size = new Size(280, 556);
        dataGridView1.TabIndex = 1;
        dataGridView1.Columns.Add("panel", "Panel");
        dataGridView1.Columns.Add("splitSize", "Split Size");
        dataGridView1.Columns.Add("collapsed", "Collapsed");
        dataGridView1.Rows.Add("panel1", "220", "False");
        dataGridView1.Rows.Add("panel2", "260", "False");
        dataGridView1.Rows.Add("panel3", "320", "False");
        dataGridView1.Rows.Add("panel4", "280", "False");

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 650);
        Controls.Add(multiSplitContainer1);
        Controls.Add(statusStrip1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MultiSplitContainer Demo";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        multiSplitContainer1.ResumeLayout(false);
        panel1.ResumeLayout(false);
        panel2.ResumeLayout(false);
        panel3.ResumeLayout(false);
        panel4.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}