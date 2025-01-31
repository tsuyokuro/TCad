using HalfEdgeNS;
using MessagePack;
using System;
using System.Collections.Generic;
using CadDataTypes;
using SplineCurve;
using System.Drawing.Printing;
using OpenTK.Mathematics;

namespace Plotter.Serializer;

public class VersionCode_v1003
{
    private static VersionCode Version_ = new VersionCode(1, 0, 0, 3);

    public static VersionCode Version => Version_;
}

[MessagePackObject]
public class MpCadData_v1003
{
    [Key("Version")]
    public VersionCode Version = VersionCode_v1003.Version;

    [Key("DB")]
    public MpCadObjectDB_v1003 MpDB;

    [Key("ViewInfo")]
    public MpViewInfo_v1003 ViewInfo;

    public static MpCadData_v1003 Create(SerializeContext sc, CadData cadData)
    {
        MpCadData_v1003 ret = new MpCadData_v1003();

        ret.MpDB = MpCadObjectDB_v1003.Create(sc, cadData.DB);

        ret.ViewInfo = new MpViewInfo_v1003();

        ret.ViewInfo.WorldScale = cadData.WorldScale;

        ret.ViewInfo.PaperSettings.Set(cadData.PageSize);

        return ret;
    }

    public CadData Restore(DeserializeContext dsc)
    {
        CadData cd = new CadData();

        vcompo_t worldScale = 0;

        PaperPageSize pps = null;

        if (ViewInfo != null)
        {
            worldScale = ViewInfo.WorldScale;

            if (ViewInfo.PaperSettings != null)
            {
                pps = ViewInfo.PaperSettings.GetPaperPageSize();
            }
        }


        if (worldScale == 0)
        {
            worldScale = (vcompo_t)(1.0);
        }

        cd.WorldScale = worldScale;


        if (pps == null)
        {
            pps = new PaperPageSize();
        }

        cd.PageSize = pps;

        cd.DB = MpDB.Restore(dsc);

        return cd;
    }
}

[MessagePackObject]
public class MpViewInfo_v1003
{
    [Key("WorldScale")]
    public vcompo_t WorldScale = (vcompo_t)(1.0);

    [Key("Paper")]
    public MpPaperSettings_v1003 PaperSettings = new MpPaperSettings_v1003();
}

[MessagePackObject]
public class MpPaperSettings_v1003
{
    [Key("W")]
    public vcompo_t Width = (vcompo_t)(210.0);

    [Key("H")]
    public vcompo_t Height = (vcompo_t)(297.0);

    [Key("Landscape")]
    public bool Landscape = false;

    [Key("Kind")]
    public PaperKind Kind = PaperKind.A4;

    public void Set(PaperPageSize pp)
    {
        Width = pp.Width;
        Height = pp.Height;

        Landscape = pp.mLandscape;

        Kind = pp.mPaperKind;
    }

    public PaperPageSize GetPaperPageSize()
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
public class MpCadObjectDB_v1003
{
    [Key("LayerIdCnt")]
    public uint LayerIdCount;

    [Key("FigIdCnt")]
    public uint FigureIdCount;

    [Key("FigList")]
    public List<MpFigure_v1003> FigureList;

    [Key("LayerList")]
    public List<MpLayer_v1003> LayerList;

    [Key("CurrentLayerID")]
    public uint CurrentLayerID;

    public static MpCadObjectDB_v1003 Create(SerializeContext sc, CadObjectDB db)
    {
        MpCadObjectDB_v1003 ret = new MpCadObjectDB_v1003();

        ret.LayerIdCount = db.LayerIdProvider.Counter;
        ret.FigureIdCount = db.FigIdProvider.Counter;

        //ret.FigureList = MpUtil_v1003.FigureMapToMp_v1003(db.FigureMap);
        ret.FigureList = MpUtil.FigureMapToMp<MpFigure_v1003>(sc, db.FigureMap);

        ret.LayerList = MpUtil.LayerListToMp<MpLayer_v1003>(sc, db.LayerList);

        ret.CurrentLayerID = db.CurrentLayerID;

        return ret;
    }

