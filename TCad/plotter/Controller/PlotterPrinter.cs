//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using GLUtil;
using OpenTK.Graphics.OpenGL;
using Plotter.Settings;
using System.Drawing;
using TCad.Plotter;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;

namespace Plotter.Controller;

public class PlotterPrinter
{
    public void PrintPage(IPlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
    {
        Log.pl($"Dev Width:{deviceSize.Width} Height:{deviceSize.Height}");
#if PRINT_WITH_GL_ONLY
        Bitmap bmp = GetPrintableBmp(pc, pageSize, deviceSize);
        printerGraphics.DrawImage(bmp, 0, 0);
#elif PRINT_WITH_GDI_ONLY
        PrintPageGDI(printerGraphics, pageSize, deviceSize);
#else
        PrintPageSwitch(pc, printerGraphics, pageSize, deviceSize);
#endif
    }

    private void PrintPageSwitch(IPlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
    {
        if (pc.DC.GetType() == typeof(DrawContextGLPers) || SettingsHolder.Settings.PrintWithBitmap)
        {
            Bitmap bmp = GetPrintableBmp(pc, pageSize, deviceSize);
            printerGraphics.DrawImage(bmp, 0, 0);
        }
        else
        {
            DrawContextPrinter dc = new DrawContextPrinter(pc.DC, printerGraphics, pageSize, deviceSize);
            dc.SetupTools(DrawModes.PRINTER);

            pc.Drawer.DrawFiguresRaw(dc);
        }
    }

    private static Bitmap GetPrintableBmp(IPlotterController pc, CadSize2D pageSize, CadSize2D deviceSize)
    {
        if (!(pc.DC is DrawContextGL))
        {
            return null;
        }

        vcompo_t upRes = (vcompo_t)(1.0);

        deviceSize *= upRes;

        DrawContext dc = pc.DC.CreatePrinterContext(pageSize, deviceSize);
        dc.SetupTools(DrawModes.PRINTER);

        // Bitmapを印刷すると大きさが変わるので、補正
        vcompo_t mag = SettingsHolder.Settings.MagnificationBitmapPrinting;

        dc.UnitPerMilli *= mag;

        vector3_t org = dc.ViewOrg;

        org *= mag;

        dc.SetViewOrg(org);

        FrameBufferW frameBuffer = new FrameBufferW();
        frameBuffer.Create((int)deviceSize.Width, (int)deviceSize.Height);

        frameBuffer.Begin();

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

        pc.Drawer.DrawFiguresRaw(dc);

        // EnableCap.LineSmoothがONだと線が太くなる(謎)
        GL.Disable(EnableCap.LineSmooth);

        dc.EndDraw();

        Bitmap bmp = frameBuffer.GetBitmap();
        Bitmap rsBmp = bmp;

        frameBuffer.End();
        frameBuffer.Dispose();

        if (upRes != (vcompo_t)(1.0))
        {
            rsBmp = BitmapUtil.ResizeBitmap(bmp,
                                    (int)(bmp.Width / upRes),
                                    (int)(bmp.Height / upRes),
                                    System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic);
        }

        return rsBmp;
    }
}
