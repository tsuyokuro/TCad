using System;
using System.Collections.Generic;
using System.Drawing.Printing;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public class PaperSizes
{
    public static Dictionary<PaperKind, (vcompo_t Width, vcompo_t Height)> SizeMap =
        new Dictionary<PaperKind, (vcompo_t Width, vcompo_t Height)>();

    public static (vcompo_t Width, vcompo_t Height) GetSize(PaperKind kind, bool landscape)
    {
        (vcompo_t w, vcompo_t h) = SizeMap[kind];

        if (landscape)
        {
            return (h, w);
        }

        return (w, h);
    }

    static PaperSizes()
    {
        SizeMap[PaperKind.A4] = ((vcompo_t)(210.0), (vcompo_t)(297.0));
        SizeMap[PaperKind.A5] = ((vcompo_t)(148.0), (vcompo_t)(210.0));
        SizeMap[PaperKind.A6] = ((vcompo_t)(105.0), (vcompo_t)(148.0));

        SizeMap[PaperKind.B5] = ((vcompo_t)(182.0), (vcompo_t)(257.0));
    }
}


public class PaperPageSize
{
    // 1inchは何ミリ?
    public const vcompo_t MILLI_PER_INCH = (vcompo_t)(25.4);

    // デフォルト A4縦
    public vcompo_t Width = (vcompo_t)(210.0);
    public vcompo_t Height = (vcompo_t)(297.0);

    public PaperKind mPaperKind = PaperKind.A4;

    public bool mLandscape = false;

    public PaperPageSize()
    {
    }

    public PaperPageSize(PaperKind papaerKind, bool landscape)
    {
        mPaperKind = papaerKind;
        mLandscape = landscape;

        (Width, Height) = PaperSizes.GetSize(papaerKind, landscape);
    }

    public void Setup(PageSettings settings)
    {
        mPaperKind = settings.PaperSize.Kind;

        mLandscape = settings.Landscape;


        // PageSettingsは、1/100 Inch単位で設定されているのでmmに変換

        Width =
            (vcompo_t)Math.Round(
                    settings.Bounds.Width * MILLI_PER_INCH / (vcompo_t)(100.0),
                    MidpointRounding.AwayFromZero);

        Height =
            (vcompo_t)Math.Round(
                    settings.Bounds.Height * MILLI_PER_INCH / (vcompo_t)(100.0),
                    MidpointRounding.AwayFromZero);
    }

    public bool IsLandscape()
    {
        return mLandscape;
    }

    public PaperSize GetPaperSize()
    {
        PrintDocument pd = new PrintDocument();
        int cnt = pd.PrinterSettings.PaperSizes.Count;
        int i;

        for (i = 0; i < cnt; i++)
        {
            PaperSize ps = pd.PrinterSettings.PaperSizes[i];
            if (ps.Kind == mPaperKind)
            {
                return ps;
            }
        }

        return null;
    }
}
