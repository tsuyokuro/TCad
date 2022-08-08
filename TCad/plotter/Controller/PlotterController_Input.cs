using CadDataTypes;
using TCad.Controls;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    // User interface handling
    public partial class PlotterController
    {
        private InteractCtrl mInteractCtrl = new InteractCtrl();
        public InteractCtrl InteractCtrl
        {
            get => mInteractCtrl;
        }

        public CadMouse Mouse { get; } = new CadMouse();

        public CadCursor CrossCursor = CadCursor.Create();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private ItemCursor<NearPointSearcher.Result> mSpPointList = null;

        private CadRulerSet RulerSet = new CadRulerSet();


        private Vector3d StoreViewOrg = default;

        private Vector3d SnapPoint;

        private SnapInfo mSnapInfo;

        private Vector3d MoveOrgScrnPoint;

        // 生のL button down point (デバイス座標系)
        private Vector3d RawDownPoint = default;

        // Snap等で補正された L button down point (World座標系)
        public Vector3d LastDownPoint = default;

        // 選択したObjectの点の座標 (World座標系)
        private Vector3d ObjDownPoint = default;
        private Vector3d StoredObjDownPoint = default;

        // 実際のMouse座標からCross cursorへのOffset
        private Vector3d OffsetScreen = default;

        public Vector3d RubberBandScrnPoint0 = VectorExt.InvalidVector3d;

        public Vector3d RubberBandScrnPoint1 = default;

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
        private bool CursorLocked
        {
            set
            {
                mCursorLocked = value;
                Callback.CursorLocked(mCursorLocked);
                if (!mCursorLocked)
                {
                    mSpPointList = null;
                    Callback.ClosePopupMessage();
                }
                else
                {
                    Callback.OpenPopupMessage("Cursor locked", PlotterCallback.MessageType.INFO);
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

        private void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            //DOut.tpl($"LButtonDown ({x},{y})");

            if (CursorLocked)
            {
                x = CrossCursor.Pos.X;
                y = CrossCursor.Pos.Y;
            }

            Vector3d pixp = new Vector3d(x, y, 0);
            Vector3d cp = dc.DevPointToWorldPoint(pixp);

            RawDownPoint = pixp;

            RubberBandScrnPoint1 = pixp;
            RubberBandScrnPoint0 = pixp;

            OffsetScreen = pixp - CrossCursor.Pos;

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.SetPoint(SnapPoint);
                LastDownPoint = SnapPoint;
                return;
            }

            switch (State)
            {
                case States.SELECT:
                    if (SelectNearest(dc, (Vector3d)CrossCursor.Pos))
                    {
                        if (!CursorLocked)
                        {
                            State = States.START_DRAGING_POINTS;
                        }

                        OffsetScreen = pixp - CrossCursor.Pos;

                        StoredObjDownPoint = ObjDownPoint;
                    }
                    else
                    {
                        State = States.RUBBER_BAND_SELECT;
                    }

                    break;

                case States.RUBBER_BAND_SELECT:

                    break;

                case States.START_CREATE:
                    {
                        LastDownPoint = SnapPoint;

                        CadFigure fig = mDB.NewFigure(CreatingFigType);

                        mFigureCreator = FigCreator.Get(CreatingFigType, fig);

                        State = States.CREATING;

                        FigureCreator.StartCreate(dc);


                        CadVertex p = (CadVertex)dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, (CadVertex)SnapPoint);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = SnapPoint;

                        CadVertex p = (CadVertex)dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, (CadVertex)SnapPoint);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = SnapPoint;

                        CadVertex p;

                        if (mSnapInfo.IsPointMatch)
                        {
                            p = new CadVertex(SnapPoint);
                        }
                        else
                        {
                            p = (CadVertex)dc.DevPointToWorldPoint(CrossCursor.Pos);
                        }

                        SetPointInMeasuring(dc, p);
                        PutMeasure();
                    }
                    break;

                default:
                    break;

            }

            if (CursorLocked)
            {
                CursorLocked = false;
            }

            Callback.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void PutMeasure()
        {
            int pcnt = MeasureFigureCreator.Figure.PointCount;

            double currentD = 0;

            if (pcnt > 1)
            {
                CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                currentD = (p1 - p0).Norm();
                currentD = Math.Round(currentD, 4);
            }

            double a = 0;

            if (pcnt > 2)
            {
                CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 3);
                CadVertex p2 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                Vector3d v1 = p1.vector - p0.vector;
                Vector3d v2 = p2.vector - p0.vector;

                double t = CadMath.AngleOfVector(v1, v2);
                a = CadMath.Rad2Deg(t);
                a = Math.Round(a, 4);
            }

            double totalD = CadUtil.AroundLength(MeasureFigureCreator.Figure);

            totalD = Math.Round(totalD, 4);

            int cnt = MeasureFigureCreator.Figure.PointCount;

            ItConsole.println("[" + cnt.ToString() + "]" +
                AnsiEsc.Reset + " LEN:" + AnsiEsc.BGreen + currentD.ToString() +
                AnsiEsc.Reset + " ANGLE:" + AnsiEsc.BBlue + a.ToString() +
                AnsiEsc.Reset + " TOTAL:" + totalD.ToString());
        }

        private void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            mBackState = State;

            pointer.MDownPoint = DC.WorldPointToDevPoint(SnapPoint);

            State = States.DRAGING_VIEW_ORG;

            StoreViewOrg = dc.ViewOrg;
            CursorLocked = false;

            CrossCursor.Store();

            Callback.ChangeMouseCursor(PlotterCallback.MouseCursorType.HAND);
        }

        private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            Vector3d p = DC.WorldPointToDevPoint(SnapPoint);

            if (pointer.MDownPoint.X == p.X && pointer.MDownPoint.Y == p.Y)
            {
                ViewUtil.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
            }

            State = mBackState;

            CrossCursor.Pos = new Vector3d(x, y, 0);

            Callback.ChangeMouseCursor(PlotterCallback.MouseCursorType.CROSS);
        }

        private void ViewOrgDrag(CadMouse pointer, DrawContext dc, double x, double y)
        {
            //DOut.tpl("ViewOrgDrag");

            Vector3d cp = new Vector3d(x, y, 0);

            Vector3d d = cp - pointer.MDownPoint;

            Vector3d op = StoreViewOrg + d;

            ViewUtil.SetOrigin(dc, (int)op.X, (int)op.Y);

            CrossCursor.Pos = CrossCursor.StorePos + d;
        }

        private void Wheel(CadMouse pointer, DrawContext dc, double x, double y, int delta)
        {
            if (CadKeyboard.IsCtrlKeyDown())
            {
                CursorLocked = false;

                double f = 1.0;

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

        #region RubberBand
        public void RubberBandSelect(Vector3d p0, Vector3d p1)
        {
            LastSelPoint = null;
            LastSelSegment = null;

            Vector3d minp = VectorExt.Min(p0, p1);
            Vector3d maxp = VectorExt.Max(p0, p1);

            DB.ForEachEditableFigure(
                (layer, fig) =>
                {
                    SelectIfContactRect(minp, maxp, layer, fig);
                });
        }

        public void SelectIfContactRect(Vector3d minp, Vector3d maxp, CadLayer layer, CadFigure fig)
        {
            for (int i = 0; i < fig.PointCount; i++)
            {
                Vector3d p = DC.WorldPointToDevPoint(fig.PointList[i].vector);

                if (CadUtil.IsInRect2D(minp, maxp, p))
                {
                    fig.SelectPointAt(i, true);
                }
            }
            return;
        }
        #endregion

        private void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            switch (State)
            {
                case States.SELECT:
                    break;

                case States.RUBBER_BAND_SELECT:
                    RubberBandSelect(RubberBandScrnPoint0, RubberBandScrnPoint1);

                    State = States.SELECT;
                    break;

                case States.START_DRAGING_POINTS:
                case States.DRAGING_POINTS:

                    //mPointSearcher.SetIgnoreList(null);
                    //mSegSearcher.SetIgnoreList(null);
                    //mSegSearcher.SetIgnoreSeg(null);

                    if (State == States.DRAGING_POINTS)
                    {
                        EndEdit();
                    }

                    State = States.SELECT;
                    break;
            }

            UpdateObjectTree(false);

            OffsetScreen = default;
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

            if (mInteractCtrl.IsActive)
            {
                foreach (Vector3d v in mInteractCtrl.PointList)
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


            //si.SnapPoint = dc.DevPointToWorldPoint(si.Cursor.Pos);
            si.SnapPoint = mGridding.MatchW;

            //HighlightPointList.Add(new HighlightPointListItem(si.SnapPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

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

            mSnapInfo = si;
        }

        private void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (State == States.DRAGING_VIEW_ORG)
            {
                ViewOrgDrag(pointer, dc, x, y);
                return;
            }

            if (CursorLocked)
            {
                x = CrossCursor.Pos.X;
                y = CrossCursor.Pos.Y;
            }

            Vector3d pixp = new Vector3d(x, y, 0) - OffsetScreen;
            Vector3d cp = dc.DevPointToWorldPoint(pixp);

            if (State == States.START_DRAGING_POINTS)
            {
                //
                // 選択時に思わずずらしてしまうことを防ぐため、
                // 最初だけある程度ずらさないと移動しないようにする
                //
                CadVertex v = CadVertex.Create(x, y, 0);
                double d = (RawDownPoint - v).Norm();

                if (d > SettingsHolder.Settings.InitialMoveLimit)
                {
                    State = States.DRAGING_POINTS;
                    StartEdit();
                }
            }

            RubberBandScrnPoint1 = pixp;

            CrossCursor.Pos = pixp;
            SnapPoint = cp;

            if (!CursorLocked)
            {
                SnapCursor(dc);
            }

            if (State == States.DRAGING_POINTS)
            {
                Vector3d p0 = dc.DevPointToWorldPoint(MoveOrgScrnPoint);
                Vector3d p1 = dc.DevPointToWorldPoint(CrossCursor.Pos);

                //p0.dump("p0");
                //p1.dump("p1");

                Vector3d delta = p1 - p0;

                MoveSelectedPoints(dc, new MoveInfo(p0, p1, MoveOrgScrnPoint, CrossCursor.Pos));

                ObjDownPoint = StoredObjDownPoint + delta;
            }

            Callback.CursorPosChanged(this, SnapPoint, CursorType.TRACKING);
            Callback.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            MouseMove(pointer, dc, x, y);
        }

        private void SetPointInCreating(DrawContext dc, CadVertex p)
        {
            FigureCreator.AddPointInCreating(dc, p);

            FigCreator.State state = FigureCreator.GetCreateState();

            if (state == FigCreator.State.FULL)
            {
                FigureCreator.EndCreate(dc);

                CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);

                NextState();
            }
            else if (state == FigCreator.State.ENOUGH)
            {
                CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);
            }
            else if (state == FigCreator.State.WAIT_NEXT_POINT)
            {
                CadOpe ope = new CadOpeAddPoint(
                    CurrentLayer.ID,
                    FigureCreator.Figure.ID,
                    FigureCreator.Figure.PointCount - 1,
                    ref p
                    );

                HistoryMan.foward(ope);
            }
        }

        private void SetPointInMeasuring(DrawContext dc, CadVertex p)
        {
            MeasureFigureCreator.AddPointInCreating(dc, p);
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

        public void CursorLock()
        {
            CursorLocked = true;
        }

        public void CursorUnlock()
        {
            CursorLocked = false;
        }

        public Vector3d GetCursorPos()
        {
            return SnapPoint;
        }

        public void SetCursorWoldPos(Vector3d v)
        {
            SnapPoint = v;
            CrossCursor.Pos = DC.WorldPointToDevPoint(SnapPoint);

            Callback.CursorPosChanged(this, SnapPoint, CursorType.TRACKING);
        }


        public Vector3d GetLastDownPoint()
        {
            return LastDownPoint;
        }

        public void SetLastDownPoint(Vector3d v)
        {
            LastDownPoint = v;
            Callback.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
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
}
