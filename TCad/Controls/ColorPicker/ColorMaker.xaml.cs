using Plotter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TCad.Controls;

public partial class ColorMaker : UserControl
{
    public struct Color
    {
        public float A = 0;
        public float R = 0;
        public float G = 0;
        public float B = 0;

        public static Color FromRGBA(byte r, byte g, byte b, byte a)
        {
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public static Color Default = FromRGBA(255, 255, 255, 255);

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public static readonly DependencyProperty SelectedColorProperty;
    public static readonly RoutedEvent SelectedColorChangedEvent;

    public Color SelectedColor
    {
        get
        {
            return (Color)GetValue(SelectedColorProperty);
        }

        set
        {
            R = value.R;
            G = value.G;
            B = value.B;
            A = value.A;

            ColorSpaceUtil.RgbToHsl(R, G, B, out H, out S, out Y);

            SetValue(SelectedColorProperty, value);
        }
    }

    public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
    {
        add { AddHandler(SelectedColorChangedEvent, value); }
        remove { RemoveHandler(SelectedColorChangedEvent, value); }
    }

    private double R = 0;
    private double G = 0;
    private double B = 0;
    private double A = 1;

    private double H;
    private double S = 1.0;
    private double Y = 1.0;

    private bool ignoreValueChangeEvent = false;

    static ColorMaker()
    {
        SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(ColorMaker.Color), typeof(ColorMaker),
            new FrameworkPropertyMetadata(ColorMaker.Color.Default, new PropertyChangedCallback(OnSelectedColorChanged)));

        SelectedColorChangedEvent = EventManager.RegisterRoutedEvent("SelectedColorChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ColorMaker.Color>), typeof(ColorMaker));
    }

    public ColorMaker()
    {
        InitializeComponent();
        Loaded += ColorPicker_Loaded;

        double small = 1.0 / 255.0;
        double large = 1.0 / 255.0;
        double maxv = 1.0;

        r_slider.Minimum = 0;
        r_slider.Maximum = maxv;
        r_slider.LargeChange = large;
        r_slider.SmallChange = small;

        g_slider.Minimum = 0;
        g_slider.Maximum = maxv;
        g_slider.LargeChange = large;
        g_slider.SmallChange = small;

        b_slider.Minimum = 0;
        b_slider.Maximum = maxv;
        b_slider.LargeChange = large;
        b_slider.SmallChange = small;

        a_slider.Minimum = 0;
        a_slider.Maximum = maxv;
        a_slider.LargeChange = large;
        a_slider.SmallChange = small;


        hue_slider.Minimum = 0;
        hue_slider.Maximum = 359;
        hue_slider.IsMoveToPointEnabled = true;


        small = 0.001;
        large = 0.01;
        maxv = 1.0;

        s_slider.Minimum = 0;
        s_slider.Maximum = maxv;
        s_slider.LargeChange = large;
        s_slider.SmallChange = small;

        y_slider.Minimum = 0;
        y_slider.Maximum = maxv;
        y_slider.LargeChange = large;
        y_slider.SmallChange = small;



        r_slider.ValueChanged += R_slider_ValueChanged;
        g_slider.ValueChanged += G_slider_ValueChanged;
        b_slider.ValueChanged += B_slider_ValueChanged;
        a_slider.ValueChanged += A_slider_ValueChanged;

        hue_slider.ValueChanged += Hue_slider_ValueChanged;
        s_slider.ValueChanged += S_slider_ValueChanged;
        y_slider.ValueChanged += Y_slider_ValueChanged;

        UpdateSelectedColor();
    }

    private static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        ColorMaker control = (ColorMaker)obj;
        var e = new RoutedPropertyChangedEventArgs<Color>((Color)args.OldValue, (Color)args.NewValue, SelectedColorChangedEvent);
        control.OnSelectedColorChanged(e);
    }

    protected virtual void OnSelectedColorChanged(RoutedPropertyChangedEventArgs<Color> args)
    {
        DOut.plx("in");

        ForceUpdateWithRGB();
        RaiseEvent(args);
    }

    private void Hue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        H = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        UpdateSelectedColor();
    }

    private void S_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        S = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        UpdateSelectedColor();
    }

    private void Y_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        Y = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        UpdateSelectedColor();
    }

    private void R_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        R = r_slider.Value;

        UpdateSelectedColor();
    }

    private void G_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        G = g_slider.Value;

        UpdateSelectedColor();
    }

    private void B_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        B = b_slider.Value;

        UpdateSelectedColor();
    }

    private void A_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        A = a_slider.Value;

        UpdateSelectedColor();
    }

    private void ForceUpdateWithRGB()
    {
        ignoreValueChangeEvent = true;

        ColorSpaceUtil.RgbToHsl(R, G, B, out H, out S, out Y);

        r_slider.Value = R;
        r_value.Content = string.Format("{0:0.000}", R);
        r_byte_value.Content = (int)(R * 255.0);

        g_slider.Value = G;
        g_value.Content = string.Format("{0:0.000}", G);
        g_byte_value.Content = (int)(G * 255.0);

        b_slider.Value = B;
        b_value.Content = string.Format("{0:0.000}", B);
        b_byte_value.Content = (int)(B * 255.0);

        a_slider.Value = A;
        a_value.Content = string.Format("{0:0.000}", A);
        a_byte_value.Content = (int)(A * 255.0);


        hue_slider.Value = H;
        hue_value.Content = string.Format("{0:0.0}", H);

        s_slider.Value = S;
        s_value.Content = string.Format("{0:0.000}", S);

        y_slider.Value = Y;
        y_value.Content = string.Format("{0:0.000}", Y);

        ignoreValueChangeEvent = false;
    }

    private void UpdateSelectedColor()
    {
        SelectedColor = new Color((float)R, (float)G, (float)B, (float)A);
    }

    private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
    {
        
    }
}
