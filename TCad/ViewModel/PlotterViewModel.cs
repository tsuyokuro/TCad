using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using TCad.Controls;
using System.Drawing.Printing;
using Plotter.Controller;
using TCad.Dialogs;
using System.Text.RegularExpressions;
using Plotter.svg;
using System.Xml.Linq;
using Plotter;
using Plotter.Settings;
using System.IO;
using TCad.ScriptEditor;

namespace TCad.ViewModel
{

    public class PlotterViewModel : ViewModelContext, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class KeyAction
        {
            public Action Down;
            public Action Up;
            public string Description;

            public KeyAction(Action down, Action up, string description = null)
            {
                Down = down;
                Up = up;
                Description = description;
            }
        }

        private Dictionary<string, Action> CommandMap;

        private Dictionary<string, KeyAction> KeyMap;

        public CursorPosViewModel CursorPosVM = new CursorPosViewModel();

        public ObjectTreeViewModel ObjTreeVM;

        public LayerListViewModel LayerListVM;

        public ViewManager mViewManager;

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

        private string mCurrentFileName = null;
        public string CurrentFileName
        {
            get => mCurrentFileName;

            private set
            {
                mCurrentFileName = value;
                ChangeCurrentFileName(mCurrentFileName);
            }
        }

        public PlotterViewModel(ICadMainWindow mainWindow)
        {
            mController = new PlotterController();

            SettingsVM = new SettingsVeiwModel(this);

            ObjTreeVM = new ObjectTreeViewModel(this);

            LayerListVM = new LayerListViewModel(this);

            mViewManager = new ViewManager(this);

            mMainWindow = mainWindow;

            InitCommandMap();
            InitKeyMap();

            SelectMode = mController.SelectMode;
            CreatingFigureType = mController.CreatingFigType;

            mController.Callback.StateChanged = StateChanged;

            mController.Callback.CursorPosChanged = CursorPosChanged;

            mController.Callback.OpenPopupMessage = OpenPopupMessage;

            mController.Callback.ClosePopupMessage = ClosePopupMessage;

            mController.Callback.CursorLocked = CursorLocked;

            mController.Callback.ChangeMouseCursor = ChangeMouseCursor;

            mController.Callback.HelpOfKey = HelpOfKey;


            mController.UpdateLayerList();

            mMoveKeyHandler = new MoveKeyHandler(Controller);
        }


        #region handling IMainWindow
        private void ChangeCurrentFileName(string fname)
        {
            if (fname != null)
            {
                mMainWindow.SetCurrentFileName(fname);
            }
            else
            {
                mMainWindow.SetCurrentFileName("");
            }
        }

        private void OpenPopupMessage(string text, PlotterCallback.MessageType messageType)
        {
            mMainWindow.OpenPopupMessage(text, messageType);
        }

        private void ClosePopupMessage()
        {
            mMainWindow.ClosePopupMessage();
        }
#endregion handling IMainWindow

#region Maps
        private void InitCommandMap()
        {
            CommandMap = new Dictionary<string, Action>{
                { "new_doc", NewDocument },
                { "load", Load },
                { "save",Save },
                { "save_as",SaveAs },
                { "print",StartPrint },
                { "page_setting",PageSetting },
                { "doc_setting",DocSetting },
                { "undo",Undo },
                { "redo",Redo },
                { "copy",Copy },
                { "paste",Paste },
                { "separate",SeparateFigure },
                { "bond",BondFigure },
                { "to_bezier",ToBezier },
                { "cut_segment",CutSegment },
                { "ins_point",InsPoint },
                { "to_loop", ToLoop },
                { "to_unloop", ToUnloop },
                { "clear_layer", ClearLayer },
                { "flip_with_vector", FlipWithVector },
                { "flip_and_copy_with_vector", FlipAndCopyWithVector },
                { "flip_normal", FlipNormal },
                { "rotate_with_point", RotateWithPoint },
                { "grid_settings", GridSettings },
                { "add_layer", AddLayer },
                { "remove_layer", RemoveLayer },
                { "centroid", AddCentroid },
                { "select_all", SelectAll },
                { "snap_settings", SnapSettings },
                { "show_editor", ShowEditor },
                { "export_svg", ExportSVG },
                { "obj_order_down", ObjOrderDown },
                { "obj_order_up", ObjOrderUp },
                { "obj_order_bottom", ObjOrderBottom },
                { "obj_order_top", ObjOrderTop },
                { "reset_camera", ResetCamera },
                { "cut_mesh_with_vector", CutMeshWithVector },
                { "print_setting", PrintSettings },
            };
        }

