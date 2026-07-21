using System.ComponentModel;
using LucideSharp.WinForms.Rendering;

namespace LucideSharp.WinForms;

/// <summary>
/// A Windows Forms button that displays a Lucide SVG icon.
/// Inspired by FontAwesome.Sharp's <c>IconButton</c>.
/// </summary>
[ToolboxItem(true)]
[DesignTimeVisible(true)]
[DefaultProperty(nameof(Kind))]
[Description("A Windows Forms button supporting Lucide icons")]
public class IconButton : Button
{
    private LucideKind _kind = LucideKind.Heart;
    private Color _iconColor = Color.Black;
    private int _iconSize = 16;
    private float _strokeWidth = 2f;
    private float _rotation;
    private FlipMode _flip = FlipMode.None;
    private RenderEngine _renderEngine = RenderEngine.SvgSkia;
    private Image? _ownedImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconButton"/> class.
    /// </summary>
    public IconButton()
    {
        UpdateImage();
    }

    /// <summary>Gets or sets the Lucide icon to display.</summary>
    [Category("Lucide")]
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
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the icon stroke color.</summary>
    [Category("Lucide")]
    [Description("The icon stroke color.")]
    public Color IconColor
    {
        get => _iconColor;
        set
        {
            if (_iconColor == value)
            {
                return;
            }

            _iconColor = value;
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the rendered icon size in pixels.</summary>
    [Category("Lucide")]
    [Description("The rendered icon size in pixels.")]
    [DefaultValue(16)]
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
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the SVG stroke width.</summary>
    [Category("Lucide")]
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
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the icon rotation in degrees.</summary>
    [Category("Lucide")]
    [Description("The icon rotation in degrees.")]
    [DefaultValue(0f)]
    public float Rotation
    {
        get => _rotation;
        set
        {
            var v = value % 360f;
            if (_rotation.Equals(v))
            {
                return;
            }

            _rotation = v;
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the flip mode.</summary>
    [Category("Lucide")]
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
            UpdateImage();
        }
    }

    /// <summary>Gets or sets the SVG rendering engine.</summary>
    [Category("Lucide")]
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
            UpdateImage();
        }
    }

    /// <summary>
    /// Gets or sets the image displayed on the button.
    /// Hidden from the designer — use <see cref="Kind"/> and related properties instead.
    /// </summary>
    [ReadOnly(true)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Image? Image
    {
        get => base.Image;
        set => base.Image = value;
    }

    /// <summary>Prevents designer serialization of <see cref="Image"/>.</summary>
    public bool ShouldSerializeImage() => false;

    /// <summary>Returns whether <see cref="IconColor"/> should be serialized.</summary>
    private bool ShouldSerializeIconColor() => _iconColor != Color.Black;

    /// <summary>Resets <see cref="IconColor"/> to its default value.</summary>
    private void ResetIconColor() => IconColor = Color.Black;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeOwnedImage();
        }

        base.Dispose(disposing);
    }

    private void UpdateImage()
    {
        var previous = _ownedImage;
        _ownedImage = Lucide.GetImage(
            _kind,
            _iconSize,
            _iconColor,
            _strokeWidth,
            _rotation,
            _flip,
            _renderEngine);

        base.Image = _ownedImage;
        previous?.Dispose();
    }

    private void DisposeOwnedImage()
    {
        if (_ownedImage is null)
        {
            return;
        }

        if (ReferenceEquals(base.Image, _ownedImage))
        {
            base.Image = null;
        }

        _ownedImage.Dispose();
        _ownedImage = null;
    }
}
