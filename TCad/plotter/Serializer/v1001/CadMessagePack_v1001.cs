using HalfEdgeNS;
using MessagePack;
using System;
using System.Collections.Generic;
using CadDataTypes;
using SplineCurve;
using System.Drawing.Printing;
using OpenTK;

namespace Plotter.Serializer.v1001
{
    [MessagePackObject]
    public class MpCadData_v1001
    {
        [Key("DB")]
        public MpCadObjectDB_v1001 MpDB;

        [Key("ViewInfo")]
        public MpViewInfo_v1001 ViewInfo;

        [IgnoreMember]
        CadObjectDB DB = null;

        public static MpCadData_v1001 Create(CadObjectDB db)
        {
            MpCadData_v1001 ret = new MpCadData_v1001();

            ret.MpDB = MpCadObjectDB_v1001.Create(db);

            ret.ViewInfo = new MpViewInfo_v1001();

            return ret;
        }

        public CadObjectDB GetDB()
        {
            if (DB == null)
            {
                DB = MpDB.Restore();
            }

            return DB;
        }
    }

    [MessagePackObject]
    public class MpViewInfo_v1001
    {
        [Key("WorldScale")]
        public double WorldScale = 1.0;

        [Key("Paper")]
        public MpPaperSettings_v1001 PaperSettings = new MpPaperSettings_v1001();
    }

    [MessagePackObject]
    public class MpPaperSettings_v1001
    {
        [Key("W")]
        public double Width = 210.0;

        [Key("H")]
        public double Height = 297.0;

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
    public class MpCadObjectDB_v1001
    {
        [Key("LayerIdCnt")]
        public uint LayerIdCount;

        [Key("FigIdCnt")]
        public uint FigureIdCount;

        [Key("FigList")]
        public List<MpFigure_v1001> FigureList;

        [Key("LayerList")]
        public List<MpLayer_v1001> LayerList;

        [Key("CurrentLayerID")]
        public uint CurrentLayerID;

        public static MpCadObjectDB_v1001 Create(CadObjectDB db)
        {
            MpCadObjectDB_v1001 ret = new MpCadObjectDB_v1001();

            ret.LayerIdCount = db.LayerIdProvider.Counter;
            ret.FigureIdCount = db.FigIdProvider.Counter;

            ret.FigureList = MpUtil_v1001.FigureMapToMp_v1001(db.FigureMap);

            ret.LayerList = MpUtil_v1001.LayerListToMp(db.LayerList);

            ret.CurrentLayerID = db.CurrentLayerID;

            return ret;
        }

        public void GarbageCollect()
        {
            var idMap = new Dictionary<uint, MpFigure_v1001>();

            foreach (MpFigure_v1001 fig in FigureList)
            {
                idMap.Add(fig.ID, fig);
            }

            var activeSet = new HashSet<uint>();

            foreach (MpLayer_v1001 layer in LayerList)
            {
                foreach (uint id in layer.FigureIdList)
                {
                    MpFigure_v1001 fig = idMap[id];

                    fig.ForEachFigID(idMap, (a) =>
                    {
                        activeSet.Add(a);
                    });
                }
            }

            int i = FigureList.Count - 1;

            for (; i >= 0; i--)
            {
                MpFigure_v1001 fig = FigureList[i];

                if (!activeSet.Contains(fig.ID))
                {
                    FigureList.RemoveAt(i);
                }
            }
        }

        public CadObjectDB Restore()
        {
            CadObjectDB ret = new CadObjectDB();

            ret.LayerIdProvider.Counter = LayerIdCount;
            ret.FigIdProvider.Counter = FigureIdCount;

            // Figure map
            List<CadFigure> figList = MpUtil_v1001.FigureListFromMp_v1001(FigureList);

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
                MpFigure_v1001 mpfig = FigureList[i];
                SetFigChild(mpfig, dic);
            }


            // Layer map
            ret.LayerList = MpUtil_v1001.LayerListFromMp(LayerList, dic);

            ret.LayerMap = new Dictionary<uint, CadLayer>();

            for (int i = 0; i < ret.LayerList.Count; i++)
            {
                CadLayer layer = ret.LayerList[i];

                ret.LayerMap.Add(layer.ID, layer);
            }

            ret.CurrentLayerID = CurrentLayerID;

            return ret;
        }

        private void SetFigChild(MpFigure_v1001 mpfig, Dictionary<uint, CadFigure> dic)
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
    public class MpLayer_v1001
    {
        [Key("ID")]
        public uint ID;

        [Key("Visible")]
        public bool Visible;

        [Key("Locked")]
        public bool Locked;