        private void InitKeyMap()
        {
            KeyMap = new Dictionary<string, KeyAction>
            {
                { "ctrl+z", new KeyAction(Undo , null, "Undo")},
                { "ctrl+y", new KeyAction(Redo , null, "Rendo")},
                { "ctrl+c", new KeyAction(Copy , null, "Copy")},
                { "ctrl+insert", new KeyAction(Copy , null, "Copy")},
                { "ctrl+v", new KeyAction(Paste ,null, "Paste")},
                { "shift+insert", new KeyAction(Paste , null, "Paste")},
                { "delete", new KeyAction(Remove , null)},
                { "ctrl+s", new KeyAction(Save , null, "Save")},
                { "ctrl+a", new KeyAction(SelectAll , null, "Select All")},
                { "escape", new KeyAction(Cancel , null)},
                { "ctrl+p", new KeyAction(InsPoint , null, "Inser Point")},
                { "f3", new KeyAction(SearchNearPoint , null, "Search near Point")},
                { "f2", new KeyAction(CursorLock , null, "Lock Cursor")},
                { "left", new KeyAction(MoveKeyDown, MoveKeyUp)},
                { "right", new KeyAction(MoveKeyDown, MoveKeyUp)},
                { "up", new KeyAction(MoveKeyDown, MoveKeyUp)},
                { "down", new KeyAction(MoveKeyDown, MoveKeyUp)},
                { "m", new KeyAction(AddMark, null, " Add snap point")},
                { "ctrl+m", new KeyAction(CleanMark, null, " Clear snap points")},
            };
        }

        private string GetDisplayKeyString(string s)
        {
            string[] ss = s.Split('+');

            string t = ss[0];
            string p = Char.ToUpper(t[0]) + t.Substring(1);

            string ret = p;

            for (int i=1; i<ss.Length; i++)
            {
                t = ss[i];
                p = Char.ToUpper(t[0]) + t.Substring(1);
                ret += "+" + p;
            }

            return ret;
        }

        public List<string> HelpOfKey(string keyword)
        {
            List<string> ret = new List<string>();

            if (keyword == null)
            {
                foreach (String s in KeyMap.Keys)
                {
                    KeyAction k = KeyMap[s];

                    if (k.Description == null) continue;

                    string t = GetDisplayKeyString(s);

                    ret.Add(AnsiEsc.BGreen + t + AnsiEsc.Reset + " " + k.Description);
                }

                return ret;
            }

            Regex re = new Regex(keyword, RegexOptions.IgnoreCase);

            foreach (String s in KeyMap.Keys)
            {
                KeyAction k = KeyMap[s];

                if (k.Description == null) continue;

                if (re.Match(k.Description).Success)
                {
                    string t = GetDisplayKeyString(s);
                    ret.Add(AnsiEsc.BGreen + t + AnsiEsc.Reset + " " + k.Description);
                }
            }

            return ret;
        }

        public void ExecCommand(string cmd)
        {
            Action action;
            CommandMap.TryGetValue(cmd, out action);

            action?.Invoke();
        }

        public bool ExecShortcutKey(string keyCmd, bool down)
        {
            KeyAction ka;
            KeyMap.TryGetValue(keyCmd, out ka);

            if (down)
            {
                ka?.Down?.Invoke();
            }
            else
            {
                ka?.Up?.Invoke();
            }

            return true;
        }

#endregion


