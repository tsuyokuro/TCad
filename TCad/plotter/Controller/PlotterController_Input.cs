using CadDataTypes;
using TCad.Controls;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using System.Collections.Generic;
using TCad.ViewModel;

namespace Plotter.Controller;

// User interface handling
public partial class PlotterController
{
    public InteractCtrl InteractCtrl
    {
        get;
        private set;
    } = new InteractCtrl();

    public CadMouse Mouse { get; } = new CadMouse();

    public CadCursor CrossCursor = CadCursor.Create();

    private PointSearcher mPointSearcher = new PointSearcher();

    private SegSearcher mSegSearcher = new SegSearcher();

    private ItemCursor<NearPointSearcher.Result> mSpPointList = null;

    private CadRulerSet RulerSet = new CadRulerSet();


    public Vector3d StoreViewOrg = default;

    public Vector3d SnapPoint;

    public SnapInfo CurrentSnapInfo;

    public Vector3d MoveOrgScrnPoint;

    // 生のL button down point (デバイス座標系)
    public Vector3d RawDownPoint = default;

    // Snap等で補正された L button down point (World座標系)
    private Vector3d mLastDownPoint = default;
    public Vector3d LastDownPoint
    {
        get => mLastDownPoint;
        set
        {
            mLastDownPoint = value;
            ViewIF?.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
        }
    }

    // 選択したObjectの点の座標 (World座標系)
    public Vector3d ObjDownPoint = default;

    // 実際のMouse座標からCross cursorへのOffset
    public Vector3d CrossCursorOffset = default;

    private CadFigure mCurrentFigure = null;

    public MarkSegment? LastSelSegment = null;

    public MarkPoint? LastSelPoint = null;

    public CadFigure CurrentFigure
    {
        set
        {
            if (mCurrentFigure != null)
            {
                mCurrentFigure.GetGroupRoot().Current = false;
            }

            mCurrentFigure = value;

            if (mCurrentFigure != null)
            {
                mCurrentFigure.GetGroupRoot().Current = true;
            }
        }

        get
        {
            return mCurrentFigure;
        }
    }

    private bool mCursorLocked = false;
    public bool CursorLocked
    {
        set
        {
            mCursorLocked = value;
            ViewIF.CursorLocked(mCursorLocked);
            if (!mCursorLocked)
            {
                mSpPointList = null;
                ViewIF.ClosePopupMessage();
            }
            else
            {
                ViewIF.OpenPopupMessage("Cursor locked", UITypes.MessageType.INFO);
            }
        }

        get => mCursorLocked;
    }

    private List<HighlightPointListItem> HighlightPointList = new List<HighlightPointListItem>();

    private List<MarkSegment> HighlightSegList = new List<MarkSegment>();

    private Gridding mGridding = new Gridding();

    public Gridding Grid
    {
        get
        {
            return mGridding;
        }
    }


    private void InitHid()
    {
        Mouse.LButtonDown = LButtonDown;
        Mouse.LButtonUp = LButtonUp;

        Mouse.RButtonDown = RButtonDown;
        Mouse.RButtonUp = RButtonUp;

        Mouse.MButtonDown = MButtonDown;
        Mouse.MButtonUp = MButtonUp;

        Mouse.PointerMoved = MouseMove;

        Mouse.Wheel = Wheel;
    }

    private void ClearSelectionConditional(MarkPoint newSel)
    {
        if (!CadKeyboard.IsCtrlKeyDown())
        {
            if (!newSel.IsSelected())
            {
                ClearSelection();
            }
        }
    }

    private void ClearSelectionConditional(MarkSegment newSel)
    {
        if (!CadKeyboard.IsCtrlKeyDown())
        {
            if (!newSel.IsSelected())
            {
                ClearSelection();
            }
        }
    }

