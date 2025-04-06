using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TCad.Controls;
using System;

namespace TCad.ViewModel;

public class PlotterViewModel : IPlotterViewModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected PlotterController Controller_;
    public PlotterController Controller
    {
        get => Controller_;
    }

    protected ICadMainWindow mMainWindow;
    public ICadMainWindow MainWindow
    {
        get => mMainWindow;
    }

    public CursorPosViewModel CursorPosVM
    {
        get;
        set;
    } = new();

    public ObjectTreeViewModel ObjTreeVM;


    public ICadObjectTree ObjectTree
    {
        get => ObjTreeVM.ObjectTree;
        set => ObjTreeVM.ObjectTree = value;
    }

    public LayerListViewModel LayerListVM
    {
        get; set;
    }

    private ViewManager ViewManager_;
    public ViewManager ViewManager
    {
        get => ViewManager_;
    }

    private string CaptionFileName_ = "";
    public string CaptionFileName
    {
        set
        {
            CaptionFileName_ = value ?? "----";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CaptionFileName)));
        }

        get => CaptionFileName_;
    }

    private SelectModes SelectMode_ = SelectModes.OBJECT;
    public SelectModes SelectMode
    {
        set
        {
            SelectMode_ = value;
            Controller_.SelectMode = SelectMode_;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectMode)));
        }

        get => SelectMode_;
    }


    private CadFigure.Types CurrentFigureType_ = CadFigure.Types.NONE;
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

        get => CurrentFigureType_;
    }

    private MeasureModes MeasureMode_ = MeasureModes.NONE;
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

        get => MeasureMode_;
    }

    public DrawContext DC
    {
        get => Controller_?.DC;
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
        get => Controller_.CurrentFileName;
        set
        {
            Controller_.CurrentFileName = value;
            CaptionFileName = value;
        }
    }

    public IAutoCompleteTextBox CommandTextBox
    {
        get;
        private set;
    }

    public CurrentFigCommand CurrentFigCmd { get; set; }

    public SimpleCommand SimpleCmd{ get; set; }

    private CommandHandler mCommandHandler;

    public PlotterViewModel(ICadMainWindow mainWindow)
    {
        Log.plx("in");

        CurrentFigCmd = new(this);
        SimpleCmd = new(this);

        Controller_ = new PlotterController(this);

        SettingsVM = new SettingsVeiwModel(this);

        ObjTreeVM = new ObjectTreeViewModel(this);

        LayerListVM = new LayerListViewModel(this);

        ViewManager_ = new ViewManager(this);

        mCommandHandler = new CommandHandler(this);

        mMainWindow = mainWindow;

        CurrentFileName = null;

        SelectMode = Controller_.SelectMode;
        CreatingFigureType = Controller_.CreatingFigType;

        Controller_.UpdateLayerList();

        mMoveKeyHandler = new MoveKeyHandler(Controller);

        Log.plx("out");
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
            CursorPosVM.CursorPos3 = pt - Controller.Input.LastDownPoint;
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
            ViewManager_.PlotterView.CursorLocked(locked);
        }, true);
    }

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            ViewManager_.PlotterView.ChangeMouseCursor(cursorType);
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
        ViewManager_.PlotterView.ShowContextMenu(menuInfo, x, y);
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
        ViewManager_.SetWorldScale(scale);
    }

    public void EvalTextCommand(string s)
    {
        Controller_.EvalTextCommand(s);
    }

    private bool ChangeFigureType(CadFigure.Types newType)
    {
        var prev = CurrentFigureType_;

        if (CurrentFigureType_ == newType)
        {
            // 現在のタイプを再度選択したら解除する
            if (CurrentFigureType_ != CadFigure.Types.NONE)
            {
                Controller_.Cancel();
                Redraw();
                return true;
            }
        }

        CurrentFigureType_ = newType;

        if (newType != CadFigure.Types.NONE)
        {
            MeasureMode = MeasureModes.NONE;
            Controller_.StartCreateFigure(newType);

            Redraw();

            return prev != CurrentFigureType_;
        }

        return prev != CurrentFigureType_;
    }

    private bool ChangeMeasuerType(MeasureModes newType)
    {
        var prev = MeasureMode_;

        if (MeasureMode_ == newType)
        {
            // 現在のタイプを再度選択したら解除する
            MeasureMode_ = MeasureModes.NONE;
        }
        else
        {
            MeasureMode_ = newType;
        }

        if (MeasureMode_ != MeasureModes.NONE)
        {
            CreatingFigureType = CadFigure.Types.NONE;
            Controller_.StartMeasure(newType);
        }
        else if (prev != MeasureModes.NONE)
        {
            Controller_.EndMeasure();
            Redraw();
        }

        return true;
    }


    public void Open()
    {
        Log.plx("in");

        Settings.Load();
        ViewManager_.SetupViews();

        Log.plx("out");
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

    public void DrawModeChanged(DrawModes mode)
    {
        ViewManager_.DrawModeChanged(mode);
        //OnDrawModeChanged?.Invoke(mode);
    }

    public void Redraw()
    {
        ThreadUtil.RunOnMainThread(Controller_.Drawer.Redraw, true);
    }

    public void AttachCommandView(IAutoCompleteTextBox textBox)
    {
        CommandTextBox = textBox;
        CommandTextBox.CandidateList.Clear();
        CommandTextBox.CandidateList.AddRange(Controller.ScriptEnv.AutoCompleteList);
        CommandTextBox.Determined += EvalTextCommand;
    }
}
