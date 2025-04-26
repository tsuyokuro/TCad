using CadDataTypes;
using MessagePack;
using OpenTK.Mathematics;
using TCad.Plotter;
using TCad.Plotter.Serializer;
using SplineCurve;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Model.HalfEdgeModel;

namespace TCad.Plotter.Serializer.v1004;

public class VersionCode_v1004
{
    private static VersionCode Version_ = new VersionCode(1, 0, 0, 4);

    public static VersionCode Version => Version_;
}

[MessagePackObject]
public class MpCadData_v1004
{
    [Key("Version")]
    public VersionCode Version = VersionCode_v1004.Version;

    [Key("DB")]
    public MpCadObjectDB_v1004 MpDB;

    [Key("ViewInfo")]
    public MpViewInfo_v1004 ViewInfo;


    public void Store(SerializeContext sc, CadData cadData)
    {
        MpDB = new MpCadObjectDB_v1004();
        MpDB.Store(sc, cadData.DB);

        ViewInfo = new MpViewInfo_v1004();
        ViewInfo.Store(sc, cadData.WorldScale, cadData.PageSize);
    }

    public CadData Restore(DeserializeContext dsc)
    {
        CadData cd = new CadData();

        var viewInfo = ViewInfo.Restore(dsc);

        cd.WorldScale = viewInfo.worldScale;
        cd.PageSize = viewInfo.paperPageSize;

        cd.DB = MpDB.Restore(dsc);

        return cd;
    }
}

[MessagePackObject]
public class MpViewInfo_v1004
{
    [Key("WorldScale")]
    public vcompo_t WorldScale = (vcompo_t)1.0;

    [Key("Paper")]
    public MpPaperSettings_v1004 PaperSettings = new MpPaperSettings_v1004();

    public void Store(SerializeContext sc, vcompo_t worldScale, PaperPageSize pp)
    {
        WorldScale = worldScale;
        PaperSettings.Store(pp);
    }

    public (vcompo_t worldScale, PaperPageSize paperPageSize) Restore(DeserializeContext dsc)
    {
        if (WorldScale == 0)
        {
            WorldScale = (vcompo_t)1.0;
        }

        var pps = PaperSettings.Restore();

        return (WorldScale, pps);
    }
}

[MessagePackObject]
public class MpPaperSettings_v1004
{
    [Key("W")]
    public vcompo_t Width = (vcompo_t)210.0;

    [Key("H")]
    public vcompo_t Height = (vcompo_t)297.0;

    [Key("Landscape")]
    public bool Landscape = false;

    [Key("Kind")]
    public PaperKind Kind = PaperKind.A4;

    public void Store(PaperPageSize pp)
    {
        Width = pp.Width;
        Height = pp.Height;

        Landscape = pp.mLandscape;

        Kind = pp.mPaperKind;
    }

    public PaperPageSize Restore()
    {
        PaperPageSize pp = new PaperPageSize();

        pp.Width = Width;
        pp.Height = Height;

        pp.mLandscape = Landscape;

        pp.mPaperKind = Kind;

        return pp;
    }
}

[MessagePackObject]
public class MpCadObjectDB_v1004
{
    [Key("LayerIdCnt")]
    public uint LayerIdCount;

    [Key("FigIdCnt")]
    public uint FigureIdCount;

    [Key("FigList")]
    public List<MpFigure_v1004> FigureList;

    [Key("LayerList")]
    public List<MpLayer_v1004> LayerList;

    [Key("CurrentLayerID")]
    public uint CurrentLayerID;

    public void Store(SerializeContext sc, CadObjectDB db)
    {
        LayerIdCount = db.LayerIdProvider.Counter;
        FigureIdCount = db.FigIdProvider.Counter;

        FigureList = MpUtil.FigureMapToMp<MpFigure_v1004>(sc, db.FigureMap);

        LayerList = MpUtil.LayerListToMp<MpLayer_v1004>(sc, db.LayerList);

        CurrentLayerID = db.CurrentLayerID;
    }

