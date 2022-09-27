//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using GLUtil;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Settings;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace Plotter.Controller
{
    public class PlotterPrinter
    {
        public int PenWidth = 1;

        public void PrintPage(PlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            DOut.pl($"Dev Width:{deviceSize.Width} Height:{deviceSize.Height}");
#if PRINT_WITH_GL_ONLY
            Bitmap bmp = GetPrintableBmp(pc, pageSize, deviceSize);
            printerGraphics.DrawImage(bmp, 0, 0);
#elif PRINT_WITH_GDI_ONLY
            PrintPageGDI(printerGraphics, pageSize, deviceSize);
#else
            PrintPageSwitch(pc, printerGraphics, pageSize, deviceSize);
#endif
        }

        private void PrintPageSwitch(PlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (pc.DC.GetType() == typeof(DrawContextGLPers) || SettingsHolder.Settings.PrintWithBitmap)
            {
                Bitmap bmp = GetPrintableBmp(pc, pageSize, deviceSize);
                printerGraphics.DrawImage(bmp, 0, 0);
            }
            else
            {
                DrawContextPrinter dc = new DrawContextPrinter(pc.DC, printerGraphics, pageSize, deviceSize);
                dc.SetupTools(DrawTools.DrawMode.PRINTER, PenWidth);

                pc.DrawFiguresRaw(dc);
            }
        }

        private static Bitmap GetPrintableBmp(PlotterController pc, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(pc.DC is DrawContextGL))
            {
                return null;
            }

            double upRes = 1.0;

            deviceSize *= upRes;

            DrawContext dc = pc.DC.CreatePrinterContext(pageSize, deviceSize);
            dc.SetupTools(DrawTools.DrawMode.PRINTER, 2);

            // Bitmapを印刷すると大きさが変わるので、補正
            double f = SettingsHolder.Settings.MagnificationBitmapPrinting;
            dc.UnitPerMilli *= f;
            //dc.UnitPerMilli *= 0.96;

            Vector3d org = dc.ViewOrg;

            //org *= 0.96;
            org *= f;

            dc.SetViewOrg(org);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            if (SettingsHolder.Settings.PrintLineSmooth)
            {
                GL.Enable(EnableCap.LineSmooth);
            }
            else
            {
                GL.Disable(EnableCap.LineSmooth);
            }

            GL.LineWidth((float)upRes);

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            pc.DrawFiguresRaw(dc);

            GL.Enable(EnableCap.LineSmooth);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();
            Bitmap rsBmp = bmp;

            fb.End();
            fb.Dispose();

            if (upRes != 1.0)
            {
                rsBmp = BitmapUtil.ResizeBitmap(bmp,
                                        (int)(bmp.Width / upRes),
                                        (int)(bmp.Height / upRes),
                                        System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic);
            }

            return rsBmp;
        }
    }
}
