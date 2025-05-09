//#define LOG_DEBUG

using CadDataTypes;
using TCad.Plotter;
using System;
using System.Collections.Generic;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.DrawToolSet;

namespace TCad.Plotter.Model.Figure;

public abstract partial class CadFigure
{
    #region Enums
    public enum Types : byte
    {
        NONE,
        LINE,
        RECT,
        POLY_LINES,
        CIRCLE,
        POINT,
        GROUP,
        DIMENTION_LINE,
        MESH,
        NURBS_LINE,
        NURBS_SURFACE,
        PICTURE,
        MAX,
    }
    #endregion


    private static Dictionary<CadFigure.Types, string> TypeNames;

    static CadFigure()
    {
        TypeNames = new Dictionary<CadFigure.Types, string>()
        {
            { Types.NONE, "NONE" },
            { Types.LINE, "LINE" },
            { Types.RECT, "RECT" },
            { Types.POLY_LINES, "LINES" },
            { Types.CIRCLE, "CIRCLE" },
            { Types.POINT, "POINT" },
            { Types.GROUP, "GROUP" },
            { Types.DIMENTION_LINE, "DIM" },
            { Types.MESH, "MESH" },
            { Types.NURBS_LINE, "NURBS-LINE" },
            { Types.NURBS_SURFACE, "NURBS-SURFACE" },
        };
    }

    public static string TypeName(Types type)
    {
        string s;

        if (TypeNames.TryGetValue(type, out s))
        {
            return s;
        }

        return TypeNames[Types.NONE];

        //string s = Enum.GetName(typeof(Types), type);
        //if (s == null) return "UNKNOWN";

        //return s;
    }


    #region  "public properties"
    public uint ID { get; set; }

    public Types Type
    {
        get;
        set;
    }

    public virtual bool IsLoop
    {
        get => false;
        set { /* Nop */ }
    }

    public virtual VertexList PointList => mPointList;

    public virtual int PointCount => PointList.Count;

    public VertexList StoreList => mStoreList;

    public bool Locked { set; get; } = false;

    public uint LayerID { set; get; } = 0;

    public bool Current { set; get; } = false;

    public bool IsSelected { get; set; } = false;

    public string Name { get; set; } = null;

    public LocalCoordinate LocalCoord = new LocalCoordinate();

    public DrawPen LinePen = DrawPen.InvalidPen;
    public DrawBrush FillBrush = DrawBrush.InvalidBrush;

    #endregion

    protected VertexList mPointList = new VertexList(4);

    protected VertexList mStoreList = null;


    #region Group management
    protected CadFigure mParent = null;

    public CadFigure Parent
    {
        set => mParent = value;
        get => mParent;
    }

    protected List<CadFigure> mChildList = new List<CadFigure>();

    public List<CadFigure> ChildList
    {
        get => mChildList;
        set => mChildList = value;
    }

    /// <summary>
    /// 自分自身とその下にあるFigureを全て列挙
    /// </summary>
    /// <param name="action"></param>
    /// <returns>true:列挙を継続</returns>
    public void ForEachFig(Action<CadFigure> action)
    {
        action(this);

        int i;
        for (i = 0; i < mChildList.Count; i++)
        {
            CadFigure c = mChildList[i];
            c.ForEachFig(action);
        }
    }
    #endregion

    protected CadFigure()
    {
        ID = 0;
        IsLoop = false;
        Type = Types.NONE;
    }

    public static CadFigure Create(Types type)
    {
        CadFigure fig = null;

        switch (type)
        {
            case Types.LINE:
                fig = new CadFigurePolyLines();
                break;

            case Types.RECT:
                fig = new CadFigurePolyLines();
                break;

            case Types.POLY_LINES:
                fig = new CadFigurePolyLines();
                break;

            case Types.CIRCLE:
                fig = new CadFigureCircle();
                break;

            case Types.POINT:
                fig = new CadFigurePoint();
                break;

            case Types.GROUP:
                fig = new CadFigureGroup();
                break;

            case Types.DIMENTION_LINE:
                fig = new CadFigureDimLine();
                break;

            case Types.MESH:
                fig = new CadFigureMesh();
                break;

            case Types.NURBS_LINE:
                fig = new CadFigureNurbsLine();
                break;

            case Types.NURBS_SURFACE:
                fig = new CadFigureNurbsSurface();
                break;

            case Types.PICTURE:
                fig = new CadFigurePicture();
                break;

            default:
                break;
        }

        return fig;
    }


    public virtual void AddPoints(VertexList points, int sp, int num)
    {
        for (int i = 0; i < num; i++)
        {
            CadVertex p = points[i + sp];
            AddPoint(p);
        }
    }

    public virtual void AddPoints(VertexList points, int sp)
    {
        AddPoints(points, sp, points.Count - sp);
    }

    public virtual void AddPoints(VertexList points)
    {
        foreach (CadVertex p in points)
        {
            AddPoint(p);
        }
    }

    public virtual void InsertPointAt(int index, CadVertex pt)
    {
        if (index > mPointList.Count - 1)
        {
            mPointList.Add(pt);
            return;
        }

        mPointList.Insert(index, pt);
    }