    public void GarbageCollect()
    {
        var idMap = new Dictionary<uint, MpFigure_v1004>();

        foreach (MpFigure_v1004 fig in FigureList)
        {
            idMap.Add(fig.ID, fig);
        }

        var activeSet = new HashSet<uint>();

        foreach (MpLayer_v1004 layer in LayerList)
        {
            foreach (uint id in layer.FigureIdList)
            {
                MpFigure_v1004 fig = idMap[id];

                fig.ForEachFigID(idMap, (a) =>
                {
                    activeSet.Add(a);
                });
            }
        }

        int i = FigureList.Count - 1;

        for (; i >= 0; i--)
        {
            MpFigure_v1004 fig = FigureList[i];

            if (!activeSet.Contains(fig.ID))
            {
                FigureList.RemoveAt(i);
            }
        }
    }

    public CadObjectDB Restore(DeserializeContext dsc)
    {
        CadObjectDB ret = new CadObjectDB();

        ret.LayerIdProvider.Counter = LayerIdCount;
        ret.FigIdProvider.Counter = FigureIdCount;

        // Figure map
        List<CadFigure> figList = MpUtil.FigureListFromMp(dsc, FigureList);

        var dic = new Dictionary<uint, CadFigure>();

        for (int i = 0; i < figList.Count; i++)
        {
            CadFigure fig = figList[i];

            dic.Add(fig.ID, fig);
            FigureList[i].TempFigure = fig;
        }

        ret.FigureMap = dic;


        // Child list
        for (int i = 0; i < figList.Count; i++)
        {
            MpFigure_v1004 mpfig = FigureList[i];
            SetFigChild(mpfig, dic);
        }


        // Layer map
        ret.LayerList = MpUtil.LayerListFromMp(dsc, LayerList, dic);

        ret.LayerMap = new Dictionary<uint, CadLayer>();

        for (int i = 0; i < ret.LayerList.Count; i++)
        {
            CadLayer layer = ret.LayerList[i];

            ret.LayerMap.Add(layer.ID, layer);
        }

        ret.CurrentLayerID = CurrentLayerID;

        return ret;
    }

    private void SetFigChild(MpFigure_v1004 mpfig, Dictionary<uint, CadFigure> dic)
    {
        for (int i = 0; i < mpfig.ChildIdList.Count; i++)
        {
            uint id = mpfig.ChildIdList[i];

            mpfig.TempFigure.ChildList.Add(dic[id]);
            dic[id].Parent = mpfig.TempFigure;
        }
    }
}



[MessagePackObject]
public class MpLayer_v1004 : IMpLayer
{
    [Key("ID")]
    public uint ID;

    [Key("Visible")]
    public bool Visible;

    [Key("Locked")]
    public bool Locked;

    [Key("FigIdList")]
    public List<uint> FigureIdList;

    public void Store(SerializeContext sc, CadLayer layer)
    {
        ID = layer.ID;
        Visible = layer.Visible;
        Locked = layer.Locked;

        FigureIdList = MpUtil.FigureListToIdList(layer.FigureList);
    }


    public CadLayer Restore(DeserializeContext dsc, Dictionary<uint, CadFigure> dic)
    {
        CadLayer ret = new CadLayer();
        ret.ID = ID;
        ret.Visible = Visible;
        ret.Locked = Locked;
        ret.FigureList = new List<CadFigure>();

        for (int i = 0; i < FigureIdList.Count; i++)
        {
            ret.AddFigure(dic[FigureIdList[i]]);
        }

        return ret;
    }
}

[MessagePackObject]
public class MpFigure_v1004 : IMpFigure
{
    public const uint VERSION = 0x00001000;

    [Key("V")]
    public uint V;

    [Key("ID")]
    public uint ID;

    [Key("Type")]
    public byte Type;

    [Key("Locked")]
    public bool Locked;

    [Key("ChildList")]
    public List<MpFigure_v1004> ChildList;

    [Key("ChildIdList")]
    public List<uint> ChildIdList;

    [Key("GeoData")]
    public MpGeometricData_v1004 GeoData;

    [Key("Name")]
    public string Name;

    [Key("LinePen")]
    public MpDrawPen_v1004 LinePen;

    [Key("FillBrush")]
    public MpDrawBrush_v1004 FillBrush;

    [IgnoreMember]
    public CadFigure TempFigure = null;

