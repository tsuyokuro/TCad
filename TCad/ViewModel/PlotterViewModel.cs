//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TCad.Controls;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad.ViewModel;

public class PlotterViewModel : IPlotterViewModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected PlotterController mController;
    public PlotterController Controller
    {
        get => mController;
    }

    protected ICadMainWindow mMainWindow;
    public ICadMainWindow MainWindow
    {
        get => mMainWindow;
    }

    public CursorPosViewModel CursorPosVM = new();

    public ObjectTreeViewModel ObjTreeVM;

    public LayerListViewModel LayerListVM;

    public ViewManager mViewManager;
    public ViewManager ViewManager
    {
        get => mViewManager;
    }

    private string mCaptionFileName = "";
    public string CaptionFileName
    {
        set
        {
            mCaptionFileName = value ?? "----";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CaptionFileName)));
        }

        get => mCaptionFileName;
    }

    private SelectModes mSelectMode = SelectModes.OBJECT;
    public SelectModes SelectMode
    {
        set
        {
            mSelectMode = value;
            mController.SelectMode = mSelectMode;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectMode)));
        }

        get => mSelectMode;
    }


    private CadFigure.Types mCreatingFigureType = CadFigure.Types.NONE;
    public CadFigure.Types CreatingFigureType
    {
        set
        {
            bool changed = ChangeFigureType(value);

            if (changed)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatingFigureType)));
            }
        }

        get => mCreatingFigureType;
    }

    private MeasureModes mMeasureMode = MeasureModes.NONE;
    public MeasureModes MeasureMode
    {
        set
        {
            bool changed = ChangeMeasuerType(value);

            if (changed)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasureMode)));
            }
        }

        get => mMeasureMode;
    }

    public DrawContext DC
    {
        get => mController?.DC;
    }

    private SettingsVeiwModel SettingsVM;
    public SettingsVeiwModel Settings
    {
        get => SettingsVM;
    }

    private Window mEditorWindow;

    private MoveKeyHandler mMoveKeyHandler;

    public string CurrentFileName
    {
        get => mController.CurrentFileName;
        set
        {
            mController.CurrentFileName = value;
            CaptionFileName = value;
        }
    }

    public AutoCompleteTextBox CommandTextBox
    {
        get;
        private set;
    }

    public CurrentFigCommand CurrentFigCmd { get; set; }

    public SimpleCommand SimpleCmd{ get; set; }

    private CommandHandler mCommandHandler;

    public delegate void DrawModeChangeEventHandler(DrawModes mode);
    public event DrawModeChangeEventHandler DrawModeChanged;

    public PlotterViewModel(ICadMainWindow mainWindow)
    {
        DOut.plx("in");

        CurrentFigCmd = new(this);
        SimpleCmd = new(this);

        mController = new PlotterController(this);

        SettingsVM = new SettingsVeiwModel(this);

        ObjTreeVM = new ObjectTreeViewModel(this);

        LayerListVM = new LayerListViewModel(this);

        mViewManager = new ViewManager(this);

        mCommandHandler = new CommandHandler(this);

        mMainWindow = mainWindow;

        CurrentFileName = null;

        SelectMode = mController.SelectMode;
        CreatingFigureType = mController.CreatingFigType;

        mController.UpdateLayerList();

        mMoveKeyHandler = new MoveKeyHandler(Controller);

        DOut.plx("out");
    }


    #region handling IMainWindow

    public void OpenPopupMessage(string text, UITypes.MessageType messageType)
    {
        mMainWindow.OpenPopupMessage(text, messageType);
    }

    public void ClosePopupMessage()
    {
        mMainWindow.ClosePopupMessage();
    }
    #endregion handling IMainWindow

    public void ExecCommand(string cmd)
    {
        mCommandHandler.ExecCommand(cmd);
    }

    // Handle events from PlotterController
    #region Event From PlotterController

    public void StateChanged(StateChangedParam param)
    {
        if (CreatingFigureType != Controller.CreatingFigType)
        {
            CreatingFigureType = Controller.CreatingFigType;
        }

        if (MeasureMode != Controller.MeasureMode)
        {
            MeasureMode = Controller.MeasureMode;
        }

        if (param.Type == StateChangedType.SELECTION_CHANGED)
        {
            CurrentFigCmd.UpdateCanExecute();
        }
    }

    public void CursorPosChanged(vector3_t pt, Plotter.Controller.CursorType type)
    {
        if (type == Plotter.Controller.CursorType.TRACKING)
        {
            CursorPosVM.CursorPos = pt;
            CursorPosVM.CursorPos3 = pt - Controller.LastDownPoint;
        }
        else if (type == Plotter.Controller.CursorType.LAST_DOWN)
        {
            CursorPosVM.CursorPos2 = pt;
        }
    }

    public void CursorLocked(bool locked)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            mViewManager.PlotterView.CursorLocked(locked);
        }, true);
    }

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            mViewManager.PlotterView.ChangeMouseCursor(cursorType);
        }, true);
    }

    public List<string> HelpOfKey(string keyword)
    {
        return mCommandHandler.HelpOfKey(keyword);
    }

    public void LayerListChanged(LayerListInfo layerListInfo)
    {
        LayerListVM.LayerListChanged(layerListInfo);
    }

    public void UpdateTreeView(bool remakeTree)
    {
        ObjTreeVM?.UpdateTreeView(remakeTree);
    }

    public void SetTreeViewPos(int index)
    {
        ObjTreeVM.SetTreeViewPos(index);
    }

    public int FindTreeViewItemIndex(uint id)
    {
        return ObjTreeVM.FindTreeViewItemIndex(id);
    }

    public void ShowContextMenu(MenuInfo menuInfo, int x, int y)
    {
        mViewManager.PlotterView.ShowContextMenu(menuInfo, x, y);
    }
    #endregion Event From PlotterController

    #region Keyboard handling
    public bool OnKeyDown(object sender, KeyEventArgs e)
    {
        return mCommandHandler.OnKeyDown(sender, e);
    }

    public bool OnKeyUp(object sender, KeyEventArgs e)
    {
        return mCommandHandler.OnKeyUp(sender, e);
    }
    #endregion Keyboard handling

    public void SetWorldScale(vcompo_t scale)
    {
        mViewManager.SetWorldScale(scale);
    }

    public void EvalTextCommand(string s)
    {
        mController.EvalTextCommand(s);
    }

    private bool ChangeFigureType(CadFigure.Types newType)
    {
        var prev = mCreatingFigureType;

        if (mCreatingFigureType == newType)
        {
            // 現在のタイプを再度選択したら解除する
            if (mCreatingFigureType != CadFigure.Types.NONE)
            {
                mController.Cancel();
                Redraw();
                return true;
            }
        }

        mCreatingFigureType = newType;

        if (newType != CadFigure.Types.NONE)
        {
            MeasureMode = MeasureModes.NONE;
            mController.StartCreateFigure(newType);

            Redraw();

            return prev != mCreatingFigureType;
        }

        return prev != mCreatingFigureType;
    }

    private bool ChangeMeasuerType(MeasureModes newType)
    {
        var prev = mMeasureMode;

        if (mMeasureMode == newType)
        {
            // 現在のタイプを再度選択したら解除する
            mMeasureMode = MeasureModes.NONE;
        }
        else
        {
            mMeasureMode = newType;
        }

        if (mMeasureMode != MeasureModes.NONE)
        {
            CreatingFigureType = CadFigure.Types.NONE;
            mController.StartMeasure(newType);
        }
        else if (prev != MeasureModes.NONE)
        {
            mController.EndMeasure();
            Redraw();
        }

        return true;
    }



    public void SetupTextCommandView(AutoCompleteTextBox textBox)
    {
        CommandTextBox = textBox;
        CommandTextBox.CandidateList.Clear();
        CommandTextBox.CandidateList.AddRange(Controller.ScriptEnv.AutoCompleteList);
        CommandTextBox.Determined += EvalTextCommand;
    }

    public void Open()
    {
        DOut.plx("in");

        Settings.Load();
        mViewManager.SetupViews();

        DOut.plx("out");
    }

    public void Close()
    {
        Settings.Save();

        if (mEditorWindow != null)
        {
            mEditorWindow.Close();
            mEditorWindow = null;
        }

        CommandTextBox.Determined -= EvalTextCommand;

        GDIToolManager.Instance.Dispose();
    }

    public void DrawModeUpdated(DrawModes mode)
    {
        mViewManager.DrawModeUpdated(mode);
        DrawModeChanged?.Invoke(mode);
    }

    public void Redraw()
    {
        ThreadUtil.RunOnMainThread(mController.Redraw, true);
    }
}