    public virtual void RemovePointAt(int index)
    {
        mPointList.RemoveAt(index);
    }

    public virtual void RemovePointsRange(int index, int count)
    {
        mPointList.RemoveRange(index, count);
    }

    public virtual void InsertPointsRange(int index, VertexList collection)
    {
        mPointList.InsertRange(index, collection);
    }

    public virtual bool HasSelectedPoint()
    {
        int i;
        for (i = 0; i < mPointList.Count; i++)
        {
            if (mPointList[i].Selected)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasSelectedPointInclueChild()
    {
        int i;
        for (i = 0; i < mPointList.Count; i++)
        {
            if (mPointList[i].Selected)
            {
                return true;
            }
        }

        if (ChildList == null)
        {
            return false;
        }

        for (i = 0; i < ChildList.Count; i++)
        {
            CadFigure c = ChildList[i];
            if (c.HasSelectedPointInclueChild())
            {
                return true;
            }
        }

        return false;
    }

    public virtual CadVertex GetPointAt(int idx)
    {
        return mPointList[idx];
    }

    public virtual CadVertex GetStorePointAt(int idx)
    {
        return mStoreList[idx];
    }

    public virtual void SetPointAt(int index, CadVertex pt)
    {
        mPointList[index] = pt;
    }

    public virtual void SelectPointAt(int index, bool sel)
    {
        CadVertex p = mPointList[index];
        p.Selected = sel;
        mPointList[index] = p;
    }

    public virtual void ClearSelectFlags()
    {
        int i;
        for (i = 0; i < mPointList.Count; i++)
        {
            SelectPointAt(i, false);
        }

        IsSelected = false;
    }

    public virtual void Select()
    {
        SelectAllPoints();

        mChildList.ForEach(c =>
        {
            c.Select();
        });
    }

    public virtual void SelectAllPoints()
    {
        // Set select flag to all points
        int i;
        for (i = 0; i < mPointList.Count; i++)
        {
            SelectPointAt(i, true);
        }
    }

    public virtual void StartEdit()
    {
        if (Locked) return;

        if (mStoreList != null)
        {
            return;
        }

        //DOut.pl($"StartEdit ID:{ID}");

        mStoreList = new VertexList();
        mStoreList.AddRange(mPointList);
    }

    public virtual void EndEdit()
    {
        if (mStoreList != null)
        {
            mStoreList.Clear();
            mStoreList = null;
        }

        //DOut.pl($"EndEdit ID:{ID}");
    }

    public virtual void CancelEdit()
    {
        if (Locked) return;

        if (mStoreList == null)
        {
            return;
        }

        mPointList.Clear();
        mStoreList.ForEach(a => mPointList.Add(a));
        mStoreList = null;
    }

    public virtual int FindPoint(CadVertex t)
    {
        int i = 0;
        foreach (CadVertex p in mPointList)
        {
            if (t.Equals(p))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    public virtual void AddChild(CadFigure fig)
    {
        if (Locked) return;

        mChildList.Add(fig);
        fig.SetParent(this);
    }

    public virtual void ReleaseAllChildlen()
    {
        if (Locked) return;

        foreach (CadFigure fig in mChildList)
        {
            fig.Parent = null;
        }

        mChildList.Clear();
    }

    #region Group
    public void SelectWithGroup()
    {
        CadFigure root = GetGroupRoot();
        root.Select();
    }

    public void SetParent(CadFigure fig)
    {
        mParent = fig;
    }

    public CadFigure GetParent()
    {
        return mParent;
    }

    public CadFigure GetGroupRoot()
    {
        CadFigure fig = this;
        CadFigure parent = null;

        while (fig != null)
        {
            parent = fig.GetParent();

            if (parent == null)
            {
                break;
            }

            fig = parent;
        }

        return fig;
    }
    #endregion

    public virtual void CopyFrom(CadFigure fig)
    {
        Type = fig.Type;
        IsLoop = fig.IsLoop;
        Locked = fig.Locked;

        mPointList.Clear();
        mPointList.AddRange(fig.mPointList);

        mParent = fig.mParent;

        mChildList.Clear();
        mChildList.AddRange(fig.mChildList);
    }

    public virtual void SetPointList(VertexList list)
    {
        mPointList = list;
    }

    #region "Dump" 
    public void SimpleDump(string prefix = nameof(CadFigure))
    {
        Log.pl(
            prefix +
            "(" + this.GetHashCode().ToString() + ")" +
            "ID=" + ID.ToString());
    }

    public void Dump(string prefix = nameof(CadFigure))
    {
        Log.pl(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
        Log.Indent++;
        Log.pl("ID=" + ID.ToString());

        string name = Name == null ? "null" : Name.ToString();

        Log.pl("Name=" + name);
        Log.pl("LayerID=" + LayerID.ToString());
        Log.pl("Type=" + Type.ToString());

        Log.pl("PointList [");
        Log.Indent++;
        foreach (CadVertex point in PointList)
        {
            point.dump("");
        }
        Log.Indent--;
        Log.pl("]");


        Log.pl("ParentID=" + (mParent != null ? mParent.ID : 0));

        Log.pl("Child [");
        Log.Indent++;
        foreach (CadFigure fig in mChildList)
        {
            Log.pl("" + fig.ID);
        }
        Log.Indent--;
        Log.pl("]");

        Log.Indent--;
        Log.pl("}");
    }

    #endregion

    public virtual void MoveSelectedPointsFromStored(DrawContext dc, MoveInfo moveInfo)
    {
        if (Locked) return;

        //Log.d("moveSelectedPoints" + 
        //    " dx=" + delta.x.ToString() +
        //    " dy=" + delta.y.ToString() +
        //    " dz=" + delta.z.ToString()
        //    );

        FigUtil.MoveSelectedPointsFromStored(this, dc, moveInfo);

        mChildList.ForEach(c =>
        {
            if (c.HasSelectedPoint())
            {
                c.MoveSelectedPointsFromStored(dc, moveInfo);
            }
        });
    }

    public virtual void MoveAllPoints(vector3_t delta)
    {
        if (Locked) return;

        FigUtil.MoveAllPoints(this, delta);
    }

    public virtual void AddPoint(CadVertex p)
    {
        mPointList.Add(p);
    }

    public virtual void AddPointInCreating(DrawContext dc, CadVertex p)
    {
        mPointList.Add(p);
    }

    public virtual void RemoveSelected()
    {
        if (Locked) return;

        mPointList.RemoveAll(a => a.Selected);

        if (PointCount < 2)
        {
            mPointList.Clear();
        }
    }

    public abstract void Draw(DrawContext dc, DrawOption dp);

    public abstract void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB);

    public abstract void DrawSelected(DrawContext dc, DrawOption dp);

    public abstract void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen);

    public abstract void StartCreate(DrawContext dc);

    public abstract void EndCreate(DrawContext dc);

    public void DrawEach(DrawContext dc, in DrawOption dp)
    {
        Draw(dc, dp);

        foreach (CadFigure c in ChildList)
        {
            c.DrawEach(dc, dp);
        }
    }

    public void DrawSelectedEach(DrawContext dc, DrawOption dp)
    {
        DrawSelected(dc, dp);

        foreach (CadFigure c in ChildList)
        {
            c.DrawSelectedEach(dc, dp);
        }
    }

    public virtual CadRect GetContainsRect()
    {
        return FigUtil.GetContainsRect(this);
    }

    public virtual CadRect GetContainsRectScrn(DrawContext dc)
    {
        return FigUtil.GetContainsRectScrn(this, dc);
    }

    public virtual VertexList GetPoints(int curveSplitNum)
    {
        return FigUtil.GetPoints(this, curveSplitNum);
    }

    public virtual Centroid GetCentroid()
    {
        return default(Centroid);
    }

    public virtual CadSegment GetSegmentAt(int n)
    {
        return FigUtil.GetSegmentAt(this, n);
    }

    public virtual FigureSegment GetFigSegmentAt(int n)
    {
        return FigUtil.GetFigSegmentAt(this, n);
    }

    public virtual int SegmentCount
    {
        get
        {
            return FigUtil.SegmentCount(this);
        }
    }

    public virtual bool IsSelectedAll()
    {
        int i;
        for (i = 0; i < mPointList.Count; i++)
        {
            if (!mPointList[i].Selected)
            {
                return false;
            }
        }

        return true;
    }

    public virtual bool IsPointSelected(int idx)
    {
        if (idx >= PointCount) return false;
        return PointList[idx].Selected;
    }

    public virtual void InvertDir()
    {
    }

    public virtual bool IsGarbage()
    {
        if (mPointList.Count > 0)
        {
            return false;
        }

        if (mChildList.Count > 0)
        {
            return false;
        }

        return true;
    }

    public void RemoveGarbageChildren()
    {
        for (int i = mChildList.Count - 1; i >= 0; i--)
        {
            CadFigure fig = mChildList[i];

            fig.RemoveGarbageChildren();

            if (fig.IsGarbage())
            {
                mChildList.RemoveAt(i);
            }
        }
    }

    public virtual void Rotate(vector3_t org, CadQuaternion q, CadQuaternion r)
    {
        CadQuaternion qp;

        int n = PointList.Count;

        for (int i = 0; i < n; i++)
        {
            CadVertex p = PointList[i];

            p.vector -= org;

            qp = CadQuaternion.FromPoint(p.vector);

            qp = r * qp;
            qp = qp * q;

            p.vector = qp.ToPoint();

            p += org;

            PointList[i] = p;
        }
    }

    public virtual void FlipWithPlane(vector3_t p0, vector3_t normal)
    {
        Log.plx("in");

        VertexList vl = PointList;

        for (int i = 0; i < vl.Count; i++)
        {
            CadVertex v = vl[i];

            vector3_t cp = CadMath.CrossPlane(v.vector, p0, normal);

            CadVertex d = v - cp;

            v = cp - d;

            v.Flag = vl[i].Flag;

            vl[i] = v;
        }


        Log.plx("out");
    }

} // End of class CadFigure