    public void Store(SerializeContext sc, CadFigure fig, bool withChild)
    {
        StoreCommon(sc, fig);

        if (withChild)
        {
            StoreChildList(sc, fig);
        }
        else
        {
            StoreChildIdList(fig);
        }
    }

    public virtual void ForEachFigID(Dictionary<uint, MpFigure_v1004> allMap, Action<uint> d)
    {
        d(ID);

        if (ChildIdList == null)
        {
            return;
        }

        int i;
        for (i = 0; i < ChildIdList.Count; i++)
        {
            uint id = ChildIdList[i];
            MpFigure_v1004 childFig = allMap[id];
            childFig.ForEachFigID(allMap, d);
        }
    }

    public void StoreCommon(SerializeContext sc, CadFigure fig)
    {
        V = VERSION;

        ID = fig.ID;
        Type = (byte)fig.Type;
        Locked = fig.Locked;

        GeoData = fig.GeometricDataToMp_v1004(sc);

        Name = fig.Name;

        LinePen = new MpDrawPen_v1004();
        LinePen.Store(fig.LinePen);

        FillBrush = new MpDrawBrush_v1004();
        FillBrush.Store(fig.FillBrush);
    }

    public void StoreChildIdList(CadFigure fig)
    {
        ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
    }

    public void StoreChildList(SerializeContext sc, CadFigure fig)
    {
        ChildList = MpUtil.FigureListToMp<MpFigure_v1004>(sc, fig.ChildList);
    }

    public void RestoreTo(DeserializeContext dsc, CadFigure fig)
    {
        fig.ID = ID;
        fig.Locked = Locked;


        if (ChildList != null)
        {
            fig.ChildList = MpUtil.FigureListFromMp(dsc, ChildList);

            for (int i = 0; i < fig.ChildList.Count; i++)
            {
                CadFigure c = fig.ChildList[i];
                c.Parent = fig;
            }
        }
        else
        {
            fig.ChildList.Clear();
        }

        fig.GeometricDataFromMp_v1004(dsc, GeoData);


        fig.Name = Name;

        fig.LinePen = LinePen.Restore();
        fig.FillBrush = FillBrush.Restore();
    }

    public CadFigure Restore(DeserializeContext dsc)
    {
        CadFigure fig = CadFigure.Create((CadFigure.Types)Type);

        RestoreTo(dsc, fig);

        return fig;
    }
}

[MessagePackObject]
public struct MpVector3_v1004 : IMpVector3
{
    [Key(0)]
    public vcompo_t X;

    [Key(1)]
    public vcompo_t Y;

    [Key(2)]
    public vcompo_t Z;


    public void Store(vector3_t v)
    {
        X = v.X;
        Y = v.Y;
        Z = v.Z;
    }

    public vector3_t Restore()
    {
        return new vector3_t(X, Y, Z);
    }
}

[MessagePackObject]
public struct MpColor4_v1004
{
    [Key(0)]
    public float R;

    [Key(1)]
    public float G;

    [Key(2)]
    public float B;

    [Key(3)]
    public float A;


    public void Store(Color4 c)
    {
        R = c.R;
        G = c.G;
        B = c.B;
        A = c.A;
    }

    public Color4 Restore()
    {
        return new Color4(R, G, B, A);
    }
}

[MessagePackObject]
public struct MpDrawPen_v1004
{
    [Key("color")]
    public MpColor4_v1004 Color4;

    [Key("W")]
    public float Width;

    public void Store(DrawPen v)
    {
        Color4.Store(v.Color4);
        Width = v.Width;
    }

    public DrawPen Restore()
    {
        return new DrawPen
        {
            Color4 = Color4.Restore(),
            Width = Width,
        };
    }
}

[MessagePackObject]
public struct MpDrawBrush_v1004
{
    [Key("color")]
    public MpColor4_v1004 Color4;


    public void Store(DrawBrush v)
    {
        Color4.Store(v.Color4);
    }

    public DrawBrush Restore()
    {
        return new DrawBrush
        {
            Color4 = Color4.Restore(),
        };
    }
}

[MessagePackObject]
public struct MpVertexAttr_v1004
{
    [Key("C")]
    public MpColor4_v1004 Color;

    [Key("N")]
    public MpVector3_v1004 Normal;

