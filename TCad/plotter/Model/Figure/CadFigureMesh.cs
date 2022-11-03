using CadDataTypes;
using HalfEdgeNS;
using MyCollections;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Serializer.v1001;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
using Plotter.Settings;
using System;
using System.Collections.Generic;

namespace Plotter;

public class CadFigureMesh : CadFigure
{
    public HeModel mHeModel;

    public static double EDGE_THRESHOLD;

    private FlexArray<IndexPair> SegList = new FlexArray<IndexPair>();


    static CadFigureMesh()
    {
        EDGE_THRESHOLD = Math.Cos(CadMath.Deg2Rad(30));
    }

    public override VertexList PointList => mPointList;

    public override int PointCount => PointList.Count;

    public CadFigureMesh()
    {
        Type = Types.MESH;

        mHeModel = new HeModel();

        mPointList = mHeModel.VertexStore;
    }

    public void SetMesh(HeModel mesh)
    {
        mHeModel = mesh;
        mPointList = mHeModel.VertexStore;

        UpdateSegList();
    }

    public void CreateModel(CadFigure fig)
    {
        if (!(fig is CadFigurePolyLines))
        {
            return;
        }

        mHeModel.Clear();

        for (int i = 0; i < fig.PointCount; i++)
        {
            int idx = mHeModel.AddVertex(fig.PointList[i]);
        }

        List<CadFigure> figList = TriangleSplitter.Split(fig, 16);

        HeModelBuilder mb = new HeModelBuilder();

        mb.Start(mHeModel);

        for (int i = 0; i < figList.Count; i++)
        {
            CadFigure t = figList[i];
            mb.AddTriangle(t.PointList[0], t.PointList[1], t.PointList[2]);
        }
    }

    public override void EndEdit()
    {
        base.EndEdit();
        mHeModel.RecreateNormals();
    }

    public override CadSegment GetSegmentAt(int n)
    {
        CadSegment seg = default;
        seg.P0 = mPointList[SegList[n].Idx0];
        seg.P1 = mPointList[SegList[n].Idx1];

        return seg;
    }

    public override FigureSegment GetFigSegmentAt(int n)
    {
        FigureSegment seg = new FigureSegment(this, n, SegList[n].Idx0, SegList[n].Idx1);
        return seg;
    }

    public override int SegmentCount
    {
        get
        {
            return SegList.Count;
        }
    }

    private void UpdateSegList()
    {
        SegList.Clear();

        for (int i = 0; i < mHeModel.FaceStore.Count; i++)
        {
            HeFace f = mHeModel.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;


            for (; ; )
            {
                HalfEdge next = c.Next;

                SegList.Add(new IndexPair(c.Vertex, next.Vertex));

                c = next;

                if (c == head)
                {
                    break;
                }
            }
        }
    }

    public override void Draw(DrawContext dc, DrawOption opt)
    {
        DrawBrush brush;
        DrawPen borderPen;
        DrawPen edgePen;

        if (!opt.ForceMeshBrush && (!FillBrush.IsInvalid))
        {
            brush = FillBrush;
        }
        else
        {
            brush = opt.MeshBrush;
        }

        if (!opt.ForceMeshPen && (!LinePen.IsInvalid))
        {
            edgePen = LinePen;
            if (edgePen.IsNull)
            {
                borderPen = edgePen;
            }
            else
            {
                borderPen = new DrawPen(ColorUtil.Mix(LinePen.Color4, brush.Color4, 0.2f), LinePen.Width);
            }
        }
        else
        {
            edgePen = opt.MeshEdgePen;
            if (edgePen.IsInvalid)
            {
                borderPen = DrawPen.InvalidPen;
            }
            else
            {
                borderPen = new DrawPen(ColorUtil.Mix(edgePen.Color4, brush.Color4, 0.2f), edgePen.Width); ;
            }
        }

        dc.Drawing.DrawHarfEdgeModel(
            brush,
            borderPen,
            edgePen,
            EDGE_THRESHOLD,
            mHeModel);
    }

    public override void DrawSelected(DrawContext dc, DrawOption dp)
    {
        dc.Drawing.DrawSelectedPoints(PointList, dc.GetPen(DrawTools.PEN_SELECTED_POINT));
    }

