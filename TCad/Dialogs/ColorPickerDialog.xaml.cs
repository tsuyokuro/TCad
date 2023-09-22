//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using Plotter;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TCad.Controls;


#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad;

public partial class ColorPickerDialog : Window
{
    Color4 mSelectedColor;

		public Color4 SelectedColor
		{
        get
        {
            return mSelectedColor;
        }
			set
        {
            if (value.IsInvalid())
            {
                InvalidColor = true;
                return;
            }
            mSelectedColor = value;
        }
		}

    public bool InvalidColor
    {
        get;
        set;
    } = false;

    public ColorPickerDialog()
    {
        InitializeComponent();

        SelectedColor = Color4.Blue;

        ok_button.Click += Ok_button_Click;
        cancel_button.Click += Cancel_button_Click;
        invalid_color_button.Click += Invalid_color_button_Click;

        Loaded += Dialog_Loaded;

        color_maker.SelectedColorChanged += Color_maker_SelectedColorChanged;
    }

    private void Invalid_color_button_Click(object sender, RoutedEventArgs e)
    {
        InvalidColor = !InvalidColor;
        UpdatePreview();
    }

    private void Color_maker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<ColorMaker.Color> e)
    {
        float r = e.NewValue.R;
        float g = e.NewValue.G;
        float b = e.NewValue.B;
        float a = e.NewValue.A;

        SelectedColor = new Color4(r, g, b, a);

        InvalidColor = false;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        Color wpfColor = Color.FromArgb(
            (byte)(SelectedColor.A * 255.0f),
            (byte)(SelectedColor.R * 255.0f),
            (byte)(SelectedColor.G * 255.0f),
            (byte)(SelectedColor.B * 255.0f));

        preview_rect.Fill = new SolidColorBrush(wpfColor);

        if (InvalidColor)
        {
            preview_rect.Visibility = Visibility.Collapsed;
            preview_invalid_color_label.Visibility = Visibility.Visible;
        }
        else
        {
            preview_rect.Visibility = Visibility.Visible;
            preview_invalid_color_label.Visibility = Visibility.Collapsed;
        }
    }

    private void Dialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            HandleOK();
        }
        else if (e.Key == Key.Escape)
        {
            HandleCancel();
        }
    }

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        color_maker.SelectedColor =
            new ColorMaker.Color(
                SelectedColor.R,
                SelectedColor.G,
                SelectedColor.B,
                SelectedColor.A);

        UpdatePreview();
    }

    private void Cancel_button_Click(object sender, RoutedEventArgs e)
    {
        HandleCancel();
    }

    private void Ok_button_Click(object sender, RoutedEventArgs e)
    {
        HandleOK();
    }

    private void HandleCancel()
    {
        DialogResult = false;
    }

    private void HandleOK()
    {
        bool ret = true;

        DialogResult = ret;
    }
}