    public void Store(CadVertexAttr attr)
    {
        Color = new MpColor4_v1004();
        Color.Store(attr.Color);

        Normal = new MpVector3_v1004();
        Normal.Store(attr.Normal);
    }

    public CadVertexAttr Restore()
    {
        CadVertexAttr attr = new CadVertexAttr();

        attr.Color = Color.Restore();
        attr.Normal = Normal.Restore();
        return attr;
    }
}

[MessagePackObject]
public struct MpVertex_v1004 : IMpVertex
{
    [Key("flag")]
    public byte Flag;

    [Key("P")]
    public MpVector3_v1004 P;


    public MpVertex_v1004()
    {
    }

    public void Store(CadVertex v)
    {
        Flag = (byte)(v.Flag & ~CadVertex.SELECTED);

        P.X = v.X;
        P.Y = v.Y;
        P.Z = v.Z;
    }


    public CadVertex Restore()
    {
        CadVertex v = CadVertex.Create(P.X, P.Y, P.Z);
        v.Flag = Flag;

        return v;
    }
}

[Union(0, typeof(MpSimpleGeometricData_v1004))]
[Union(1, typeof(MpMeshGeometricData_v1004))]
[Union(2, typeof(MpNurbsLineGeometricData_v1004))]
[Union(3, typeof(MpNurbsSurfaceGeometricData_v1004))]
[Union(4, typeof(MpPictureGeometricData_v1004))]
[Union(5, typeof(MpPolyLinesGeometricData_v1004))]
public interface MpGeometricData_v1004
{
}

#region "GeometricData"

[MessagePackObject]
public class MpSimpleGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("PointList")]
    public List<MpVertex_v1004> PointList;
}

[MessagePackObject]
public class MpPolyLinesGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("IsLoop")]
    public bool IsLoop;


    [Key("PointList")]
    public List<MpVertex_v1004> PointList;
}


[MessagePackObject]
public class MpPictureGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("FilePathName")]
    public string FilePathName;

    [Key("PointList")]
    public List<MpVertex_v1004> PointList;

    [Key("Bytes")]
    public byte[] Bytes = null;

    [Key("base64")]
    public string Base64 = null;
}

// CadFigureMesh    
[MessagePackObject]
public class MpMeshGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("HeModel")]
    public MpHeModel_v1004 HeModel;
}

// CadFigureNurbsLine
[MessagePackObject]
public class MpNurbsLineGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("Nurbs")]
    public MpNurbsLine_v1004 Nurbs;
}

// CadFigureNurbsSurface
[MessagePackObject]
public class MpNurbsSurfaceGeometricData_v1004 : MpGeometricData_v1004
{
    [Key("Nurbs")]
    public MpNurbsSurface_v1004 Nurbs;
}
#endregion


[MessagePackObject]
public class MpHeModel_v1004
{
    [Key("VertexStore")]
    public List<MpVertex_v1004> VertexStore;

    [Key("NormalStore")]
    public List<MpVector3_v1004> NormalStore;

    [Key("FaceStore")]
    public List<MpHeFace_v1004> FaceStore;

    [Key("HalfEdgeList")]
    public List<MpHalfEdge_v1004> HalfEdgeList;

    [Key("HeIdCnt")]
    public uint HeIdCount;

    [Key("FaceIdCnt")]
    public uint FaceIdCount;


    public void Store(HeModel model)
    {
        VertexStore = MpUtil.VertexListToMp<MpVertex_v1004>(model.VertexStore);

        NormalStore = MpUtil.Vector3ListToMp<MpVector3_v1004>(model.NormalStore);

        FaceStore = MpUtil.HeFaceListToMp<MpHeFace_v1004>(model.FaceStore);

        HeIdCount = model.HeIdProvider.Counter;

        FaceIdCount = model.FaceIdProvider.Counter;

        List<HalfEdge> heList = model.GetHalfEdgeList();

        HalfEdgeList = MpUtil.HalfEdgeListToMp<MpHalfEdge_v1004>(heList);
    }