        [Key("FigIdList")]
        public List<uint> FigureIdList;

        public static MpLayer_v1001 Create(CadLayer layer)
        {
            MpLayer_v1001 ret = new MpLayer_v1001();

            ret.ID = layer.ID;
            ret.Visible = layer.Visible;
            ret.Locked = layer.Locked;

            ret.FigureIdList = MpUtil_v1001.FigureListToIdList(layer.FigureList);

            return ret;
        }

        public CadLayer Restore(Dictionary<uint, CadFigure> dic)
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
    public class MpFigure_v1001
    {
        [Key("ID")]
        public uint ID;

        [Key("Type")]
        public byte Type;

        [Key("Locked")]
        public bool Locked;

        [Key("IsLoop")]
        public bool IsLoop;

        [Key("Normal")]
        public MpVector3d_v1001 Normal;

        [Key("ChildList")]
        public List<MpFigure_v1001> ChildList;

        [Key("ChildIdList")]
        public List<uint> ChildIdList;

        [Key("GeoData")]
        public MpGeometricData_v1001 GeoData;

        [IgnoreMember]
        public CadFigure TempFigure = null;

        public static MpFigure_v1001 Create(CadFigure fig, bool withChild = false)
        {
            MpFigure_v1001 ret = new MpFigure_v1001();

            ret.StoreCommon(fig);

            if (withChild)
            {
                ret.StoreChildList(fig);
            }
            else
            {
                ret.StoreChildIdList(fig);
            }
            return ret;
        }

        public virtual void ForEachFig(Action<MpFigure_v1001> d)
        {
            d(this);

            if (ChildList == null)
            {
                return;
            }

            int i;
            for (i = 0; i < ChildList.Count; i++)
            {
                MpFigure_v1001 c = ChildList[i];
                c.ForEachFig(d);
            }
        }

        public virtual void ForEachFigID(Dictionary<uint, MpFigure_v1001> allMap, Action<uint> d)
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
                MpFigure_v1001 childFig = allMap[id];
                childFig.ForEachFigID(allMap, d);
            }
        }

        public void StoreCommon(CadFigure fig)
        {
            ID = fig.ID;
            Type = (byte)fig.Type;
            Locked = fig.Locked;
            IsLoop = fig.IsLoop;
            Normal = MpVector3d_v1001.Create(fig.Normal);

            GeoData = fig.GeometricDataToMp_v1001();
        }

        public void StoreChildIdList(CadFigure fig)
        {
            ChildIdList = MpUtil_v1001.FigureListToIdList(fig.ChildList);
        }

        public void StoreChildList(CadFigure fig)
        {
            ChildList = MpUtil_v1001.FigureListToMp_v1001(fig.ChildList);
        }

        public void RestoreTo(CadFigure fig)
        {
            fig.ID = ID;
            fig.Locked = Locked;
            fig.IsLoop = IsLoop;
            fig.Normal = Normal.Restore();

            if (ChildList != null)
            {
                fig.ChildList = MpUtil_v1001.FigureListFromMp_v1001(ChildList);

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

            fig.GeometricDataFromMp_v1001(GeoData);
        }

        public CadFigure Restore()
        {
            CadFigure fig = CadFigure.Create((CadFigure.Types)Type);

            RestoreTo(fig);

            return fig;
        }
    }

    [MessagePackObject]
    public struct MpVertex_v1001
    {
        [Key(0)]
        public byte Flag;

        [Key(1)]
        public double x;

        [Key(2)]
        public double y;

        [Key(3)]
        public double z;

        public static MpVertex_v1001 Create(CadVertex v)
        {
            MpVertex_v1001 ret = new MpVertex_v1001();
            ret.Flag = v.Flag;
            ret.x = v.X;
            ret.y = v.Y;
            ret.z = v.Z;
            return ret;
        }

        public CadVertex Restore()
        {
            CadVertex v = CadVertex.Create(x, y, z);
            v.Flag = Flag;

            return v;
        }
    }

    [MessagePackObject]
    public struct MpVector3d_v1001
    {
        [Key(0)]
        public double X;

        [Key(1)]
        public double Y;

        [Key(2)]
        public double Z;

        public static MpVector3d_v1001 Create(Vector3d v)
        {
            MpVector3d_v1001 ret = new MpVector3d_v1001();

            ret.X = v.X;
            ret.Y = v.Y;
            ret.Z = v.Z;

            return ret;
        }

        public Vector3d Restore()
        {
            return new Vector3d(X, Y, Z);
        }
    }