    public bool SelectNearest(DrawContext dc, Vector3d pixp)
    {
        SelectContext sc = default;

        ObjDownPoint = VectorExt.InvalidVector3d;

        RulerSet.Clear();

        sc.DC = dc;
        sc.CursorWorldPt = dc.DevPointToWorldPoint(pixp);
        sc.PointSelected = false;
        sc.SegmentSelected = false;

        sc.CursorScrPt = pixp;
        sc.Cursor = CadCursor.Create(pixp);

        sc = PointSelectNearest(sc);

        if (!sc.PointSelected)
        {
            //DOut.tpl("SelectNearest: sc.PointSelected=false");

            sc = SegSelectNearest(sc);

            if (!sc.SegmentSelected)
            {
                if (!CadKeyboard.IsCtrlKeyDown())
                {
                    ClearSelection();
                }
            }
        }

        if (ObjDownPoint.IsValid())
        {
            LastDownPoint = ObjDownPoint;

            CrossCursor.Pos = dc.WorldPointToDevPoint(ObjDownPoint);

            // LastDownPointを投影面上にしたい場合は、こちら
            //LastDownPoint = mSnapPoint;
        }
        else
        {
            LastDownPoint = SnapPoint;

            if (SettingsHolder.Settings.SnapToGrid)
            {
                mGridding.Clear();
                mGridding.Check(dc, pixp);

                LastDownPoint = mGridding.MatchW;
            }
        }

        return sc.PointSelected || sc.SegmentSelected;
    }

    private SelectContext PointSelectNearest(SelectContext sc)
    {
        mPointSearcher.Clean();
        mPointSearcher.SetRangePixel(sc.DC, SettingsHolder.Settings.PointSnapRange);
        mPointSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;

        //sc.Cursor.Pos.dump("CursorPos");

        mPointSearcher.SetTargetPoint(sc.Cursor);

        mPointSearcher.SearchAllLayer(sc.DC, mDB);

        if (CurrentFigure != null)
        {
            mPointSearcher.CheckFigure(sc.DC, CurrentLayer, CurrentFigure);
        }

        sc.MarkPt = mPointSearcher.GetXYMatch();

        //sc.MarkPt.dump();

        if (sc.MarkPt.FigureID == 0)
        {
            return sc;
        }

        ObjDownPoint = sc.MarkPt.Point;

        MoveOrgScrnPoint = sc.DC.WorldPointToDevPoint(sc.MarkPt.Point);

        MoveOrgScrnPoint.Z = 0;

        CadFigure fig = mDB.GetFigure(sc.MarkPt.FigureID);

        CadLayer layer = mDB.GetLayer(sc.MarkPt.LayerID);

        if (layer.Locked)
        {
            sc.MarkPt.reset();
            return sc;
        }

        ClearSelectionConditional(sc.MarkPt);

        if (SelectMode == SelectModes.POINT)
        {
            LastSelPoint = sc.MarkPt;

            sc.PointSelected = true;
            fig.SelectPointAt(sc.MarkPt.PointIndex, true);
        }
        else if (SelectMode == SelectModes.OBJECT)
        {
            LastSelPoint = sc.MarkPt;

            sc.PointSelected = true;
            fig.SelectWithGroup();
        }

        // Set ignore list for snap cursor
        //mPointSearcher.SetIgnoreList(SelList.List);
        //mSegSearcher.SetIgnoreList(SelList.List);

        if (sc.PointSelected)
        {
            RulerSet.Set(sc.MarkPt);
        }

        CurrentFigure = fig;

        return sc;
    }

