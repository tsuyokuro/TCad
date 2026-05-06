using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using OpenTK.Mathematics;
using TCad.Controls;
using TCad.Plotter;
using TCad.Util;

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

        add_color_button.Click += add_color_button_Click;
        remove_color_button.Click += remove_color_button_Click;
        select_color_button.Click += select_color_button_Click;

        Loaded += Dialog_Loaded;
        Closed += (sender, e) =>
        {
            SaveColorList("color_list.json");
        };

        color_maker.SelectedColorChanged += Color_maker_SelectedColorChanged;
    }

    private void add_color_button_Click(object sender, RoutedEventArgs e)
    {
        if (InvalidColor)
        {
            return;
        }

        Color wpfColor = Color.FromArgb(
            (byte)(SelectedColor.A * 255.0f),
            (byte)(SelectedColor.R * 255.0f),
            (byte)(SelectedColor.G * 255.0f),
            (byte)(SelectedColor.B * 255.0f));

        string name = $"#{SelectedColor.ToArgb():X8}";

        color_list_box.AddColor(name, wpfColor);
    }

    private void remove_color_button_Click(object sender, RoutedEventArgs e)
    {
        color_list_box.RemoveColor();
    }

    private void select_color_button_Click(object sender, RoutedEventArgs e)
    {
        if (color_list_box.SelectedIndex < 0)
        {
            return;
        }

        ColorListBox.Item listItem = color_list_box.GetAt(color_list_box.SelectedIndex);

        color_maker.SelectedColor =
            new ColorMaker.Color(
                listItem.Brush.Color.R / 255.0f,
                listItem.Brush.Color.G / 255.0f,
                listItem.Brush.Color.B / 255.0f,
                listItem.Brush.Color.A / 255.0f);
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


        LoadColorList("color_list.json");

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

    private void SaveColorList(string fname)
    {
        string pathName = FileUtil.PathNameOnExeDir(fname);

        string data = color_list_box.ToJson();

        File.WriteAllText(pathName, data);
    }

    private void LoadColorList(string fname)
    {
        string pathName = FileUtil.PathNameOnExeDir(fname);

        if (!File.Exists(pathName)) return;

        string data = File.ReadAllText(pathName);

        if (data == null) return;
        if (data.Length == 0) return;

        color_list_box.FromJson(data);
    }
}
