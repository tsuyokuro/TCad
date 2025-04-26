using CadDataTypes;
using MyCollections;
using Plotter;
using System;
using System.Collections.Generic;
using TCad.Plotter.Model.HalfEdgeModel;

namespace TCad.Plotter.Model.Figure;

public partial class CadFigureMesh : CadFigure
{
    public HeModel mHeModel;

    public static vcompo_t EDGE_THRESHOLD;

    private FlexArray<IndexPair> SegList = new FlexArray<IndexPair>();


    static CadFigureMesh()
    {
        EDGE_THRESHOLD = (vcompo_t)Math.Cos(CadMath.Deg2Rad((vcompo_t)(0.5)));
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

        List<Vector3List> trList = TriangleSplitter.Split(fig, 16);

        HeModelBuilder mb = new HeModelBuilder();

        mb.Start(mHeModel);

        CadVertex v0 = new();
        CadVertex v1 = new();
        CadVertex v2 = new();


        for (int i = 0; i < trList.Count; i++)
        {
            Vector3List t = trList[i];
            v0.vector = t[0];
            v1.vector = t[1];
            v2.vector = t[2];


            mb.AddTriangle(v0, v1, v2);
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
                if (opt.DrawMeshBorder)
                {
                    borderPen = new DrawPen(ColorUtil.Mix(LinePen.Color4, brush.Color4, 0.2f), LinePen.Width);
                }
                else
                {
                    borderPen = DrawPen.InvalidPen;
                }
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
                if (opt.DrawMeshBorder)
                {
                    borderPen = new DrawPen(ColorUtil.Mix(edgePen.Color4, brush.Color4, 0.2f), edgePen.Width); ;
                }
                else
                {
                    borderPen = DrawPen.InvalidPen;
                }
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

    public override void FlipWithPlane(vector3_t p0, vector3_t normal)
    {
        VertexList vl = PointList;

        for (int i = 0; i < vl.Count; i++)
        {
            CadVertex v = vl[i];

            vector3_t cp = CadMath.CrossPlane(v.vector, p0, normal);

            CadVertex d = v - cp;

            v = cp - d;

            vl[i] = v;
        }

        mHeModel.InvertAllFace();
        mHeModel.RecreateNormals();

        //Vector3List nl = mHeModel.NormalStore;

        //for (int i = 0; i < nl.Count; i++)
        //{
        //    vector3_t v = nl[i];

        //    vector3_t cp = CadMath.CrossPlane(v, vector3_t.Zero, normal);

        //    vector3_t d = v - cp;

        //    v = cp - d;

        //    nl[i] = v;
        //}
    }

    public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
    {
        Log.tpl($"idxA:{idxA} idxB:{idxB}");

        CadVertex a = PointList[idxA];
        CadVertex b = PointList[idxB];

        vcompo_t shift = dc.DevSizeToWoldSize((vcompo_t)(0.11));
        vector3_t sv = -dc.ViewDir * shift;

        dc.Drawing.DrawLine(pen, a.vector + sv, b.vector + sv);
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
}