    private SelectContext SegSelectNearest(SelectContext sc)
    {
        mSegSearcher.Clean();
        mSegSearcher.SetRangePixel(sc.DC, SettingsHolder.Settings.LineSnapRange);
        mSegSearcher.SetTargetPoint(sc.Cursor);
        mSegSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;

        mSegSearcher.SearchAllLayer(sc.DC, mDB);

        sc.MarkSeg = mSegSearcher.GetMatch();

        if (sc.MarkSeg.FigureID == 0)
        {
            return sc;
        }

        CadLayer layer = mDB.GetLayer(sc.MarkSeg.LayerID);

        if (layer.Locked)
        {
            sc.MarkSeg.FigSeg.Figure = null;
            return sc;
        }

        Vector3d center = sc.MarkSeg.CenterPoint;

        Vector3d t = sc.DC.WorldPointToDevPoint(center);

        if ((t - sc.CursorScrPt).Norm() < SettingsHolder.Settings.LineSnapRange)
        {
            ObjDownPoint = center;
        }
        else
        {
            ObjDownPoint = sc.MarkSeg.CrossPoint;
        }

        CadFigure fig = mDB.GetFigure(sc.MarkSeg.FigureID);

        ClearSelectionConditional(sc.MarkSeg);

        if (SelectMode == SelectModes.POINT)
        {
            LastSelPoint = null;
            LastSelSegment = sc.MarkSeg;

            sc.SegmentSelected = true;

            fig.SelectPointAt(sc.MarkSeg.PtIndexA, true);
            fig.SelectPointAt(sc.MarkSeg.PtIndexB, true);
        }
        else if (SelectMode == SelectModes.OBJECT)
        {
            sc.SegmentSelected = true;

            LastSelPoint = null;
            LastSelSegment = sc.MarkSeg;

            fig.SelectWithGroup();
        }

        MoveOrgScrnPoint = sc.DC.WorldPointToDevPoint(ObjDownPoint);

        if (sc.SegmentSelected)
        {
            RulerSet.Set(sc.MarkSeg, sc.DC);
        }

        CurrentFigure = fig;

        return sc;
    }

    private void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
    {
        if (State == ControllerStates.DRAGING_VIEW_ORG)
        {
            //ViewOrgDrag(pointer, DC, x, y);
            CurrentState.MouseMove(pointer, dc, x, y);
            return;
        }

        if (CursorLocked)
        {
            x = CrossCursor.Pos.X;
            y = CrossCursor.Pos.Y;
        }

        Vector3d pixp = new Vector3d(x, y, 0) - CrossCursorOffset;
        Vector3d cp = dc.DevPointToWorldPoint(pixp);

        CrossCursor.Pos = pixp;
        SnapPoint = cp;

        if (!CursorLocked)
        {
            SnapCursor(dc);
        }

        if (CurrentState.State == ControllerStates.DRAGING_POINTS || CurrentState.State == ControllerStates.RUBBER_BAND_SELECT)
        {
            CurrentState.MouseMove(pointer, dc, x, y);
        }

        ViewIF.CursorPosChanged(SnapPoint, CursorType.TRACKING);
        ViewIF.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
    }

    private void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        //DOut.tpl($"LButtonDown ({x},{y})");

        if (CursorLocked)
        {
            x = CrossCursor.Pos.X;
            y = CrossCursor.Pos.Y;
        }

        Vector3d pixp = new Vector3d(x, y, 0);

        RawDownPoint = pixp;

        CrossCursorOffset = pixp - CrossCursor.Pos;

        if (InteractCtrl.IsActive)
        {
            InteractCtrl.SetPoint(SnapPoint);
            LastDownPoint = SnapPoint;
            return;
        }

        CurrentState.LButtonDown(pointer, dc, x, y);

        if (CursorLocked)
        {
            CursorLocked = false;
        }