    public override void SelectPointAt(int index, bool sel)
    {
        CadVertex p = mPointList[index];
        p.Selected = sel;
        mPointList[index] = p;
    }

    public override Centroid GetCentroid()
    {
        Centroid cent = default;
        Centroid ct = default;

        for (int i = 0; i < mHeModel.FaceStore.Count; i++)
        {
            HeFace f = mHeModel.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge he = head;

            int i0 = he.Vertex;
            int i1 = he.Next.Vertex;
            int i2 = he.Next.Next.Vertex;

            ct.set(
                mHeModel.VertexStore[i0].vector,
                mHeModel.VertexStore[i1].vector,
                mHeModel.VertexStore[i2].vector
                );

            cent = cent.Merge(ct);
        }

        return cent;
    }

    public override void InvertDir()
    {
        mHeModel.InvertAllFace();
    }


    public override void RemoveSelected()
    {
        List<int> removeList = new List<int>();

        #region Improved performance of deleting large objects
        int cnt = 0;
        mPointList.ForEach((p) =>
        {
            if (p.Selected) cnt++;
        });

        if (cnt == mPointList.Count)
        {
            mHeModel.Clear();
            return;
        }
        #endregion


        for (int i = 0; i < mPointList.Count; i++)
        {
            if (mPointList[i].Selected)
            {
                mHeModel.RemoveVertexRelationFace(i);
                removeList.Add(i);
            }
        }

        if (mHeModel.FaceStore.Count == 0)
        {
            mHeModel.Clear();
            return;
        }

        mHeModel.RemoveVertexs(removeList);
    }

    public override void FlipWithPlane(Vector3d p0, Vector3d normal)
    {
        VertexList vl = PointList;

        for (int i = 0; i < vl.Count; i++)
        {
            CadVertex v = vl[i];

            Vector3d cp = CadMath.CrossPlane(v.vector, p0, normal);

            CadVertex d = v - cp;

            v = cp - d;

            vl[i] = v;
        }

        mHeModel.InvertAllFace();
        mHeModel.RecreateNormals();

        //Vector3dList nl = mHeModel.NormalStore;

        //for (int i = 0; i < nl.Count; i++)
        //{
        //    Vector3d v = nl[i];

        //    Vector3d cp = CadMath.CrossPlane(v, Vector3d.Zero, normal);

        //    Vector3d d = v - cp;

        //    v = cp - d;

        //    nl[i] = v;
        //}
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
    }

    public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
    {
    }

    public override void StartCreate(DrawContext dc)
    {
    }

    public override void EndCreate(DrawContext dc)
    {
    }


    #region Serialize

    public override MpGeometricData_v1002 GeometricDataToMp_v1002()
    {
        MpMeshGeometricData_v1002 mpGeo = new MpMeshGeometricData_v1002();
        mpGeo.HeModel = MpHeModel_v1002.Create(mHeModel);

        return mpGeo;
    }

    public override void GeometricDataFromMp_v1002(MpGeometricData_v1002 mpGeo)
    {
        if (!(mpGeo is MpMeshGeometricData_v1002))
        {
            return;
        }

        MpMeshGeometricData_v1002 meshGeo = (MpMeshGeometricData_v1002)mpGeo;

        //mHeModel = meshGeo.HeModel.Restore();
        //mPointList = mHeModel.VertexStore;
        SetMesh(meshGeo.HeModel.Restore());
    }

    public override MpGeometricData_v1003 GeometricDataToMp_v1003()
    {
        MpMeshGeometricData_v1003 mpGeo = new MpMeshGeometricData_v1003();
        mpGeo.HeModel = MpHeModel_v1003.Create(mHeModel);

        return mpGeo;
    }

    public override void GeometricDataFromMp_v1003(MpGeometricData_v1003 mpGeo)
    {
        if (!(mpGeo is MpMeshGeometricData_v1003))
        {
            return;
        }

        MpMeshGeometricData_v1003 meshGeo = (MpMeshGeometricData_v1003)mpGeo;

        //mHeModel = meshGeo.HeModel.Restore();
        //mPointList = mHeModel.VertexStore;
        SetMesh(meshGeo.HeModel.Restore());
    }
    #endregion
}
