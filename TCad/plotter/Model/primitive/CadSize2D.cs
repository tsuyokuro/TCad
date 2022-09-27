using System.Windows.Forms;

namespace Plotter
{
    public struct CadSize2D
    {
        public double Width;
        public double Height;

        public CadSize2D(double w, double h)
        {
            Width = w;
            Height = h;
        }

        public static CadSize2D operator * (CadSize2D me,double f)
        {
            return new CadSize2D(me.Width * f, me.Height * f);
        } 
    }
}
