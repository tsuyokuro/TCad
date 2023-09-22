//#define DEFAULT_DATA_TYPE_DOUBLE
using System.Collections.Generic;



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


namespace Plotter.Controller;

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