        // Actions
#region Actions
        public void Undo()
        {
            mController.Undo();
            Redraw();
        }

        public void Redo()
        {
            mController.Redo();
            Redraw();
        }

        public void Remove()
        {
            mController.Remove();
            Redraw();
        }

        public void SeparateFigure()
        {
            mController.SeparateFigures();
            Redraw();
        }

        public void BondFigure()
        {
            mController.BondFigures();
            Redraw();
        }

        public void ToBezier()
        {
            mController.ToBezier();
            Redraw();
        }

        public void CutSegment()
        {
            mController.CutSegment();
            Redraw();
        }

        public void InsPoint()
        {
            mController.InsPoint();
            Redraw();
        }

        public void ToLoop()
        {
            mController.SetLoop(true);
            Redraw();
        }

        public void ToUnloop()
        {
            mController.SetLoop(false);
            Redraw();
        }

        public void FlipWithVector()
        {
            mController.FlipWithVector();
        }

        public void FlipAndCopyWithVector()
        {
            mController.FlipAndCopyWithVector();
        }

        public void CutMeshWithVector()
        {
            mController.CutMeshWithVector();
        }

        public void RotateWithPoint()
        {
            mController.RotateWithPoint();
        }

        public void FlipNormal()
        {
            mController.FlipNormal();
            Redraw();
        }

        public void ClearLayer()
        {
            mController.ClearLayer(0);
            Redraw();
        }

        public void Copy()
        {
            mController.Copy();
            Redraw();
        }

        public void Paste()
        {
            mController.Paste();
            Redraw();
        }

        public void NewDocument()
        {
            CurrentFileName = null;

            mViewManager.SetWorldScale(1.0);

            mController.ClearAll();
            Redraw();
        }

        private bool IsVaridDir(string path)
        {
            if (path == null)
            {
                return false;
            }

            if (path.Length == 0)
            {
                return false;
            }

            return Directory.Exists(path);
        }

        public void Load()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            
            if (IsVaridDir(SettingsHolder.Settings.LastDataDir))
            {
                ofd.InitialDirectory = SettingsHolder.Settings.LastDataDir;
            }
            
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsHolder.Settings.LastDataDir = Path.GetDirectoryName(ofd.FileName);