        ViewIF.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
    }

    private void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
        CurrentState.LButtonUp(pointer, dc, x, y);

        UpdateObjectTree(false);

        CrossCursorOffset = default;
    }

    private void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        pointer.MDownPoint = DC.WorldPointToDevPoint(SnapPoint);

        StateMachine.PushState(ControllerStates.DRAGING_VIEW_ORG);

        StoreViewOrg = dc.ViewOrg;
        CursorLocked = false;

        CrossCursor.Store();

        ViewIF.ChangeMouseCursor(UITypes.MouseCursorType.HAND);
    }

    private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
        Vector3d p = DC.WorldPointToDevPoint(SnapPoint);

        if (pointer.MDownPoint.X == p.X && pointer.MDownPoint.Y == p.Y)
        {
            ViewUtil.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
        }

        StateMachine.PopState();

        CrossCursor.Pos = new Vector3d(x, y, 0);

        ViewIF.ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    private void Wheel(CadMouse pointer, DrawContext dc, double x, double y, int delta)
    {
        if (CadKeyboard.IsCtrlKeyDown())
        {
            CursorLocked = false;

            double f;

            if (delta > 0)
            {
                f = 1.2;
            }
            else
            {
                f = 0.8;
            }

            ViewUtil.DpiUpDown(dc, f);
        }
    }

    private void RButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
    {
        LastDownPoint = SnapPoint;

        mContextMenuMan.RequestContextMenu(x, y);
    }

    private void RButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
    {
    }

    private void PointSnap(DrawContext dc)
    {
        // 複数の点が必要な図形を作成中、最初の点が入力された状態では、
        // オブジェクトがまだ作成されていない。このため、別途チェックする
        if (FigureCreator != null)
        {
            if (FigureCreator.Figure.PointCount == 1)
            {
                mPointSearcher.Check(dc, FigureCreator.Figure.GetPointAt(0).vector);
            }
        }

        if (InteractCtrl.IsActive)
        {
            foreach (Vector3d v in InteractCtrl.PointList)
            {
                mPointSearcher.Check(dc, v);
            }
        }

        // 計測用オブジェクトの点のチェック
        if (MeasureFigureCreator != null)
        {
            mPointSearcher.Check(dc, MeasureFigureCreator.Figure.PointList);
        }

        CheckExtendSnapPoints(dc);

        // Search point
        mPointSearcher.SearchAllLayer(dc, mDB);
    }

    private SnapInfo EvalPointSearcher(DrawContext dc, SnapInfo si)
    {
        MarkPoint mxy = mPointSearcher.GetXYMatch();
        MarkPoint mx = mPointSearcher.GetXMatch();
        MarkPoint my = mPointSearcher.GetYMatch();

        Vector3d cp = si.Cursor.Pos;

        if (mx.IsValid)
        {
            HighlightPointList.Add(
                new HighlightPointListItem(mx.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            Vector3d tp = dc.WorldPointToDevPoint(mx.Point);

            Vector3d distanceX = si.Cursor.DistanceX(tp);

            cp += distanceX;

            si.SnapPoint = dc.DevPointToWorldPoint(cp);
            si.PriorityMatch = SnapInfo.MatchType.X_MATCH;
        }

        if (my.IsValid)
        {
            HighlightPointList.Add(
                new HighlightPointListItem(my.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            Vector3d tp = dc.WorldPointToDevPoint(my.Point);

            Vector3d distanceY = si.Cursor.DistanceY(tp);

            cp += distanceY;

            si.SnapPoint = dc.DevPointToWorldPoint(cp);

            if (my.DistanceY < mx.DistanceX)
            {
                si.PriorityMatch = SnapInfo.MatchType.Y_MATCH;
            }
        }

        if (mxy.IsValid)
        {
            HighlightPointList.Clear();
            HighlightPointList.Add(new HighlightPointListItem(mxy.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT2)));
            si.SnapPoint = mxy.Point;
            si.IsPointMatch = true;
            si.PriorityMatch = SnapInfo.MatchType.POINT_MATCH;

            cp = dc.WorldPointToDevPoint(mxy.Point);
        }

        si.Cursor.Pos = cp;

        return si;
    }

    private void SegSnap(DrawContext dc)
    {
        mSegSearcher.SearchAllLayer(dc, mDB);
    }

    private SnapInfo EvalSegSeracher(DrawContext dc, SnapInfo si)
    {
        MarkSegment markSeg = mSegSearcher.GetMatch();

        if (mSegSearcher.IsMatch)
        {
            if (markSeg.Distance < si.Distance)
            {
                HighlightSegList.Add(markSeg);

                Vector3d center = markSeg.CenterPoint;

                Vector3d t = dc.WorldPointToDevPoint(center);

                if ((t - si.Cursor.Pos).Norm() < SettingsHolder.Settings.LineSnapRange)
                {
                    si.SnapPoint = center;
                    si.IsPointMatch = true;

                    si.Cursor.Pos = t;
                    si.Cursor.Pos.Z = 0;

                    HighlightPointList.Clear();
                    HighlightPointList.Add(new HighlightPointListItem(center, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT2)));
                }
                else
                {
                    si.SnapPoint = markSeg.CrossPoint;
                    si.IsPointMatch = true;

                    si.Cursor.Pos = markSeg.CrossPointScrn;
                    si.Cursor.Pos.Z = 0;

                    HighlightPointList.Add(new HighlightPointListItem(si.SnapPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
                }
            }
            else
            {
                mSegSearcher.Clean();
            }
        }

        return si;
    }

    private SnapInfo SnapGrid(DrawContext dc, SnapInfo si)
    {
        mGridding.Clear();
        mGridding.Check(dc, (Vector3d)si.Cursor.Pos);

        si.Cursor.Pos = mGridding.MatchD;


        //si.SnapPoint = DC.DevPointToWorldPoint(si.Cursor.Pos);
        si.SnapPoint = mGridding.MatchW;

        //HighlightPointList.Add(new HighlightPointListItem(si.SnapPoint, DC.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

        return si;
    }

    private SnapInfo SnapLine(DrawContext dc, SnapInfo si)
    {
        if (mPointSearcher.IsXMatch)
        {
            si.Cursor.Pos.X = mPointSearcher.GetXMatch().PointScrn.X;
        }

        if (mPointSearcher.IsYMatch)
        {
            si.Cursor.Pos.Y = mPointSearcher.GetYMatch().PointScrn.Y;
        }

        RulerInfo ri = RulerSet.Capture(dc, si.Cursor, SettingsHolder.Settings.LineSnapRange);

        if (ri.IsValid)
        {
            si.SnapPoint = ri.CrossPoint;
            si.Cursor.Pos = dc.WorldPointToDevPoint(si.SnapPoint);

            if (mSegSearcher.IsMatch)
            {
                MarkSegment ms = mSegSearcher.GetMatch();

                if (ms.FigureID != ri.Ruler.Fig.ID)
                {
                    Vector3d cp = PlotterUtil.CrossOnScreen(dc, ri.Ruler.P0, ri.Ruler.P1, ms.FigSeg.Point0.vector, ms.FigSeg.Point1.vector);

                    if (cp.IsValid())
                    {
                        si.SnapPoint = dc.DevPointToWorldPoint(cp);
                        si.Cursor.Pos = cp;
                    }
                }
            }

            HighlightPointList.Add(new HighlightPointListItem(ri.Ruler.P1, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            // 点が線分上にある時は、EvalSegSeracherで登録されているのでポイントを追加しない
            Vector3d p0 = dc.WorldPointToDevPoint(ri.Ruler.P0);
            Vector3d p1 = dc.WorldPointToDevPoint(ri.Ruler.P1);
            Vector3d crp = dc.WorldPointToDevPoint(ri.CrossPoint);

            if (!CadMath.IsPointInSeg2D(p0, p1, crp))
            {
                HighlightPointList.Add(new HighlightPointListItem(ri.CrossPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
            }
        }

        return si;
    }

    private void SnapCursor(DrawContext dc)
    {
        HighlightPointList.Clear();

        SnapInfo si =
            new SnapInfo(
                CrossCursor,
                SnapPoint,
                mPointSearcher.Distance()
                );

        #region Point search

        mPointSearcher.Clean();
        mPointSearcher.SetRangePixel(dc, SettingsHolder.Settings.PointSnapRange);
        mPointSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;
        mPointSearcher.SetTargetPoint(CrossCursor);

        if (!SettingsHolder.Settings.SnapToSelfPoint)
        {
            // Current figure にスナップしない
            if (CurrentFigure != null)
            {
                mPointSearcher.AddIgnoreFigureID(CurrentFigure.ID);
            }
        }

        // (0, 0, 0)にスナップするようにする
        if (SettingsHolder.Settings.SnapToZero)
        {
            mPointSearcher.Check(dc, Vector3d.Zero);
        }

        // 最後にマウスダウンしたポイントにスナップする
        if (SettingsHolder.Settings.SnapToLastDownPoint)
        {
            mPointSearcher.Check(dc, LastDownPoint);
        }

        if (SettingsHolder.Settings.SnapToPoint)
        {
            PointSnap(dc);
            si = EvalPointSearcher(dc, si);

            //DOut.tpl($"si.si.PriorityMatch: {si.PriorityMatch}");
        }

        #endregion

        #region Segment search

        mSegSearcher.Clean();
        mSegSearcher.SetRangePixel(dc, SettingsHolder.Settings.LineSnapRange);
        mSegSearcher.SetTargetPoint(si.Cursor);
        mSegSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;
        mSegSearcher.SetCheckPriorityWithSnapInfo(si);

        HighlightSegList.Clear();

        if (SettingsHolder.Settings.SnapToSegment)
        {
            if (!mPointSearcher.IsXYMatch)
            {
                SegSnap(dc);
                si = EvalSegSeracher(dc, si);
            }
        }

        #endregion

        if (SettingsHolder.Settings.SnapToGrid)
        {
            if (!mPointSearcher.IsXYMatch && !mSegSearcher.IsMatch)
            {
                si = SnapGrid(dc, si);
            }
        }

        if (SettingsHolder.Settings.SnapToLine)
        {
            if (!mPointSearcher.IsXYMatch)
            {
                si = SnapLine(dc, si);
            }
        }

        SnapPoint = si.SnapPoint;
        CrossCursor.Pos = si.Cursor.Pos;

        CurrentSnapInfo = si;
    }

    public void MoveCursorToNearPoint(DrawContext dc)
    {
        if (mSpPointList == null)
        {
            NearPointSearcher searcher = new NearPointSearcher(this);

            var resList = searcher.Search((CadVertex)CrossCursor.Pos, 64);

            if (resList.Count == 0)
            {
                return;
            }

            mSpPointList = new ItemCursor<NearPointSearcher.Result>(resList);
        }

        NearPointSearcher.Result res = mSpPointList.LoopNext();

        ItConsole.println(res.ToInfoString());

        Vector3d sv = DC.WorldPointToDevPoint(res.WoldPoint.vector);

        LockCursorScrn(sv);

        Mouse.MouseMove(dc, sv.X, sv.Y);
    }

    public void LockCursorScrn(Vector3d p)
    {
        CursorLocked = true;

        SnapPoint = DC.DevPointToWorldPoint(p);
        CrossCursor.Pos = p;
    }

    public Vector3d GetCursorPos()
    {
        return SnapPoint;
    }

    public void SetCursorWoldPos(Vector3d v)
    {
        SnapPoint = v;
        CrossCursor.Pos = DC.WorldPointToDevPoint(SnapPoint);

        ViewIF.CursorPosChanged(SnapPoint, CursorType.TRACKING);
    }

    public void AddExtendSnapPoint()
    {
        ExtendSnapPointList.Add(LastDownPoint);
    }

    public void ClearExtendSnapPointList()
    {
        ExtendSnapPointList.Clear();
    }

    private void CheckExtendSnapPoints(DrawContext dc)
    {
        ExtendSnapPointList.ForEach(v => mPointSearcher.Check(dc, v));
    }
}
