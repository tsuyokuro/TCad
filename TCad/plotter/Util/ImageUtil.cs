using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter;

public class ImageUtil
{
    public static Image ByteArrayToImage(byte[] b)
    {
        ImageConverter imgconv = new ImageConverter();
        Image img = (Image)imgconv.ConvertFrom(b);
        return img;
    }

    public static byte[] ImageToByteArray(Image img)
    {
        ImageConverter imgconv = new ImageConverter();
        byte[] b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
        return b;
    }
}