                CadFileAccessor.LoadFile(ofd.FileName, this);
                CurrentFileName = ofd.FileName;
            }
        }

        public void Save()
        {
            if (CurrentFileName != null)
            {
                CadFileAccessor.SaveFile(CurrentFileName, this);
                return;
            }

            SaveAs();
        }

        public void SaveAs()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsHolder.Settings.LastDataDir = Path.GetDirectoryName(sfd.FileName);

                CadFileAccessor.SaveFile(sfd.FileName, this);
                CurrentFileName = sfd.FileName;
            }
        }

        public void GridSettings()
        {
            GridSettingsDialog dlg = new GridSettingsDialog();

            dlg.GridSize = Settings.GridSize;

            dlg.Owner = mMainWindow.GetWindow();

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                Settings.GridSize = dlg.GridSize;
                Redraw();
            }
        }

        public void SnapSettings()
        {
            SnapSettingsDialog dlg = new SnapSettingsDialog();

            dlg.Owner = mMainWindow.GetWindow();

            dlg.PointSnapRange = Settings.PointSnapRange;
            dlg.LineSnapRange = Settings.LineSnapRange;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                Settings.PointSnapRange = dlg.PointSnapRange;
                Settings.LineSnapRange = dlg.LineSnapRange;

                Redraw();
            }
        }

        public void PrintSettings()
        {
            PrintSettingsDialog dlg = new PrintSettingsDialog();

            dlg.Owner = mMainWindow.GetWindow();

            dlg.PrintWithBitmap = Settings.PrintWithBitmap;
            dlg.MagnificationBitmapPrinting = Settings.MagnificationBitmapPrinting;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                Settings.PrintWithBitmap = dlg.PrintWithBitmap;
                Settings.MagnificationBitmapPrinting = dlg.MagnificationBitmapPrinting;
            }
        }

        public void ShowEditor()
        {
            if (mEditorWindow == null)
            {
                mEditorWindow = new EditorWindow(mController.ScriptEnv);
                mEditorWindow.Owner = mMainWindow.GetWindow();
                mEditorWindow.Show();

                mEditorWindow.Closed += delegate
                {
                    mEditorWindow = null;
                };
            }
            else
            {
                mEditorWindow.Activate();
            }
        }

        public void ExportSVG()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<CadFigure> figList = Controller.DB.GetSelectedFigList();

                SvgExporter exporter = new SvgExporter();

                XDocument doc = exporter.ToSvg(figList, Controller.DC,
                    Controller.PageSize.Width, Controller.PageSize.Height);

                try
                {
                    doc.Save(sfd.FileName);
                    ItConsole.println("Success Export SVG: " + sfd.FileName);

                    System.Diagnostics.Process.Start(
                        "EXPLORER.EXE", $@"/select,""{sfd.FileName}""");
                }
                catch (Exception e)
                {
                    ItConsole.printError(e.Message);
                }
            }
        }

        public void ObjOrderDown()
        {
            mController.ObjOrderDown();
        }

        public void ObjOrderUp()
        {
            mController.ObjOrderUp();
        }

        public void ObjOrderBottom()
        {
            mController.ObjOrderBottom();
        }

        public void ObjOrderTop()
        {
            mController.ObjOrderTop();
        }

        public void ResetCamera()
        {
            mViewManager.ResetCamera();
            Redraw();
        }

        public void AddLayer()
        {
            mController.AddLayer(null);
            Redraw();
        }

        public void RemoveLayer()
        {
            mController.RemoveLayer(mController.CurrentLayer.ID);
            Redraw();
        }

        public void AddCentroid()
        {
            mController.AddCentroid();
            Redraw();
        }

        public void SelectAll()
        {
            mController.SelectAllInCurrentLayer();
            Redraw();
        }

        public void Cancel()
        {
            mController.Cancel();
            Redraw();
        }

        public void SearchNearPoint()
        {
            mController.MoveCursorToNearPoint(mViewManager.DrawContext);
            Redraw();
        }

        public void CursorLock()
        {
            mController.CursorLock();
        }

        public void MoveKeyDown()
        {
            mMoveKeyHandler.MoveKeyDown();
        }

        public void MoveKeyUp()
        {
            mMoveKeyHandler.MoveKeyUp();
        }


        public void AddMark()
        {
            mController.AddExtendSnapPoint();
            Redraw();
        }

        public void CleanMark()
        {
            mController.ClearExtendSnapPointList();
            Redraw();
        }

#endregion

        // Handle events from PlotterController
#region Event From PlotterController

        public void StateChanged(PlotterController sender, PlotterStateInfo si)
        {
            if (CreatingFigureType != si.CreatingFigureType)
            {
                CreatingFigureType = si.CreatingFigureType;
            }

            if (MeasureMode != si.MeasureMode)
            {
                MeasureMode = si.MeasureMode;
            }
        }

        private void CursorPosChanged(PlotterController sender, Vector3d pt, Plotter.Controller.CursorType type)
        {
            if (type == Plotter.Controller.CursorType.TRACKING)
            {
                CursorPosVM.CursorPos = pt;
            }
            else if (type == Plotter.Controller.CursorType.LAST_DOWN)
            {
                CursorPosVM.CursorPos2 = pt;
            }
        }

        private void CursorLocked(bool locked)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                mViewManager.PlotterView.CursorLocked(locked);
            }, true);
        }

        private void ChangeMouseCursor(PlotterCallback.MouseCursorType cursorType)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                mViewManager.PlotterView.ChangeMouseCursor(cursorType);
            }, true);
        }