    [MessagePack.Union(0, typeof(MpSimpleGeometricData_v1001))]
    [MessagePack.Union(1, typeof(MpMeshGeometricData_v1001))]
    [MessagePack.Union(2, typeof(MpNurbsLineGeometricData_v1001))]
    [MessagePack.Union(3, typeof(MpNurbsSurfaceGeometricData_v1001))]
    public interface MpGeometricData_v1001
    {
    }

    [MessagePackObject]
    public class MpSimpleGeometricData_v1001 : MpGeometricData_v1001
    {
        [Key("PointList")]
        public List<MpVertex_v1001> PointList;
    }

    [MessagePackObject]
    public class MpMeshGeometricData_v1001 : MpGeometricData_v1001
    {
        [Key("HeModel")]
        public MpHeModel_v1001 HeModel;
    }

    [MessagePackObject]
    public class MpNurbsLineGeometricData_v1001 : MpGeometricData_v1001
    {
        [Key("Nurbs")]
        public MpNurbsLine_v1001 Nurbs;
    }

    [MessagePackObject]
    public class MpNurbsSurfaceGeometricData_v1001 : MpGeometricData_v1001
    {
        [Key("Nurbs")]
        public MpNurbsSurface_v1001 Nurbs;
    }


    [MessagePackObject]
    public class MpHeModel_v1001
    {
        [Key("VertexStore")]
        public List<MpVertex_v1001> VertexStore;

        [Key("NormalStore")]
        public List<MpVector3d_v1001> NormalStore;

        [Key("FaceStore")]
        public List<MpHeFace_v1001> FaceStore;

        [Key("HalfEdgeList")]
        public List<MpHalfEdge_v1001> HalfEdgeList;

        [Key("HeIdCnt")]
        public uint HeIdCount;

        [Key("FaceIdCnt")]
        public uint FaceIdCount;


        public static MpHeModel_v1001 Create(HeModel model)
        {
            MpHeModel_v1001 ret = new MpHeModel_v1001();

            ret.VertexStore = MpUtil_v1001.VertexListToMp(model.VertexStore);

            ret.NormalStore = MpUtil_v1001.Vector3dListToMp(model.NormalStore);

            ret.FaceStore = MpUtil_v1001.HeFaceListToMp(model.FaceStore);

            ret.HeIdCount = model.HeIdProvider.Counter;

            ret.FaceIdCount = model.FaceIdProvider.Counter;

            List<HalfEdge> heList = model.GetHalfEdgeList();

            ret.HalfEdgeList = MpUtil_v1001.HalfEdgeListToMp(heList);

            return ret;
        }

        public HeModel Restore()
        {
            HeModel ret = new HeModel();

            ret.VertexStore = MpUtil_v1001.VertexListFromMp(VertexStore);

            ret.NormalStore = MpUtil_v1001.Vector3dListFromMp(NormalStore);

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

            ret.FaceStore = MpUtil_v1001.HeFaceListFromMp(FaceStore, dic);

            ret.HeIdProvider.Counter = HeIdCount;

            ret.FaceIdProvider.Counter = FaceIdCount;

            return ret;
        }
    }

    [MessagePackObject]
    public class MpHeFace_v1001
    {
        [Key("ID")]
        public uint ID;

        [Key("HeadID")]
        public uint HeadID;

        [Key("Normal")]
        public int Normal = HeModel.INVALID_INDEX;

