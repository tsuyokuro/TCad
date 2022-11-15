using Plotter;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            if (!ignoreSelectedColorChangedEvent)
            {
                ForceUpdateWithRGB(null);
            }
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
    private bool ignoreSelectedColorChangedEvent = false;

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
        RaiseEvent(args);
    }

    private void Hue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        H = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void S_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        S = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void Y_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        Y = e.NewValue;

        ColorSpaceUtil.HslToRgb(H, S, Y, out R, out G, out B);

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void R_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        R = r_slider.Value;

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void G_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        G = g_slider.Value;

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void B_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        B = b_slider.Value;

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void A_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ignoreValueChangeEvent) return;

        A = a_slider.Value;

        ForceUpdateWithRGB(sender);
        UpdateSelectedColor();
    }

    private void ForceUpdateWithRGB(Object triger)
    {
        ignoreValueChangeEvent = true;

        ColorSpaceUtil.RgbToHsl(R, G, B, out H, out S, out Y);

        SetSliderValue(triger, r_slider, R);
        SetValueText(triger, r_value, r_byte_value, R);

        SetSliderValue(triger, g_slider, G);
        SetValueText(triger, g_value, g_byte_value, G);

        SetSliderValue(triger, b_slider, B);
        SetValueText(triger, b_value, b_byte_value, B);

        SetSliderValue(triger, a_slider, A);
        SetValueText(triger, a_value, a_byte_value, A);

        SetSliderValue(triger, hue_slider, H);
        hue_value.Content = string.Format("{0:0.0}", H);

        SetSliderValue(triger, s_slider, S);
        s_value.Content = string.Format("{0:0.000}", S);

        SetSliderValue(triger, y_slider, Y);
        y_value.Content = string.Format("{0:0.000}", Y);

        ignoreValueChangeEvent = false;
    }

    private void SetSliderValue(Object triger, Slider slider, double v)
    {
        if (!ReferenceEquals(triger, slider))
        {
            slider.Value = v;
        }
    }

    private void SetValueText(Object triger, TextBox fText, TextBox iText, double v)
    {
        if (!ReferenceEquals(triger, fText))
        {
            fText.Text = string.Format("{0:0.000}", v);
        }

        if (!ReferenceEquals(triger, iText))
        {
            iText.Text = "" + (int)(v * 255.0);
        }
    }


    private void UpdateSelectedColor()
    {
        ignoreSelectedColorChangedEvent = true;

        SelectedColor = new Color((float)R, (float)G, (float)B, (float)A);

        ignoreSelectedColorChangedEvent = false;
    }

    private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
    {
        
    }

    private void TextChangedI(object sender, TextChangedEventArgs args)
    {
        if (ignoreValueChangeEvent)
        {
            return;
        }

        SetTextValueI(sender, r_byte_value, out R, R);
        SetTextValueI(sender, g_byte_value, out G, G);
        SetTextValueI(sender, b_byte_value, out B, B);
        SetTextValueI(sender, a_byte_value, out A, A);
    }

    private void TextChangedF(object sender, TextChangedEventArgs args)
    {
        if (ignoreValueChangeEvent)
        {
            return;
        }

        SetTextValueF(sender, r_value, out R, R);
        SetTextValueF(sender, g_value, out G, G);
        SetTextValueF(sender, b_value, out B, B);
        SetTextValueF(sender, a_value, out A, A);
    }

    void SetTextValueF(Object sender, TextBox t, out double outV, double current)
    {
        outV = current;
        if (!ReferenceEquals(sender, t))
        {
            return;
        }

        string s;
        double v;
        object triger = null;

        s = t.Text;
        if (double.TryParse(s, out v))
        {
            if (v < 0 || v > 1.0)
            {
                t.Text = string.Format("{0:0.000}", current);
            }
            else
            {
                outV = v;
                triger = t;
            }
        }

        if (triger != null)
        {
            ForceUpdateWithRGB(triger);
            UpdateSelectedColor();
        }
    }

    void SetTextValueI(Object sender, TextBox t, out double outV, double current)
    {
        outV = current;
        if (!ReferenceEquals(sender, t))
        {
            return;
        }

        string s;
        int v;
        object triger = null;

        s = t.Text;
        if (int.TryParse(s, out v))
        {
            if (v < 0 || v > 255)
            {
                t.Text = "" + (int)(current * 255.0);
            }
            else
            {
                outV = (double)v / 255.0;
                triger = t;
            }
        }

        if (triger != null)
        {
            ForceUpdateWithRGB(triger);
            UpdateSelectedColor();
        }
    }

    private Regex IntRegex = new Regex("[0-9]");
    private Regex FloatRegex = new Regex("[0-9.]");

    private void PreviewTextInputI(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IntRegex.IsMatch(e.Text);
    }

    private void PreviewTextInputF(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !FloatRegex.IsMatch(e.Text);
    }
}
