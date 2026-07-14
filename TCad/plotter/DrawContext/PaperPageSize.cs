using System;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace TCad.Plotter.DrawContexts;

public class PaperSizes
{
    private readonly static Dictionary<PaperKind, (vcompo_t Width, vcompo_t Height)> SizeMap = [];

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


public struct PaperPageSize
{
    // 1inchは何ミリ?
    public const vcompo_t MILLI_PER_INCH = (vcompo_t)(25.4);

    // A4縦
    public static PaperPageSize A4Portrate
    {
        get;
    } = new PaperPageSize(PaperKind.A4, false);

    // A4横
    public static PaperPageSize A4Landscape
    {
        get;
    } = new PaperPageSize(PaperKind.A4, true);

    // デフォルト A4縦
    public vcompo_t Width = (vcompo_t)(210.0);
    public vcompo_t Height = (vcompo_t)(297.0);

    public PaperKind PaperKind
    {
        get;
        set;
    } = PaperKind.A4;

    public bool IsLandscape
    {
        get;
        set;
    } = false;

    public PaperPageSize(PaperKind papaerKind, bool landscape)
    {
        PaperKind = papaerKind;
        IsLandscape = landscape;

        (Width, Height) = PaperSizes.GetSize(papaerKind, landscape);
    }

    public PaperPageSize(PageSettings settings)
    {
        Setup(settings);
    }

    public void Setup(PageSettings settings)
    {
        PaperKind = settings.PaperSize.Kind;

        IsLandscape = settings.Landscape;


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

    public readonly PaperSize PaperSize
    {
        get
        {
            PrintDocument pd = new();
            int cnt = pd.PrinterSettings.PaperSizes.Count;
            int i;

            for (i = 0; i < cnt; i++)
            {
                PaperSize ps = pd.PrinterSettings.PaperSizes[i];
                if (ps.Kind == PaperKind)
                {
                    return ps;
                }
            }

            return null;
        }
    }
}