    public HeModel Restore()
    {
        HeModel ret = new HeModel();

        ret.VertexStore = MpUtil.VertexListFromMp(VertexStore);

        ret.NormalStore = MpUtil.Vector3ListFromMp(NormalStore);

        // Create dictionary
        Dictionary<uint, HalfEdge> dic = new Dictionary<uint, HalfEdge>();

        dic[0] = null;

        for (int i = 0; i < HalfEdgeList.Count; i++)
        {
            HalfEdge he = HalfEdgeList[i].Restore();
            dic.Add(he.ID, he);

            HalfEdgeList[i].TempHalfEdge = he;
        }

        // Create links
        for (int i = 0; i < HalfEdgeList.Count; i++)
        {
            HalfEdge he = HalfEdgeList[i].TempHalfEdge;
            he.Pair = dic[HalfEdgeList[i].PairID];
            he.Next = dic[HalfEdgeList[i].NextID];
            he.Prev = dic[HalfEdgeList[i].PrevID];
        }

        ret.FaceStore = MpUtil.HeFaceListFromMp(FaceStore, dic);

        ret.HeIdProvider.Counter = HeIdCount;

        ret.FaceIdProvider.Counter = FaceIdCount;

        return ret;
    }
}

[MessagePackObject]
public class MpHeFace_v1004 : IMpHeFace
{
    [Key("ID")]
    public uint ID;

    [Key("HeadID")]
    public uint HeadID;

    [Key("Normal")]
    public int Normal = HeModel.INVALID_INDEX;


    public void Store(HeFace face)
    {
        ID = face.ID;
        HeadID = face.Head.ID;
        Normal = face.Normal;
    }


    public HeFace Restore(Dictionary<uint, HalfEdge> dic)
    {
        HalfEdge he = dic[HeadID];

        HeFace ret = new HeFace(he);

        ret.ID = ID;

        ret.Normal = Normal;

        return ret;
    }
}

[MessagePackObject]
public class MpHalfEdge_v1004 : IMpHalfEdge
{
    [Key("ID")]
    public uint ID;

    [Key("Vertex")]
    public int Vertex;

    [Key("Face")]
    public int Face;

    [Key("Normal")]
    public int Normal;

    // Links
    [Key("Pair")]
    public uint PairID;

    [Key("Next")]
    public uint NextID;

    [Key("Prev")]
    public uint PrevID;

    [IgnoreMember]
    public HalfEdge TempHalfEdge = null;


    public void Store(HalfEdge he)
    {
        ID = he.ID;
        PairID = he.Pair != null ? he.Pair.ID : 0;
        NextID = he.Next != null ? he.Next.ID : 0;
        PrevID = he.Prev != null ? he.Prev.ID : 0;

        Vertex = he.Vertex;
        Face = he.Face;
        Normal = he.Normal;
    }

    // リンク情報はRestoreされない
    public HalfEdge Restore()
    {
        HalfEdge he = new HalfEdge();

        he.ID = ID;
        he.Vertex = Vertex;
        he.Normal = Normal;
        he.Face = Face;

        return he;
    }
}

[MessagePackObject]
public class MpNurbsLine_v1004
{
    [Key("CtrlCnt")]
    public int CtrlCnt;

    [Key("CtrlDataCnt")]
    public int CtrlDataCnt;

    [Key("Weights")]
    public vcompo_t[] Weights;

    [Key("CtrlPoints")]
    public List<MpVertex_v1004> CtrlPoints;

    [Key("CtrlOrder")]
    public int[] CtrlOrder;

    [Key("BSplineParam")]
    public MpBSplineParam_v1004 BSplineP;


    public void Store(NurbsLine src)
    {
        CtrlCnt = src.CtrlCnt;
        CtrlDataCnt = src.CtrlDataCnt;
        Weights = MpUtil.ArrayClone(src.Weights);
        CtrlPoints = MpUtil.VertexListToMp<MpVertex_v1004>(src.CtrlPoints);
        CtrlOrder = MpUtil.ArrayClone(src.CtrlOrder);

        BSplineP = new MpBSplineParam_v1004();
        BSplineP.Store(src.BSplineP);
    }

