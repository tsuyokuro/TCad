using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCad.Util;

namespace TCad.Controls;

/// <summary>
/// ColorList.xaml の相互作用ロジック
/// </summary>
public partial class ColorListBox : UserControl
{
    public class Item {
        public SolidColorBrush Brush { get; set; }
        public string Name { get; set; }
    }

    public int SelectedIndex {
        get
        {
            return ColorList.SelectedIndex;
        }

        set
        {
            ColorList.SelectedIndex = value;
        }
    }


    ObservableCollection<Item> Items_ = new();

    public Collection<Item> Items
    {
        get => Items_;
    }

    public void AddColor(string name, Color color)
    {
        Items_.Add(new Item { Name=name, Brush=new SolidColorBrush(color)});
    }

    public void RemoveColor(int idx)
    {
        Items_.RemoveAt(idx);
    }

    public void RemoveColor()
    {
        if (ColorList.SelectedIndex < 0)
        {
            return;
        }

        Items_.RemoveAt(ColorList.SelectedIndex);
    }

    public void Clear()
    {
        Items_.Clear();
    }

    public Item GetAt(int index)
    {
        return Items_[index];
    }

    public string ToJson()
    {
        JsonArray array = new JsonArray();

        foreach (var item in Items_)
        {
            JsonObject jitem = new JsonObject();
            jitem.Add("name", item.Name);

            JsonArray color = [
                item.Brush.Color.A,
                item.Brush.Color.R,
                item.Brush.Color.G,
                item.Brush.Color.B
                ];

            jitem.Add("color", color);

            array.Add(jitem);
        }

        JsonObject root = new JsonObject();
        root.Add("color_list", array);

        string s = JsonSerializer.Serialize(root);

        return s;
    }

    public void FromJson(string json)
    {
        JsonDocument jdoc = JsonDocument.Parse(json);

        JsonElement jelement;

        jdoc.RootElement.TryGetProperty("color_list", out jelement);

        var jcolorList = jelement.EnumerateArray().ToList();

        List<ColorListBox.Item> items = new();


        Clear();

        foreach (var item in jcolorList)
        {
            string name = item.GetProperty("name").GetString();

            var jarray = item.GetProperty("color");

            Color color = Color.FromArgb(
                    jarray[0].GetByte(0),
                    jarray[1].GetByte(0),
                    jarray[2].GetByte(0),
                    jarray[3].GetByte(0)
                );

            AddColor(name, color);
        }
    }

    public ColorListBox()
    {
        InitializeComponent();
        ColorList.ItemsSource = Items_;
        ColorList.SelectionChanged += ColorList_SelectionChanged;
    }

    private void ColorList_SelectionChanged(object sender, EventArgs e)
    {

    }
}