    public void GarbageCollect()
    {
        var idMap = new Dictionary<uint, MpFigure_v1003>();

        foreach (MpFigure_v1003 fig in FigureList)
        {
            idMap.Add(fig.ID, fig);
        }

        var activeSet = new HashSet<uint>();

        foreach (MpLayer_v1003 layer in LayerList)
        {
            foreach (uint id in layer.FigureIdList)
            {
                MpFigure_v1003 fig = idMap[id];

                fig.ForEachFigID(idMap, (a) =>
                {
                    activeSet.Add(a);
                });
            }
        }

        int i = FigureList.Count - 1;

        for (; i >= 0; i--)
        {
            MpFigure_v1003 fig = FigureList[i];

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
        List<CadFigure> figList = MpUtil.FigureListFromMp<MpFigure_v1003>(dsc, FigureList);

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
            MpFigure_v1003 mpfig = FigureList[i];
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

    private void SetFigChild(MpFigure_v1003 mpfig, Dictionary<uint, CadFigure> dic)
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
public class MpLayer_v1003 : MpLayer
{
    [Key("ID")]
    public uint ID;

    [Key("Visible")]
    public bool Visible;

    [Key("Locked")]
    public bool Locked;

    [Key("FigIdList")]
    public List<uint> FigureIdList;

    public static MpLayer_v1003 Create(SerializeContext sc, CadLayer layer)
    {
        MpLayer_v1003 ret = new MpLayer_v1003();

        ret.Store(sc, layer);

        return ret;
    }

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
public class MpFigure_v1003 : MpFigure
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
    public List<MpFigure_v1003> ChildList;

    [Key("ChildIdList")]
    public List<uint> ChildIdList;

    [Key("GeoData")]
    public MpGeometricData_v1003 GeoData;

    [Key("Name")]
    public string Name;

    [Key("LinePen")]
    public MpDrawPen_v1003 LinePen;

    [Key("FillBrush")]
    public MpDrawBrush_v1003 FillBrush;

    [IgnoreMember]
    public CadFigure TempFigure = null;

    public static MpFigure_v1003 Create(SerializeContext sc, CadFigure fig, bool withChild = false)
    {
        MpFigure_v1003 ret = new MpFigure_v1003();

        ret.Store(sc, fig, withChild);

        return ret;
    }

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


    public virtual void ForEachFig(Action<MpFigure_v1003> d)
    {
        d(this);

        if (ChildList == null)
        {
            return;
        }

        int i;
        for (i = 0; i < ChildList.Count; i++)
        {
            MpFigure_v1003 c = ChildList[i];
            c.ForEachFig(d);
        }
    }

    public virtual void ForEachFigID(Dictionary<uint, MpFigure_v1003> allMap, Action<uint> d)
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
            MpFigure_v1003 childFig = allMap[id];
            childFig.ForEachFigID(allMap, d);
        }
    }

    public void StoreCommon(SerializeContext sc, CadFigure fig)
    {
        V = VERSION;

        ID = fig.ID;
        Type = (byte)fig.Type;
        Locked = fig.Locked;

        GeoData = fig.GeometricDataToMp_v1003(sc);

        Name = fig.Name;

        LinePen = MpDrawPen_v1003.Create(fig.LinePen);
        FillBrush = MpDrawBrush_v1003.Create(fig.FillBrush);
    }

    public void StoreChildIdList(CadFigure fig)
    {
        ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
    }

    public void StoreChildList(SerializeContext sc, CadFigure fig)
    {
        ChildList = MpUtil.FigureListToMp<MpFigure_v1003>(sc, fig.ChildList);
    }

    public void RestoreTo(DeserializeContext dsc, CadFigure fig)
    {
        fig.ID = ID;
        fig.Locked = Locked;


        if (ChildList != null)
        {
            fig.ChildList = MpUtil.FigureListFromMp<MpFigure_v1003>(dsc, ChildList);

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

        fig.GeometricDataFromMp_v1003(dsc, GeoData);


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
public struct MpVector3_v1003 : MpVector3
{
    [Key(0)]
    public vcompo_t X;

    [Key(1)]
    public vcompo_t Y;

    [Key(2)]
    public vcompo_t Z;

    public static MpVector3_v1003 Create(vector3_t v)
    {
        MpVector3_v1003 ret = new();
        ret.Store(v);

        return ret;
    }

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
public struct MpColor4_v1003
{
    [Key(0)]
    public float R;

    [Key(1)]
    public float G;

    [Key(2)]
    public float B;

    [Key(3)]
    public float A;

    public static MpColor4_v1003 Create(Color4 c)
    {
        MpColor4_v1003 ret = new MpColor4_v1003();
        ret.R = c.R;
        ret.G = c.G;
        ret.B = c.B;
        ret.A = c.A;

        return ret;
    }

    public Color4 Restore()
    {
        return new Color4(R, G, B, A);
    }
}

[MessagePackObject]
public struct MpDrawPen_v1003
{
    [Key("color")]
    public MpColor4_v1003 Color4;

    [Key("W")]
    public float Width;

    public static MpDrawPen_v1003 Create(DrawPen v)
    {
        return new MpDrawPen_v1003
        {
            Color4 = MpColor4_v1003.Create(v.Color4),
            Width = v.Width,
        };
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
public struct MpDrawBrush_v1003
{
    [Key("color")]
    public MpColor4_v1003 Color4;

    public static MpDrawBrush_v1003 Create(DrawBrush v)
    {
        return new MpDrawBrush_v1003
        {
            Color4 = MpColor4_v1003.Create(v.Color4),
        };
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
public struct MpVertexAttr_v1003
{
    [Key("flags")]
    public byte Flags;

    [Key("C1")]
    public MpColor4_v1003 Color1;

    [Key("C2")]
    public MpColor4_v1003 Color2;

    [Key("N")]
    public MpVector3_v1003 Normal;

    public static MpVertexAttr_v1003 Create(CadVertexAttr attr)
    {
        MpVertexAttr_v1003 ret = new MpVertexAttr_v1003();

        ret.Color1 = MpColor4_v1003.Create(attr.Color);
        ret.Normal =MpVector3_v1003.Create(attr.Normal);
        ret.Flags = 0;
        return ret;
    }

    public CadVertexAttr Restore()
    {
        CadVertexAttr attr = new CadVertexAttr();

        attr.Color = Color1.Restore();
        attr.Normal = Normal.Restore();
        return attr;
    }
}

[MessagePackObject]
public struct MpVertex_v1003 : MpVertex
{
    [Key("flag")]
    public byte Flag;

    [Key("P")]
    public MpVector3_v1003 P;

    [Key("Attr")]
    public MpVertexAttr_v1003 Attr;

    public MpVertex_v1003()
    {
    }

    public static MpVertex_v1003 Create(CadVertex v)
    {
        MpVertex_v1003 ret = new MpVertex_v1003();

        ret.Store(v);

        return ret;
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

[MessagePack.Union(0, typeof(MpSimpleGeometricData_v1003))]
[MessagePack.Union(1, typeof(MpMeshGeometricData_v1003))]
[MessagePack.Union(2, typeof(MpNurbsLineGeometricData_v1003))]
[MessagePack.Union(3, typeof(MpNurbsSurfaceGeometricData_v1003))]
[MessagePack.Union(4, typeof(MpPictureGeometricData_v1003))]
[MessagePack.Union(5, typeof(MpPolyLinesGeometricData_v1003))]
public interface MpGeometricData_v1003
{
}

#region "GeometricData"

[MessagePackObject]
public class MpSimpleGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("PointList")]
    public List<MpVertex_v1003> PointList;
}

[MessagePackObject]
public class MpPolyLinesGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("IsLoop")]
    public bool IsLoop;


    [Key("PointList")]
    public List<MpVertex_v1003> PointList;
}


[MessagePackObject]
public class MpPictureGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("FilePathName")]
    public string FilePathName;

    [Key("PointList")]
    public List<MpVertex_v1003> PointList;

    [Key("Bytes")]
    public Byte[] Bytes = null;

    [Key("base64")]
    public string Base64 = null;
}

// CadFigureMesh    
[MessagePackObject]
public class MpMeshGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("HeModel")]
    public MpHeModel_v1003 HeModel;
}

// CadFigureNurbsLine
[MessagePackObject]
public class MpNurbsLineGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("Nurbs")]
    public MpNurbsLine_v1003 Nurbs;
}

// CadFigureNurbsSurface
[MessagePackObject]
public class MpNurbsSurfaceGeometricData_v1003 : MpGeometricData_v1003
{
    [Key("Nurbs")]
    public MpNurbsSurface_v1003 Nurbs;
}
#endregion


[MessagePackObject]
public class MpHeModel_v1003
{
    [Key("VertexStore")]
    public List<MpVertex_v1003> VertexStore;

