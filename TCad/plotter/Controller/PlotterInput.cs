using CadDataTypes;
using Plotter.Settings;
using System.Collections.Generic;
using TCad.MathFunctions;
using TCad.Plotter.DrawToolSet;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.searcher;
using TCad.ViewModel;

namespace Plotter.Controller;

public class PlotterInput
{
    IPlotterController Controller;

    public DrawContext DC
    {
        get => Controller.DC;
    }

    public CadObjectDB DB
    {
        get => Controller.DB;
    }

    public CadLayer CurrentLayer
    {
        get => Controller.CurrentLayer;
    }

    public SelectModes SelectMode
    {
        get => Controller.SelectMode;
    }

    public ControllerStates StateID
    {
        get => Controller.StateID;
    }

    public ControllerState CurrentState
    {
        get => Controller.CurrentState;
    }

    public FigCreator FigureCreator
    {
        get => Controller.FigureCreator;
    }

    public FigCreator MeasureFigureCreator
    {
        get => Controller.MeasureFigureCreator;
    }

    public ControllerStateMachine StateMachine
    {
        get => Controller.StateMachine;
    }

    public Vector3List ExtendSnapPointList
    {
        get;
        private set;
    } = new Vector3List(20);


    public InteractCtrl InteractCtrl
    {
        get;
        private set;
    } = new InteractCtrl();

    public CadMouse Mouse
    {
        get;
        private set;
    } = new CadMouse();

    public CadCursor CrossCursor = CadCursor.Create();

    private PointSearcher mPointSearcher = new PointSearcher();

    private SegSearcher mSegSearcher = new SegSearcher();

    private ItemCursor<NearPointSearcher.Result> mSpPointList = null;

    private CadRulerSet RulerSet = new CadRulerSet();


    public vector3_t StoreViewOrg = default;

    public vector3_t SnapPoint;

    public SnapInfo CurrentSnapInfo;

    // 生のL button down point (デバイス座標系)
    public vector3_t RawDownPoint
    {
        get;
        set;
    } = default;

    // Snap等で補正された L button down point (World座標系)
    private vector3_t mLastDownPoint = default;
    public vector3_t LastDownPoint
    {
        get => mLastDownPoint;
        set
        {
            mLastDownPoint = value;
            Controller.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
        }
    }

    // 選択したObjectの点の座標 (World座標系)
    public vector3_t ObjDownPoint = default;

    // 実際のMouse座標からCross cursorへのOffset
    public vector3_t CrossCursorOffset = default;


    public MarkSegment? LastSelSegment_ = null;

    public MarkSegment? LastSelSegment
    {
        set
        {
            LastSelSegment_ = value;
        }

        get => LastSelSegment_;
    }

    public MarkPoint? LastSelPoint_ = null;
    public MarkPoint? LastSelPoint
    {
        set
        {
            LastSelPoint_ = value;
            if (LastSelPoint_ != null)
            {
                Log.pl($"==== LastSelPoint FigID:{LastSelPoint_.Value.FigureID} Idx:{LastSelPoint_.Value.PointIndex}");
            }
            else
            {
                Log.pl("==== LastSelPoint set null");
            }
        }
        get => LastSelPoint_;
    }


    private CadFigure CurrentFigure_ = null;

    public CadFigure CurrentFigure
    {
        set
        {
            if (CurrentFigure_ != null)
            {
                CurrentFigure_.GetGroupRoot().Current = false;
            }

            CurrentFigure_ = value;

            if (CurrentFigure_ != null)
            {
                CurrentFigure_.GetGroupRoot().Current = true;
            }
        }

        get
        {
            return CurrentFigure_;
        }
    }

    public bool CursorLocked
    {
        private set;
        get;
    }

    public List<HighlightPointListItem> HighlightPointList = new List<HighlightPointListItem>();

    public List<MarkSegment> HighlightSegList = new List<MarkSegment>();

    private Gridding mGridding = new Gridding();

    public Gridding Grid
    {
        get
        {
            return mGridding;
        }
    }