#endregion Event From PlotterController


        // Keyboard handling
#region Keyboard handling
        private string GetModifyerKeysString()
        {
            ModifierKeys modifierKeys = Keyboard.Modifiers;

            string s = "";

            if ((modifierKeys & ModifierKeys.Control) != ModifierKeys.None)
            {
                s += "ctrl+";
            }

            if ((modifierKeys & ModifierKeys.Shift) != ModifierKeys.None)
            {
                s += "shift+";
            }

            if ((modifierKeys & ModifierKeys.Alt) != ModifierKeys.None)
            {
                s += "alt+";
            }

            return s;
        }

        private string KeyString(KeyEventArgs e)
        {
            string ks = GetModifyerKeysString();

            ks += e.Key.ToString().ToLower();

            return ks;
        }


        public bool OnKeyDown(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return ExecShortcutKey(ks, true);
        }

        public bool OnKeyUp(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return ExecShortcutKey(ks, false);
        }
#endregion Keyboard handling


#region print
        public void StartPrint()
        {
            PrintDocument pd =
                new PrintDocument();

            PageSettings storePageSettings = pd.DefaultPageSettings;

            pd.DefaultPageSettings.PaperSize = Controller.PageSize.GetPaperSize();

            pd.DefaultPageSettings.Landscape = Controller.PageSize.mLandscape;

            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            pd.PrintPage += PrintPage;

            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();

            pdlg.Document = pd;

            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pd.Print();
            }

            pd.DefaultPageSettings = storePageSettings;
        }

        private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            CadSize2D deviceSize = new CadSize2D(e.PageBounds.Size.Width, e.PageBounds.Size.Height);
            CadSize2D pageSize = new CadSize2D(Controller.PageSize.Width, Controller.PageSize.Height);

            Controller.PrintPage(g, pageSize, deviceSize);
        }
#endregion print

        public void PageSetting()
        {
            System.Windows.Forms.PageSetupDialog pageDlg = new System.Windows.Forms.PageSetupDialog();

            PageSettings pageSettings = new PageSettings();

            pageSettings.PaperSize = Controller.PageSize.GetPaperSize();
            pageSettings.Landscape = Controller.PageSize.mLandscape;
            pageSettings.Margins = new Margins(0, 0, 0, 0);

            pageDlg.EnableMetric = true;
            pageDlg.PageSettings = pageSettings;

            System.Windows.Forms.DialogResult result = pageDlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Controller.PageSize.Setup(pageDlg.PageSettings);

                Redraw();
            }
        }

        public void DocSetting()
        {
            DocumentSettingsDialog dlg = new DocumentSettingsDialog();

            dlg.Owner = mMainWindow.GetWindow();

            dlg.WorldScale = mViewManager.DrawContext.WorldScale;

            bool? result = dlg.ShowDialog();

            if (result ?? false)
            {
                SetWorldScale(dlg.WorldScale);
                Redraw();
            }
        }

        public void SetWorldScale(double scale)
        {
            mViewManager.SetWorldScale(scale);
        }

        public void TextCommand(string s)
        {
            mController.TextCommand(s);
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
            textBox.CandidateList = Controller.ScriptEnv.AutoCompleteList;
        }

        public void Open()
        {
            DOut.pl("in PlotterViewModel#Open");

            Settings.Load();
            mViewManager.SetupViews();

            DOut.pl("out PlotterViewModel#Open");
        }

        public void Close()
        {
            Settings.Save();

            if (mEditorWindow != null)
            {
                mEditorWindow.Close();
                mEditorWindow = null;
            }
        }

        public override void DrawModeUpdated(DrawTools.DrawMode mode)
        {
            mViewManager.DrawModeUpdated(mode);
            mMainWindow.DrawModeUpdated(mode);
        }
    }
}