    [Key("NormalStore")]
    public List<MpVector3_v1003> NormalStore;

    [Key("FaceStore")]
    public List<MpHeFace_v1003> FaceStore;

    [Key("HalfEdgeList")]
    public List<MpHalfEdge_v1003> HalfEdgeList;

    [Key("HeIdCnt")]
    public uint HeIdCount;

    [Key("FaceIdCnt")]
    public uint FaceIdCount;


    public static MpHeModel_v1003 Create(HeModel model)
    {
        MpHeModel_v1003 ret = new MpHeModel_v1003();

        ret.VertexStore = MpUtil.VertexListToMp<MpVertex_v1003>(model.VertexStore);

        ret.NormalStore = MpUtil.Vector3ListToMp<MpVector3_v1003>(model.NormalStore);

        ret.FaceStore = MpUtil.HeFaceListToMp<MpHeFace_v1003>(model.FaceStore);

        ret.HeIdCount = model.HeIdProvider.Counter;

        ret.FaceIdCount = model.FaceIdProvider.Counter;

        List<HalfEdge> heList = model.GetHalfEdgeList();

        ret.HalfEdgeList = MpUtil.HalfEdgeListToMp<MpHalfEdge_v1003>(heList);

        return ret;
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
public class MpHeFace_v1003 : MpHeFace
{
    [Key("ID")]
    public uint ID;

    [Key("HeadID")]
    public uint HeadID;

    [Key("Normal")]
    public int Normal = HeModel.INVALID_INDEX;

    public static MpHeFace_v1003 Create(HeFace face)
    {
        MpHeFace_v1003 ret = new MpHeFace_v1003();
        ret.Store(face);

        return ret;
    }

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
public class MpHalfEdge_v1003 : MpHalfEdge
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


    public static MpHalfEdge_v1003 Create(HalfEdge he)
    {
        MpHalfEdge_v1003 ret = new MpHalfEdge_v1003();
        ret.Store(he);

        return ret;
    }

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
public class MpNurbsLine_v1003
{
    [Key("CtrlCnt")]
    public int CtrlCnt;

    [Key("CtrlDataCnt")]
    public int CtrlDataCnt;

    [Key("Weights")]
    public vcompo_t[] Weights;

    [Key("CtrlPoints")]
    public List<MpVertex_v1003> CtrlPoints;

    [Key("CtrlOrder")]
    public int[] CtrlOrder;

    [Key("BSplineParam")]
    public MpBSplineParam_v1003 BSplineP;

    public static MpNurbsLine_v1003 Create(NurbsLine src)
    {
        MpNurbsLine_v1003 ret = new MpNurbsLine_v1003();

        ret.CtrlCnt = src.CtrlCnt;
        ret.CtrlDataCnt = src.CtrlDataCnt;
        ret.Weights = MpUtil.ArrayClone<vcompo_t>(src.Weights);
        ret.CtrlPoints = MpUtil.VertexListToMp<MpVertex_v1003>(src.CtrlPoints);
        ret.CtrlOrder = MpUtil.ArrayClone<int>(src.CtrlOrder);

        ret.BSplineP = MpBSplineParam_v1003.Create(src.BSplineP);

        return ret;
    }

    public NurbsLine Restore()
    {
        NurbsLine nurbs = new NurbsLine();

        nurbs.CtrlCnt = CtrlCnt;
        nurbs.CtrlDataCnt = CtrlDataCnt;
        nurbs.Weights = MpUtil.ArrayClone<vcompo_t>(Weights);
        nurbs.CtrlPoints = MpUtil.VertexListFromMp(CtrlPoints);
        nurbs.CtrlOrder = MpUtil.ArrayClone<int>(CtrlOrder);

        nurbs.BSplineP = BSplineP.Restore();

        return nurbs;
    }
}

[MessagePackObject]
public class MpNurbsSurface_v1003
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
    public List<MpVertex_v1003> CtrlPoints;

    [Key("CtrlOrder")]
    public int[] CtrlOrder;

    [Key("UBSpline")]
    public MpBSplineParam_v1003 UBSpline;

    [Key("VBSpline")]
    public MpBSplineParam_v1003 VBSpline;

    public static MpNurbsSurface_v1003 Create(NurbsSurface src)
    {
        MpNurbsSurface_v1003 ret = new MpNurbsSurface_v1003();

        ret.UCtrlCnt = src.UCtrlCnt;
        ret.VCtrlCnt = src.VCtrlCnt;

        ret.UCtrlDataCnt = src.UCtrlDataCnt;
        ret.VCtrlDataCnt = src.VCtrlDataCnt;

        ret.CtrlPoints = MpUtil.VertexListToMp<MpVertex_v1003>(src.CtrlPoints);

        ret.Weights = MpUtil.ArrayClone<vcompo_t>(src.Weights);
        ret.CtrlOrder = MpUtil.ArrayClone<int>(src.CtrlOrder);

        ret.UBSpline = MpBSplineParam_v1003.Create(src.UBSpline);
        ret.VBSpline = MpBSplineParam_v1003.Create(src.VBSpline);

        return ret;
    }

    public NurbsSurface Restore()
    {
        NurbsSurface nurbs = new NurbsSurface();

        nurbs.UCtrlCnt = UCtrlCnt;
        nurbs.VCtrlCnt = VCtrlCnt;

        nurbs.UCtrlDataCnt = UCtrlDataCnt;
        nurbs.VCtrlDataCnt = VCtrlDataCnt;

        nurbs.CtrlPoints = MpUtil.VertexListFromMp(CtrlPoints);

        nurbs.Weights = MpUtil.ArrayClone<vcompo_t>(Weights);
        nurbs.CtrlOrder = MpUtil.ArrayClone<int>(CtrlOrder);

        nurbs.UBSpline = UBSpline.Restore();
        nurbs.VBSpline = VBSpline.Restore();

        return nurbs;
    }
}


[MessagePackObject]
public class MpBSplineParam_v1003
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

    public static MpBSplineParam_v1003 Create(BSplineParam src)
    {
        MpBSplineParam_v1003 ret = new MpBSplineParam_v1003();

        ret.Degree = src.Degree;
        ret.DivCnt = src.DivCnt;
        ret.OutputCnt = src.OutputCnt;
        ret.KnotCnt = src.KnotCnt;
        ret.Knots = MpUtil.ArrayClone<vcompo_t>(src.Knots);
        ret.LowKnot = src.LowKnot;
        ret.HighKnot = src.HighKnot;
        ret.Step = src.Step;

        return ret;
    }

    public BSplineParam Restore()
    {
        BSplineParam bs = new BSplineParam();

        bs.Degree = Degree;
        bs.DivCnt = DivCnt;
        bs.OutputCnt = OutputCnt;
        bs.KnotCnt = KnotCnt;
        bs.Knots = MpUtil.ArrayClone<vcompo_t>(Knots);
        bs.LowKnot = LowKnot;
        bs.HighKnot = HighKnot;
        bs.Step = Step;

        return bs;
    }
}