        public static MpHeFace_v1001 Create(HeFace face)
        {
            MpHeFace_v1001 ret = new MpHeFace_v1001();
            ret.ID = face.ID;
            ret.HeadID = face.Head.ID;
            ret.Normal = face.Normal;

            return ret;
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
    public class MpHalfEdge_v1001
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


        public static MpHalfEdge_v1001 Create(HalfEdge he)
        {
            MpHalfEdge_v1001 ret = new MpHalfEdge_v1001();

            ret.ID = he.ID;
            ret.PairID = he.Pair != null ? he.Pair.ID : 0;
            ret.NextID = he.Next != null ? he.Next.ID : 0;
            ret.PrevID = he.Prev != null ? he.Prev.ID : 0;

            ret.Vertex = he.Vertex;
            ret.Face = he.Face;
            ret.Normal = he.Normal;

            return ret;
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
    public class MpNurbsLine_v1001
    {
        [Key("CtrlCnt")]
        public int CtrlCnt;

        [Key("CtrlDataCnt")]
        public int CtrlDataCnt;

        [Key("Weights")]
        public double[] Weights;

        [Key("CtrlPoints")]
        public List<MpVertex_v1001> CtrlPoints;

        [Key("CtrlOrder")]
        public int[] CtrlOrder;

        [Key("BSplineParam")]
        public MpBSplineParam_v1001 BSplineP;

        public static MpNurbsLine_v1001 Create(NurbsLine src)
        {
            MpNurbsLine_v1001 ret = new MpNurbsLine_v1001();

            ret.CtrlCnt = src.CtrlCnt;
            ret.CtrlDataCnt = src.CtrlDataCnt;
            ret.Weights = MpUtil_v1001.ArrayClone<double>(src.Weights);
            ret.CtrlPoints = MpUtil_v1001.VertexListToMp(src.CtrlPoints);
            ret.CtrlOrder = MpUtil_v1001.ArrayClone<int>(src.CtrlOrder);

            ret.BSplineP = MpBSplineParam_v1001.Create(src.BSplineP);

            return ret;
        }

        public NurbsLine Restore()
        {
            NurbsLine nurbs = new NurbsLine();

            nurbs.CtrlCnt = CtrlCnt;
            nurbs.CtrlDataCnt = CtrlDataCnt;
            nurbs.Weights = MpUtil_v1001.ArrayClone<double>(Weights);
            nurbs.CtrlPoints = MpUtil_v1001.VertexListFromMp(CtrlPoints);
            nurbs.CtrlOrder = MpUtil_v1001.ArrayClone<int>(CtrlOrder);

            nurbs.BSplineP = BSplineP.Restore();

            return nurbs;
        }
    }

    [MessagePackObject]
    public class MpNurbsSurface_v1001
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
        public double[] Weights;

        [Key("CtrlPoints")]
        public List<MpVertex_v1001> CtrlPoints;

        [Key("CtrlOrder")]
        public int[] CtrlOrder;

        [Key("UBSpline")]
        public MpBSplineParam_v1001 UBSpline;

        [Key("VBSpline")]
        public MpBSplineParam_v1001 VBSpline;

        public static MpNurbsSurface_v1001 Create(NurbsSurface src)
        {
            MpNurbsSurface_v1001 ret = new MpNurbsSurface_v1001();

            ret.UCtrlCnt = src.UCtrlCnt;
            ret.VCtrlCnt = src.VCtrlCnt;

            ret.UCtrlDataCnt = src.UCtrlDataCnt;
            ret.VCtrlDataCnt = src.VCtrlDataCnt;

            ret.CtrlPoints = MpUtil_v1001.VertexListToMp(src.CtrlPoints);

            ret.Weights = MpUtil_v1001.ArrayClone<double>(src.Weights);
            ret.CtrlOrder = MpUtil_v1001.ArrayClone<int>(src.CtrlOrder);

            ret.UBSpline = MpBSplineParam_v1001.Create(src.UBSpline);
            ret.VBSpline = MpBSplineParam_v1001.Create(src.VBSpline);

            return ret;
        }

        public NurbsSurface Restore()
        {
            NurbsSurface nurbs = new NurbsSurface();

            nurbs.UCtrlCnt = UCtrlCnt;
            nurbs.VCtrlCnt = VCtrlCnt;

            nurbs.UCtrlDataCnt = UCtrlDataCnt;
            nurbs.VCtrlDataCnt = VCtrlDataCnt;

            nurbs.CtrlPoints = MpUtil_v1001.VertexListFromMp(CtrlPoints);

            nurbs.Weights = MpUtil_v1001.ArrayClone<double>(Weights);
            nurbs.CtrlOrder = MpUtil_v1001.ArrayClone<int>(CtrlOrder);

            nurbs.UBSpline = UBSpline.Restore();
            nurbs.VBSpline = VBSpline.Restore();

            return nurbs;
        }
    }


    [MessagePackObject]
    public class MpBSplineParam_v1001
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
        public double[] Knots;

        [Key("CtrlCnt")]
        public int CtrlCnt;

        [Key("LowKnot")]
        public double LowKnot = 0;

        [Key("HightKnot")]
        public double HighKnot = 0;

        [Key("Step")]
        public double Step = 0;

        public static MpBSplineParam_v1001 Create(BSplineParam src)
        {
            MpBSplineParam_v1001 ret = new MpBSplineParam_v1001();

            ret.Degree = src.Degree;
            ret.DivCnt = src.DivCnt;
            ret.OutputCnt = src.OutputCnt;
            ret.KnotCnt = src.KnotCnt;
            ret.Knots = MpUtil_v1001.ArrayClone<double>(src.Knots);
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
            bs.Knots = MpUtil_v1001.ArrayClone<double>(Knots);
            bs.LowKnot = LowKnot;
            bs.HighKnot = HighKnot;
            bs.Step = Step;

            return bs;
        }
    }
}