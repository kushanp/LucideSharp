using LucideSharp.WinForms;

namespace WinFormsSample;

public partial class Form1 : Form
{
    private readonly LucideIcon _previewIcon;
    private readonly ComboBox _iconCombo;
    private readonly TrackBar _sizeTrack;
    private readonly TrackBar _strokeTrack;
    private readonly ComboBox _rendererCombo;
    private readonly ComboBox _flipCombo;
    private readonly CheckBox _spinCheck;
    private readonly Button _colorButton;
    private readonly Label _statusLabel;
    private readonly ToolStrip _toolStrip;

    public Form1()
    {
        InitializeComponent();
        Text = "LucideSharp WinForms Sample";
        Size = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(248, 249, 252);
        Font = new Font("Segoe UI", 9F);

        var title = new Label
        {
            Text = "LucideSharp Demo",
            Font = new Font("Segoe UI Semibold", 18F),
            AutoSize = true,
            Location = new Point(24, 20)
        };
        Controls.Add(title);

        _previewIcon = new LucideIcon
        {
            Kind = LucideKind.Heart,
            IconSize = 96,
            ForeColor = Color.FromArgb(220, 38, 38),
            Location = new Point(48, 80),
            Size = new Size(160, 160)
        };
        Controls.Add(_previewIcon);

        var settingsPanel = new Panel
        {
            Location = new Point(260, 72),
            Size = new Size(360, 360),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(settingsPanel);

        _iconCombo = AddLabeledCombo(settingsPanel, "Icon", 16, LucideKind.Heart, LucideKind.Star, LucideKind.Settings, LucideKind.Search, LucideKind.LoaderCircle);
        _iconCombo.SelectedIndexChanged += (_, _) =>
        {
            _previewIcon.Kind = (LucideKind)_iconCombo.SelectedItem!;
            UpdateStatus();
        };

        _sizeTrack = AddLabeledTrackBar(settingsPanel, "Size", 72, 16, 128, 96);
        _sizeTrack.ValueChanged += (_, _) =>
        {
            _previewIcon.IconSize = _sizeTrack.Value;
            UpdateStatus();
        };

        _strokeTrack = AddLabeledTrackBar(settingsPanel, "Stroke Width", 128, 1, 4, 2);
        _strokeTrack.ValueChanged += (_, _) =>
        {
            _previewIcon.StrokeWidth = _strokeTrack.Value;
            UpdateStatus();
        };

        _rendererCombo = AddLabeledCombo(settingsPanel, "Renderer", 184, RenderEngine.SvgSkia, RenderEngine.ClassicSvg);
        _rendererCombo.SelectedIndexChanged += (_, _) =>
        {
            _previewIcon.RenderEngine = (RenderEngine)_rendererCombo.SelectedItem!;
            UpdateStatus();
        };

        _flipCombo = AddLabeledCombo(settingsPanel, "Flip", 240, FlipMode.None, FlipMode.Horizontal, FlipMode.Vertical, FlipMode.Both);
        _flipCombo.SelectedIndexChanged += (_, _) =>
        {
            _previewIcon.Flip = (FlipMode)_flipCombo.SelectedItem!;
            UpdateStatus();
        };

        _spinCheck = new CheckBox
        {
            Text = "Spin",
            Location = new Point(16, 296),
            AutoSize = true
        };
        _spinCheck.CheckedChanged += (_, _) =>
        {
            _previewIcon.Spin = _spinCheck.Checked;
            UpdateStatus();
        };
        settingsPanel.Controls.Add(_spinCheck);

        _colorButton = new Button
        {
            Text = "Pick Color",
            Location = new Point(120, 292),
            Size = new Size(110, 30)
        };
        _colorButton.Click += (_, _) =>
        {
            using var dialog = new ColorDialog { Color = _previewIcon.ForeColor, FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _previewIcon.ForeColor = dialog.Color;
                UpdateStatus();
            }
        };
        settingsPanel.Controls.Add(_colorButton);

        var staticPanel = new Panel
        {
            Location = new Point(640, 72),
            Size = new Size(280, 360),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(staticPanel);

        var staticTitle = new Label
        {
            Text = "Static API",
            Font = new Font("Segoe UI Semibold", 11F),
            Location = new Point(16, 12),
            AutoSize = true
        };
        staticPanel.Controls.Add(staticTitle);

        var staticPicture = new PictureBox
        {
            Location = new Point(16, 48),
            Size = new Size(96, 96),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Image = Lucide.GetImage(LucideKind.Star, 72, Color.Goldenrod)
        };
        staticPanel.Controls.Add(staticPicture);

        var refreshStaticButton = new Button
        {
            Text = "Refresh Static Image",
            Location = new Point(16, 160),
            Size = new Size(240, 32)
        };
        refreshStaticButton.Click += (_, _) =>
        {
            staticPicture.Image?.Dispose();
            staticPicture.Image = Lucide.GetImage(
                (LucideKind)_iconCombo.SelectedItem!,
                _sizeTrack.Value,
                _previewIcon.ForeColor,
                _strokeTrack.Value,
                0f,
                (FlipMode)_flipCombo.SelectedItem!,
                (RenderEngine)_rendererCombo.SelectedItem!);
        };
        staticPanel.Controls.Add(refreshStaticButton);

        var clearCacheButton = new Button
        {
            Text = "Clear Bitmap Cache",
            Location = new Point(16, 204),
            Size = new Size(240, 32)
        };
        clearCacheButton.Click += (_, _) =>
        {
            Lucide.ClearCache();
            UpdateStatus("Bitmap cache cleared.");
        };
        staticPanel.Controls.Add(clearCacheButton);

        _toolStrip = new ToolStrip
        {
            Location = new Point(24, 460),
            Size = new Size(900, 28),
            GripStyle = ToolStripGripStyle.Hidden
        };

        _toolStrip.Items.Add(new ToolStripButton("Home", Lucide.GetImage(LucideKind.Heart, 20, Color.IndianRed), (_, _) => { }) { DisplayStyle = ToolStripItemDisplayStyle.Image });
        _toolStrip.Items.Add(new ToolStripButton("Search", Lucide.GetImage(LucideKind.Search, 20, Color.SteelBlue), (_, _) => { }) { DisplayStyle = ToolStripItemDisplayStyle.Image });
        _toolStrip.Items.Add(new ToolStripButton("Settings", Lucide.GetImage(LucideKind.Settings, 20, Color.DimGray), (_, _) => { }) { DisplayStyle = ToolStripItemDisplayStyle.Image });
        _toolStrip.Items.Add(new ToolStripSeparator());
        _toolStrip.Items.Add(new ToolStripLabel("ToolStrip icons rendered via Lucide.GetImage()"));
        Controls.Add(_toolStrip);

        var buttonRow = new FlowLayoutPanel
        {
            Location = new Point(24, 510),
            Size = new Size(900, 56),
            WrapContents = false
        };

        foreach (var kind in new[] { LucideKind.Heart, LucideKind.Star, LucideKind.Settings, LucideKind.Search })
        {
            var button = new Button
            {
                Text = kind.ToString(),
                Size = new Size(150, 40),
                Image = Lucide.GetImage(kind, 20, Color.White),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 12, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) =>
            {
                _iconCombo.SelectedItem = kind;
                _previewIcon.Kind = kind;
                UpdateStatus();
            };
            buttonRow.Controls.Add(button);
        }

        Controls.Add(buttonRow);

        _statusLabel = new Label
        {
            Location = new Point(24, 580),
            AutoSize = true,
            ForeColor = Color.DimGray
        };
        Controls.Add(_statusLabel);

        UpdateStatus();
    }

    private static ComboBox AddLabeledCombo<T>(Control parent, string label, int top, params T[] items)
    {
        parent.Controls.Add(new Label { Text = label, Location = new Point(16, top), AutoSize = true });
        var combo = new ComboBox
        {
            Location = new Point(120, top - 4),
            Width = 210,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        combo.Items.AddRange(items.Cast<object>().ToArray());
        combo.SelectedIndex = 0;
        parent.Controls.Add(combo);
        return combo;
    }

    private static TrackBar AddLabeledTrackBar(Control parent, string label, int top, int min, int max, int value)
    {
        parent.Controls.Add(new Label { Text = label, Location = new Point(16, top), AutoSize = true });
        var track = new TrackBar
        {
            Location = new Point(120, top - 8),
            Width = 210,
            Minimum = min,
            Maximum = max,
            TickFrequency = 8,
            Value = value
        };
        parent.Controls.Add(track);
        return track;
    }

    private void UpdateStatus(string? message = null)
    {
        _statusLabel.Text = message ??
            $"Preview: {_previewIcon.Kind}, Size={_previewIcon.IconSize}, Stroke={_previewIcon.StrokeWidth}, Renderer={_previewIcon.RenderEngine}, Flip={_previewIcon.Flip}, Spin={_previewIcon.Spin}";
    }
}