using System.Collections.Generic;

namespace Plotter.Controller
{
    public class MenuInfo
    {
        public class Item
        {
            public string Text;
            public object Tag;

            public Item(string defaultText, object tag)
            {
                Text = defaultText;
                Tag = tag;
            }
        }

        public List<Item> Items = new List<Item>(20);
    }
}