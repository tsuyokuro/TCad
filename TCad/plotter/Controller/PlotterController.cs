using CadDataTypes;
using Plotter.Controller.TaskRunner;
using Plotter.Settings;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public enum States
        {
            SELECT,
            RUBBER_BAND_SELECT,
            START_DRAGING_POINTS,
            DRAGING_POINTS,
            DRAGING_VIEW_ORG,
            START_CREATE,
            CREATING,
            MEASURING,
        }

        private CadObjectDB mDB = new CadObjectDB();
        public CadObjectDB DB => mDB;

        private States mState = States.SELECT;
        public States State
        {
            private set
            {
                mState = value;

                if (mInteractCtrl.IsActive)
                {
                    mInteractCtrl.Cancel();
                }
            }

            get => mState;
        }

        private States mBackState;


        private PaperPageSize mPageSize = new PaperPageSize(PaperKind.A4, false);
        public PaperPageSize PageSize
        {
            get => mPageSize;
            set => mPageSize = value;
        }

        public SelectModes SelectMode
        {
            set;
            get;
        } = SelectModes.OBJECT;

        public CadLayer CurrentLayer
        {
            get => mDB.CurrentLayer;

            set
            {
                mDB.CurrentLayer = value;
                UpdateObjectTree(true);
            }
        }


        CadFigure.Types mCreatingFigType = CadFigure.Types.NONE;

        public CadFigure.Types CreatingFigType
        {
            private set => mCreatingFigType = value;
            get => mCreatingFigType;
        }

        private MeasureModes mMeasureMode = MeasureModes.NONE;
        public MeasureModes MeasureMode
        {
            get => mMeasureMode;
        }

        private CadFigure.Creator mFigureCreator = null;
        public CadFigure.Creator FigureCreator
        {
            get => mFigureCreator;
        }

        public CadFigure.Creator MeasureFigureCreator = null;


        public HistoryManager HistoryMan = null;

        private List<CadFigure> EditFigList = new List<CadFigure>();

        public bool ContinueCreate { set; get; } = true;


        public PlotterCallback Callback = new PlotterCallback();


        public List<CadFigure> TempFigureList = new List<CadFigure>();

        private DrawContext mDC;
        public DrawContext DC
        {
            set => mDC = value; 
            get => mDC;
        }

        public ScriptEnvironment ScriptEnv;

        public PlotterTaskRunner mPlotterTaskRunner;

        private Vector3dList ExtendSnapPointList = new Vector3dList(20);

        private ContextMenuManager mContextMenuMan;
        public ContextMenuManager ContextMenuMan
        {
            get => mContextMenuMan;
        }

        public PlotterController()
        {
            CadLayer layer = mDB.NewLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            HistoryMan = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);

            mContextMenuMan = new ContextMenuManager(this);

            mPlotterTaskRunner = new PlotterTaskRunner(this);

            ObjDownPoint = VectorExt.InvalidVector3d;

            InitHid();
        }

        #region ObjectTree handling
        public void UpdateObjectTree(bool remakeTree)
        {
            Callback.UpdateObjectTree(remakeTree);
        }

        public void SetObjectTreePos(int index)
        {
            Callback.SetObjectTreePos(index);
        }

        public int FindObjectTreeItem(uint id)
        {
            return Callback.FindObjectTreeItemIndex(id);
        }
        #endregion ObjectTree handling


        #region Notify
        public void UpdateLayerList()
        {
            Callback.LayerListChanged(this, GetLayerListInfo());
        }

        private LayerListInfo GetLayerListInfo()
        {
            LayerListInfo layerInfo = default(LayerListInfo);
            layerInfo.LayerList = mDB.LayerList;
            layerInfo.CurrentID = CurrentLayer.ID;

            return layerInfo;
        }

        private void NotifyStateChange()
        {
            PlotterStateInfo si = default(PlotterStateInfo);
            si.set(this);

            Callback.StateChanged(this, si);
        }
        #endregion Notify

        #region Start and End creating figure
        public void StartCreateFigure(CadFigure.Types type)
        {
            State = States.START_CREATE;
            CreatingFigType = type;
        }

        public void EndCreateFigure()
        {
            if (mFigureCreator != null)
            {
                mFigureCreator.EndCreate(DC);
                mFigureCreator = null;
            }

            NextState();
        }

        public void CloseFigure()
        {
            if (FigureCreator != null)
            {
                FigureCreator.Figure.IsLoop = true;

                CadOpe ope = new CadOpeSetClose(CurrentLayer.ID, FigureCreator.Figure.ID, true);
                HistoryMan.foward(ope);

                FigureCreator.EndCreate(DC);
            }

            NextState();
        }

        private void NextState()
        {
            if (State == States.CREATING)
            {
                if (SettingsHolder.Settings.ContinueCreateFigure)
                {
                    mFigureCreator = null;
                    StartCreateFigure(CreatingFigType);
                    UpdateObjectTree(true);
                }
                else
                {
                    mFigureCreator = null;
                    CreatingFigType = CadFigure.Types.NONE;
                    State = States.SELECT;

                    UpdateObjectTree(true);
                    NotifyStateChange();
                }
            }
        }

        public void StartMeasure(MeasureModes mode)
        {
            State = States.MEASURING;
            mMeasureMode = mode;
            MeasureFigureCreator =
                CadFigure.Creator.Get(
                    CadFigure.Types.POLY_LINES,
                    CadFigure.Create(CadFigure.Types.POLY_LINES)
                    );
        }

        public void EndMeasure()
        {
            State = States.SELECT;
            mMeasureMode = MeasureModes.NONE;
            MeasureFigureCreator = null;
        }
        #endregion Start and End creating figure

        #region UnDo ReDo
        public void Undo()
        {
            ClearSelection();
            HistoryMan.undo();
            UpdateObjectTree(true);
            UpdateLayerList();
        }

        public void Redo()
        {
            ClearSelection();
            HistoryMan.redo();
            UpdateObjectTree(true);
            UpdateLayerList();
        }
        #endregion UnDo ReDo

        #region Getting selection
        public bool HasSelect()
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList )
                {
                    if (fig.HasSelectedPointInclueChild())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<CadFigure> GetSelectedFigureList()
        {
            List<CadFigure> figList = new List<CadFigure>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        figList.Add(fig);
                    }
                });
            }

            return figList;
        }

        public List<CadFigure> GetSelectedRootFigureList()
        {
            List<CadFigure> figList = new List<CadFigure>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachRootFig(fig =>
                {
                    if (fig.HasSelectedPointInclueChild())
                    {
                        figList.Add(fig);
                    }
                });
            }

            return figList;
        }
        #endregion Getting selection

        public void SetDB(CadObjectDB db)
        {
            mDB = db;

            HistoryMan = new HistoryManager(mDB);

            UpdateLayerList();

            UpdateObjectTree(true);

            Redraw();
        }

        public void SetCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
            UpdateObjectTree(true);
        }

        public void TextCommand(string s)
        {
            //ScriptEnv.ExecuteCommandSync(s);
            ScriptEnv.ExecuteCommandAsync(s);
        }

        public void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            PlotterPrinter printer = new PlotterPrinter();
            printer.PrintPage(this, printerGraphics, pageSize, deviceSize);
        }
    }
}