    public PlotterInput(IPlotterController controller)
    {
        Controller = controller;

        ObjDownPoint = VectorExt.InvalidVector3;

        InitHid();
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

    public bool SelectNearest(DrawContext dc, vector3_t pixp)
    {
        SelectContext sc = default;

        ObjDownPoint = VectorExt.InvalidVector3;

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

        mPointSearcher.SearchAllLayer(sc.DC, DB);

        if (CurrentFigure != null)
        {
            mPointSearcher.CheckFigure(sc.DC, CurrentLayer, CurrentFigure);
        }

        sc.MarkPt = mPointSearcher.XYMatch;

        //sc.MarkPt.dump();

        if (sc.MarkPt.FigureID == 0)
        {
            return sc;
        }

        ObjDownPoint = sc.MarkPt.Point;

        CadFigure fig = DB.GetFigure(sc.MarkPt.FigureID);

        CadLayer layer = DB.GetLayer(sc.MarkPt.LayerID);

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

        mSegSearcher.SearchAllLayer(sc.DC, DB);

        sc.MarkSeg = mSegSearcher.MatchSegment;

        if (sc.MarkSeg.FigureID == 0)
        {
            return sc;
        }

        CadLayer layer = DB.GetLayer(sc.MarkSeg.LayerID);

        if (layer.Locked)
        {
            sc.MarkSeg.FigSeg.Figure = null;
            return sc;
        }

        vector3_t center = sc.MarkSeg.CenterPoint;

        vector3_t t = sc.DC.WorldPointToDevPoint(center);

        if ((t - sc.CursorScrPt).Norm() < SettingsHolder.Settings.LineSnapRange)
        {
            ObjDownPoint = center;
        }
        else
        {
            ObjDownPoint = sc.MarkSeg.CrossPoint;
        }

        CadFigure fig = DB.GetFigure(sc.MarkSeg.FigureID);

        ClearSelectionConditional(sc.MarkSeg);

        if (SelectMode == SelectModes.POINT)
        {
            LastSelSegment = sc.MarkSeg;

            sc.SegmentSelected = true;

            fig.SelectPointAt(sc.MarkSeg.PtIndexA, true);
            fig.SelectPointAt(sc.MarkSeg.PtIndexB, true);
            UpdateLastSelPointWithSelSegment(sc, LastSelSegment.Value);
        }
        else if (SelectMode == SelectModes.OBJECT)
        {
            sc.SegmentSelected = true;

            LastSelSegment = sc.MarkSeg;

            fig.SelectWithGroup();
            UpdateLastSelPointWithSelSegment(sc, LastSelSegment.Value);
        }

        if (sc.SegmentSelected)
        {
            RulerSet.Set(sc.MarkSeg, sc.DC);
        }

        CurrentFigure = fig;

        return sc;
    }

    private void UpdateLastSelPointWithSelSegment(SelectContext sc, MarkSegment selSeg)
    {
        if (!selSeg.Valid) return;

        var fig = LastSelSegment.Value.Figure;

        var pa = DC.WorldPointToDevPoint(selSeg.pA);
        var pb = DC.WorldPointToDevPoint(selSeg.pB);

        int idx = -1;

        if ((pa - sc.CursorScrPt).Norm() <= (pb - sc.CursorScrPt).Norm())
        {
            idx = selSeg.PtIndexA;
        }
        else
        {
            idx = selSeg.PtIndexB;
        }

        MarkPoint markPoint = default;

        markPoint.Figure = fig;
        markPoint.Point = fig.PointList[idx].vector;
        markPoint.PointIndex = idx;
        markPoint.PointScrn = DC.WorldPointToDevPoint(markPoint.Point);

        LastSelPoint = markPoint;
    }

    private void MouseMove(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        if (StateID == ControllerStates.DRAGING_VIEW_ORG)
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

        vector3_t pixp = new vector3_t(x, y, 0) - CrossCursorOffset;
        vector3_t cp = dc.DevPointToWorldPoint(pixp);

        CrossCursor.Pos = pixp;
        SnapPoint = cp;

        if (!CursorLocked)
        {
            SnapCursor(dc);
        }

        if (CurrentState.ID == ControllerStates.DRAGING_POINTS || CurrentState.ID == ControllerStates.RUBBER_BAND_SELECT)
        {
            CurrentState.MouseMove(pointer, dc, x, y);
        }

        Controller.CursorPosChanged(SnapPoint, CursorType.TRACKING);
        Controller.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
    }

    private void LButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        //DOut.tpl($"LButtonDown ({x},{y})");

        if (CursorLocked)
        {
            x = CrossCursor.Pos.X;
            y = CrossCursor.Pos.Y;
        }

        vector3_t pixp = new vector3_t(x, y, 0);

        RawDownPoint = pixp;

        CrossCursorOffset = pixp - CrossCursor.Pos;

        if (InteractCtrl.IsActive)
        {
            InteractCtrl.SetPoint(SnapPoint);
            LastDownPoint = SnapPoint;
            return;
        }

        CurrentState.LButtonDown(pointer, dc, x, y);

        Controller.UpdateObjectTree(false);

        UnlockCursor();

        Controller.CursorPosChanged(LastDownPoint, CursorType.LAST_DOWN);
    }