    public NurbsLine Restore()
    {
        NurbsLine nurbs = new NurbsLine();

        nurbs.CtrlCnt = CtrlCnt;
        nurbs.CtrlDataCnt = CtrlDataCnt;
        nurbs.Weights = MpUtil.ArrayClone(Weights);
        nurbs.CtrlPoints = MpUtil.VertexListFromMp(CtrlPoints);
        nurbs.CtrlOrder = MpUtil.ArrayClone(CtrlOrder);

        nurbs.BSplineP = BSplineP.Restore();

        return nurbs;
    }
}

[MessagePackObject]
public class MpNurbsSurface_v1004
{
    [Key("UCtrlCnt")]
    public int UCtrlCnt;

    [Key("VCtrlCnt")]
    public int VCtrlCnt;

    [Key("UCtrlDataCnt")]
    public int UCtrlDataCnt;

    [Key("VCtrlDataCnt")]
    public int VCtrlDataCnt;

    [Key("Weights")]
    public vcompo_t[] Weights;

    [Key("CtrlPoints")]
    public List<MpVertex_v1004> CtrlPoints;

    [Key("CtrlOrder")]
    public int[] CtrlOrder;

    [Key("UBSpline")]
    public MpBSplineParam_v1004 UBSpline;

    [Key("VBSpline")]
    public MpBSplineParam_v1004 VBSpline;


    public void Store(NurbsSurface src)
    {
        UCtrlCnt = src.UCtrlCnt;
        VCtrlCnt = src.VCtrlCnt;

        UCtrlDataCnt = src.UCtrlDataCnt;
        VCtrlDataCnt = src.VCtrlDataCnt;

        CtrlPoints = MpUtil.VertexListToMp<MpVertex_v1004>(src.CtrlPoints);

        Weights = MpUtil.ArrayClone(src.Weights);
        CtrlOrder = MpUtil.ArrayClone(src.CtrlOrder);

        UBSpline = new MpBSplineParam_v1004();
        UBSpline.Store(src.UBSpline);

        VBSpline = new MpBSplineParam_v1004();
        VBSpline.Store(src.VBSpline);
    }

    public NurbsSurface Restore()
    {
        NurbsSurface nurbs = new NurbsSurface();

        nurbs.UCtrlCnt = UCtrlCnt;
        nurbs.VCtrlCnt = VCtrlCnt;

        nurbs.UCtrlDataCnt = UCtrlDataCnt;
        nurbs.VCtrlDataCnt = VCtrlDataCnt;

        nurbs.CtrlPoints = MpUtil.VertexListFromMp(CtrlPoints);

        nurbs.Weights = MpUtil.ArrayClone(Weights);
        nurbs.CtrlOrder = MpUtil.ArrayClone(CtrlOrder);

        nurbs.UBSpline = UBSpline.Restore();
        nurbs.VBSpline = VBSpline.Restore();

        return nurbs;
    }
}


[MessagePackObject]
public class MpBSplineParam_v1004
{
    [Key("Degree")]
    public int Degree = 3;

    [Key("DivCnt")]
    public int DivCnt = 0;

    [Key("OutputCnt")]
    public int OutputCnt = 0;

    [Key("KnotCnt")]
    public int KnotCnt;

    [Key("Knots")]
    public vcompo_t[] Knots;

    [Key("CtrlCnt")]
    public int CtrlCnt;

    [Key("LowKnot")]
    public vcompo_t LowKnot = 0;

    [Key("HightKnot")]
    public vcompo_t HighKnot = 0;

    [Key("Step")]
    public vcompo_t Step = 0;


    public void Store(BSplineParam src)
    {
        Degree = src.Degree;
        DivCnt = src.DivCnt;
        OutputCnt = src.OutputCnt;
        KnotCnt = src.KnotCnt;
        Knots = MpUtil.ArrayClone(src.Knots);
        LowKnot = src.LowKnot;
        HighKnot = src.HighKnot;
        Step = src.Step;
    }

    public BSplineParam Restore()
    {
        BSplineParam bs = new BSplineParam();

        bs.Degree = Degree;
        bs.DivCnt = DivCnt;
        bs.OutputCnt = OutputCnt;
        bs.KnotCnt = KnotCnt;
        bs.Knots = MpUtil.ArrayClone(Knots);
        bs.LowKnot = LowKnot;
        bs.HighKnot = HighKnot;
        bs.Step = Step;

        return bs;
    }
}
