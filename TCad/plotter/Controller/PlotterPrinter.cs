//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using GLUtil;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Plotter.Settings;
using System.Drawing;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace Plotter.Controller;

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
            dc.SetupTools(DrawModes.PRINTER, PenWidth);

            pc.DrawFiguresRaw(dc);
        }
    }

    private static Bitmap GetPrintableBmp(PlotterController pc, CadSize2D pageSize, CadSize2D deviceSize)
    {
        if (!(pc.DC is DrawContextGL))
        {
            return null;
        }

        vcompo_t upRes = (vcompo_t)(1.0);

        deviceSize *= upRes;

        DrawContext dc = pc.DC.CreatePrinterContext(pageSize, deviceSize);
        dc.SetupTools(DrawModes.PRINTER, 2);

        // Bitmapを印刷すると大きさが変わるので、補正
        vcompo_t f = SettingsHolder.Settings.MagnificationBitmapPrinting;
        dc.UnitPerMilli *= f;
        //DC.UnitPerMilli *= (vcompo_t)(0.96);

        vector3_t org = dc.ViewOrg;

        //org *= (vcompo_t)(0.96);
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