    private void LButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        CurrentState.LButtonUp(pointer, dc, x, y);

        Controller.UpdateObjectTree(false);

        CrossCursorOffset = default;
    }

    private void MButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        pointer.MDownPoint = DC.WorldPointToDevPoint(SnapPoint);

        StateMachine.PushState(ControllerStates.DRAGING_VIEW_ORG);

        StoreViewOrg = dc.ViewOrg;

        UnlockCursor();

        CrossCursor.Store();

        Controller.ChangeMouseCursor(UITypes.MouseCursorType.HAND);
    }

    private void MButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        vector3_t p = DC.WorldPointToDevPoint(SnapPoint);

        if (pointer.MDownPoint.X == p.X && pointer.MDownPoint.Y == p.Y)
        {
            ViewUtil.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
        }

        StateMachine.PopState();

        CrossCursor.Pos = new vector3_t(x, y, 0);

        Controller.ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    private void Wheel(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y, int delta)
    {
        if (CadKeyboard.IsCtrlKeyDown())
        {
            UnlockCursor();

            vcompo_t f;

            if (delta > 0)
            {
                f = (vcompo_t)(1.2);
            }
            else
            {
                f = (vcompo_t)(0.8);
            }

            ViewUtil.DpiUpDown(dc, f);
        }
    }

    private void RButtonDown(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
        LastDownPoint = SnapPoint;

        Controller.ContextMenuMan.RequestContextMenu(x, y);
    }

    private void RButtonUp(CadMouse pointer, DrawContext dc, vcompo_t x, vcompo_t y)
    {
    }

    private void PointSnap(DrawContext dc, PointSearcher pointSearcher)
    {
        // 複数の点が必要な図形を作成中、最初の点が入力された状態では、
        // オブジェクトがまだ作成されていない。このため、別途チェックする
        if (FigureCreator != null)
        {
            if (FigureCreator.Figure.PointCount == 1)
            {
                pointSearcher.Check(dc, FigureCreator.Figure.GetPointAt(0).vector);
            }
        }

        if (InteractCtrl.IsActive)
        {
            foreach (vector3_t v in InteractCtrl.PointList)
            {
                pointSearcher.Check(dc, v);
            }
        }

        // 計測用オブジェクトの点のチェック
        if (MeasureFigureCreator != null)
        {
            pointSearcher.Check(dc, MeasureFigureCreator.Figure.PointList);
        }

        CheckExtendSnapPoints(dc);

        // Search point
        pointSearcher.SearchAllLayer(dc, DB);
    }

    private SnapInfo EvalPointSearcher(DrawContext dc, SnapInfo si, PointSearcher pointSearcher)
    {
        MarkPoint mxy = pointSearcher.XYMatch;
        MarkPoint mx = pointSearcher.XMatch;
        MarkPoint my = pointSearcher.YMatch;

        vector3_t cp = si.Cursor.Pos;

        if (mx.IsValid)
        {
            HighlightPointList.Add(
                new HighlightPointListItem(mx.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            vector3_t tp = dc.WorldPointToDevPoint(mx.Point);

            vector3_t distanceX = si.Cursor.DistanceX(tp);

            cp += distanceX;

            si.SnapPoint = dc.DevPointToWorldPoint(cp);
        }

        if (my.IsValid)
        {
            HighlightPointList.Add(
                new HighlightPointListItem(my.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            vector3_t tp = dc.WorldPointToDevPoint(my.Point);

            vector3_t distanceY = si.Cursor.DistanceY(tp);

            cp += distanceY;

            si.SnapPoint = dc.DevPointToWorldPoint(cp);
        }

        if (mxy.IsValid)
        {
            HighlightPointList.Clear();
            HighlightPointList.Add(new HighlightPointListItem(mxy.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT2)));
            si.SnapPoint = mxy.Point;
            si.IsPointMatch = true;

            cp = dc.WorldPointToDevPoint(mxy.Point);
        }

        si.Cursor.Pos = cp;

        return si;
    }

    private void SegSnap(DrawContext dc, SegSearcher segSearcher)
    {
        segSearcher.SearchAllLayer(dc, DB);
    }

    private SnapInfo EvalSegSeracher(DrawContext dc, SnapInfo si, SegSearcher segSearcher)
    {
        MarkSegment matchSeg = segSearcher.MatchSegment;

        if (segSearcher.IsMatch)
        {
            if (matchSeg.Distance < si.PointSearcher.Distance)
            {
                HighlightSegList.Add(matchSeg);

                vector3_t center = matchSeg.CenterPoint;

                vector3_t t = dc.WorldPointToDevPoint(center);

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
                    si.SnapPoint = matchSeg.CrossPoint;
                    si.IsPointMatch = true;

                    si.Cursor.Pos = dc.WorldPointToDevPoint(matchSeg.CrossPoint);
                    si.Cursor.Pos.Z = 0;

                    HighlightPointList.Add(new HighlightPointListItem(si.SnapPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
                }
            }
            else
            {
                segSearcher.Clean();
            }
        }

        return si;
    }

    private SnapInfo SnapGrid(DrawContext dc, SnapInfo si)
    {
        mGridding.Clear();
        mGridding.Check(dc, (vector3_t)si.Cursor.Pos);

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
            si.Cursor.Pos.X = mPointSearcher.XMatch.PointScrn.X;
        }

        if (mPointSearcher.IsYMatch)
        {
            si.Cursor.Pos.Y = mPointSearcher.YMatch.PointScrn.Y;
        }


        RulerInfo ri = RulerSet.Capture(dc, si.Cursor, SettingsHolder.Settings.LineSnapRange);

        if (ri.IsValid)
        {
            si.SnapPoint = ri.CrossPoint;
            si.Cursor.Pos = dc.WorldPointToDevPoint(si.SnapPoint);

            if (mSegSearcher.IsMatch)
            {
                MarkSegment ms = mSegSearcher.MatchSegment;

                if (ms.FigureID != ri.Ruler.Fig.ID)
                {
                    vector3_t cp = PlotterUtil.CrossOnScreen(dc, ri.Ruler.P0, ri.Ruler.P1, ms.FigSeg.Point0.vector, ms.FigSeg.Point1.vector);

                    if (cp.IsValid())
                    {
                        si.SnapPoint = dc.DevPointToWorldPoint(cp);
                        si.Cursor.Pos = cp;
                    }
                }
            }

            HighlightPointList.Add(new HighlightPointListItem(ri.Ruler.P1, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

            // 点が線分上にある時は、EvalSegSeracherで登録されているのでポイントを追加しない
            vector3_t p0 = dc.WorldPointToDevPoint(ri.Ruler.P0);
            vector3_t p1 = dc.WorldPointToDevPoint(ri.Ruler.P1);
            vector3_t crp = dc.WorldPointToDevPoint(ri.CrossPoint);

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
                mPointSearcher,
                mSegSearcher
                );

        if (SettingsHolder.Settings.SnapToGrid)
        {
            si = SnapGrid(dc, si);
            if (si.IsPointMatch)
            {
                return;
            }
        }

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
            mPointSearcher.Check(dc, vector3_t.Zero);
        }

        // 最後にマウスダウンしたポイントにスナップする
        if (SettingsHolder.Settings.SnapToLastDownPoint)
        {
            mPointSearcher.Check(dc, LastDownPoint);
        }

        if (SettingsHolder.Settings.SnapToPoint)
        {
            PointSnap(dc, mPointSearcher);
        }

        si = EvalPointSearcher(dc, si, mPointSearcher);

        #endregion


        mSegSearcher.Clean();
        mSegSearcher.SetRangePixel(dc, SettingsHolder.Settings.LineSnapRange);
        mSegSearcher.SetTargetPoint(si.Cursor);
        mSegSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;

        HighlightSegList.Clear();

        if (SettingsHolder.Settings.SnapToSegment)
        {
            if (!mPointSearcher.IsXYMatch)
            {
                SegSnap(dc, mSegSearcher);
            }
        }

        si = EvalSegSeracher(dc, si, mSegSearcher);


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
            NearPointSearcher searcher = new NearPointSearcher(Controller);

            var resList = searcher.Search((CadVertex)CrossCursor.Pos, 64);

            if (resList.Count == 0)
            {
                return;
            }

            mSpPointList = new ItemCursor<NearPointSearcher.Result>(resList);
        }

        NearPointSearcher.Result res = mSpPointList.LoopNext();

        ItConsole.println(res.ToInfoString());

        vector3_t sv = DC.WorldPointToDevPoint(res.WoldPoint.vector);

        LockCursor(sv);

        LastDownPoint = res.WoldPoint.vector;

        Mouse.MouseMove(dc, sv.X, sv.Y);
    }

    public void LockCursor(vector3_t devP)
    {
        bool prevSate = CursorLocked;
        CursorLocked = true;

        SnapPoint = DC.DevPointToWorldPoint(devP);
        CrossCursor.Pos = devP;

        if (prevSate != CursorLocked)
        {
            NotifyCursorLock(CursorLocked);
        }
    }

    public void LockCursor()
    {
        bool prevSate = CursorLocked;
        CursorLocked = true;

        LastDownPoint = DC.DevPointToWorldPoint(CrossCursor.Pos);
        SnapPoint = DC.DevPointToWorldPoint(LastDownPoint);

        if (prevSate != CursorLocked)
        {
            NotifyCursorLock(CursorLocked);
        }
    }

    public void UnlockCursor()
    {
        if (CursorLocked)
        {
            CursorLocked = false;
            NotifyCursorLock(false);
        }
    }

    private void NotifyCursorLock(bool locked)
    {
        Controller.CursorLocked(locked);
        if (!locked)
        {
            mSpPointList = null;
            Controller.ClosePopupMessage();
        }
        else
        {
            Controller.OpenPopupMessage("Cursor locked", UITypes.MessageType.INFO);
        }
    }

    public vector3_t GetCursorPos()
    {
        return SnapPoint;
    }

    public void SetCursorWoldPos(vector3_t v)
    {
        SnapPoint = v;
        CrossCursor.Pos = DC.WorldPointToDevPoint(SnapPoint);

        Controller.CursorPosChanged(SnapPoint, CursorType.TRACKING);
    }

    public void AddExtendSnapPoint()
    {
        //ExtendSnapPointList.Add(LastDownPoint);
        ExtendSnapPointList.Add(GetCursorPos());
    }

    public void ClearExtendSnapPointList()
    {
        ExtendSnapPointList.Clear();
    }

    private void CheckExtendSnapPoints(DrawContext dc)
    {
        ExtendSnapPointList.ForEach(v => mPointSearcher.Check(dc, v));
    }

    public void ClearSelection()
    {
        CurrentFigure = null;

        LastSelPoint = null;
        LastSelSegment = null;

        foreach (CadLayer layer in DB.LayerList)
        {
            layer.ClearSelectedFlags();
        }
    }
}
