using System.ComponentModel;
using System.Drawing.Drawing2D;
using LucideSharp.WinForms.Rendering;

namespace LucideSharp.WinForms;

/// <summary>
/// A Windows Forms control that displays a Lucide SVG icon.
/// </summary>
[ToolboxItem(true)]
[DefaultProperty(nameof(Kind))]
[DefaultEvent(nameof(Click))]
[DesignerCategory("Code")]
public class LucideIcon : Control
{
    private LucideKind _kind = LucideKind.Heart;
    private int _iconSize = 24;
    private float _strokeWidth = 2f;
    private float _rotation;
    private FlipMode _flip = FlipMode.None;
    private RenderEngine _renderEngine = RenderEngine.SvgSkia;
    private bool _spin;
    private readonly System.Windows.Forms.Timer _spinTimer;
    private Bitmap? _cachedBitmap;

    public LucideIcon()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);

        Size = new Size(32, 32);
        BackColor = Color.Transparent;
        TabStop = false;

        _spinTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _spinTimer.Tick += (_, _) =>
        {
            _rotation = (_rotation + 6f) % 360f;
            Invalidate();
        };
    }

    /// <summary>Gets or sets the Lucide icon to display.</summary>
    [Category("Appearance")]
    [Description("The Lucide icon kind to display.")]
    [DefaultValue(LucideKind.Heart)]
    public LucideKind Kind
    {
        get => _kind;
        set
        {
            if (_kind == value)
            {
                return;
            }

            _kind = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets the rendered icon size in pixels.</summary>
    [Category("Appearance")]
    [Description("The rendered icon size in pixels.")]
    [DefaultValue(24)]
    public int IconSize
    {
        get => _iconSize;
        set
        {
            var size = Math.Max(1, value);
            if (_iconSize == size)
            {
                return;
            }

            _iconSize = size;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets the icon stroke color.</summary>
    [Category("Appearance")]
    [Description("The icon stroke color.")]
    public override Color ForeColor
    {
        get => base.ForeColor == default ? Color.Black : base.ForeColor;
        set
        {
            if (base.ForeColor == value)
            {
                return;
            }

            base.ForeColor = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets the SVG stroke width.</summary>
    [Category("Appearance")]
    [Description("The SVG stroke width.")]
    [DefaultValue(2f)]
    public float StrokeWidth
    {
        get => _strokeWidth;
        set
        {
            if (_strokeWidth.Equals(value))
            {
                return;
            }

            _strokeWidth = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets the icon rotation in degrees.</summary>
    [Category("Appearance")]
    [Description("The icon rotation in degrees.")]
    [DefaultValue(0f)]
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_spin || _rotation.Equals(value))
            {
                return;
            }

            _rotation = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets whether the icon should spin continuously.</summary>
    [Category("Behavior")]
    [Description("When enabled, the icon spins continuously.")]
    [DefaultValue(false)]
    public bool Spin
    {
        get => _spin;
        set
        {
            if (_spin == value)
            {
                return;
            }

            _spin = value;
            if (_spin)
            {
                _spinTimer.Start();
            }
            else
            {
                _spinTimer.Stop();
                _rotation = 0f;
                RefreshBitmap();
            }

            Invalidate();
        }
    }

    /// <summary>Gets or sets the flip mode.</summary>
    [Category("Appearance")]
    [Description("Flips the icon horizontally, vertically, or both.")]
    [DefaultValue(FlipMode.None)]
    public FlipMode Flip
    {
        get => _flip;
        set
        {
            if (_flip == value)
            {
                return;
            }

            _flip = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    /// <summary>Gets or sets the SVG rendering engine.</summary>
    [Category("Behavior")]
    [Description("The SVG rendering engine used to rasterize the icon.")]
    [DefaultValue(RenderEngine.SvgSkia)]
    public RenderEngine RenderEngine
    {
        get => _renderEngine;
        set
        {
            if (_renderEngine == value)
            {
                return;
            }

            _renderEngine = value;
            RefreshBitmap();
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_cachedBitmap is null)
        {
            RefreshBitmap();
        }

        if (_cachedBitmap is null)
        {
            return;
        }

        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var x = (Width - _cachedBitmap.Width) / 2f;
        var y = (Height - _cachedBitmap.Height) / 2f;

        if (_spin && !_rotation.Equals(0f))
        {
            var state = e.Graphics.Save();
            e.Graphics.TranslateTransform(Width / 2f, Height / 2f);
            e.Graphics.RotateTransform(_rotation);
            e.Graphics.TranslateTransform(-Width / 2f, -Height / 2f);
            e.Graphics.DrawImage(_cachedBitmap, x, y);
            e.Graphics.Restore(state);
        }
        else
        {
            e.Graphics.DrawImage(_cachedBitmap, x, y);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _spinTimer.Stop();
            _spinTimer.Dispose();
            _cachedBitmap?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void RefreshBitmap()
    {
        _cachedBitmap?.Dispose();
        _cachedBitmap = null;

        var options = new IconRenderOptions(
            _kind,
            _iconSize,
            ForeColor,
            _strokeWidth,
            _spin ? 0f : _rotation,
            _flip,
            _renderEngine);

        _cachedBitmap = (Bitmap)LucideRenderer.Render(options).Clone();
    }
}