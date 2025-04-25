using Plotter;
using Plotter.Controller;
using Plotter.Serializer;
using Plotter.Settings;
using Plotter.svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using TCad.Controls;
using TCad.Dialogs;
using TCad.plotter.undo;
using TCad.ScriptEditor;

namespace TCad.ViewModel;

public class CommandHandler
{
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

    readonly IPlotterController Controller;
    readonly IPlotterViewModel ViewModel;

    private Dictionary<string, Action> CommandMap;
    private Dictionary<string, KeyAction> KeyMap;

    private Window mEditorWindow;

    private readonly MoveKeyHandler mMoveKeyHandler;

    public CommandHandler(IPlotterViewModel vm)
    {
        ViewModel = vm;
        Controller = vm.Controller;

        mMoveKeyHandler = new MoveKeyHandler(Controller);

        InitCommandMap();
        InitKeyMap();
    }

    private void InitCommandMap()
    {
        CommandMap = new Dictionary<string, Action>
        {
            { "new_doc", NewDocument },
            { "open_file", Load },
            { "save_file", Save },
            { "save_as", SaveAs },
            { "print", StartPrint },
            { "page_setting", PageSetting },
            { "doc_setting", DocSetting },
            { "undo", Undo },
            { "redo", Redo },
            { "copy", Copy },
            { "paste", Paste },
            { "separate", SeparateFigure },
            { "bond", BondFigure },
            { "to_bezier", ToBezier },
            { "cut_segment", CutSegment },
            { "ins_point", InsPoint },
            { "to_loop", ToLoop },
            { "to_unloop", ToUnloop },
            { "clear_layer", ClearLayer },
            { "flip_with_vector", FlipWithVector },
            { "flip_and_copy_with_vector", FlipAndCopyWithVector },
            { "rotate_with_point", RotateWithPoint },
            { "grid_settings", GridSettings },
            { "add_layer", AddLayer },
            { "remove_layer", RemoveLayer },
            { "centroid", AddCentroid },
            { "select_all", SelectAll },
            { "snap_settings", SnapSettings },
            { "move_key_settings", MoveKeySettings },
            { "show_editor", ShowEditor },
            { "export_svg", ExportSVG },
            { "obj_order_down", ObjOrderDown },
            { "obj_order_up", ObjOrderUp },
            { "obj_order_bottom", ObjOrderBottom },
            { "obj_order_top", ObjOrderTop },
            { "reset_camera", ResetCamera },
            { "cut_mesh_with_vector", CutMeshWithVector },
            { "print_setting", PrintSettings },
            { "set_line_color", SetLineColor },
            { "set_fill_color", SetFillColor },
            { "enter_command", EnterCommand },
            { "group", Group },
            { "ungroup", Ungroup },
            { "test", Test },
        };
    }

    private void InitKeyMap()
    {
        KeyMap = new Dictionary<string, KeyAction>
        {
            { "ctrl+z", new KeyAction(Undo , null, "Undo")},
            { "ctrl+y", new KeyAction(Redo , null, "Redo")},
            { "shift+ctrl+z", new KeyAction(Redo , null, "Redo")},
            { "ctrl+c", new KeyAction(Copy , null, "Copy")},
            { "ctrl+insert", new KeyAction(Copy , null, "Copy")},
            { "ctrl+v", new KeyAction(Paste ,null, "Paste")},
            { "shift+insert", new KeyAction(Paste , null, "Paste")},
            { "delete", new KeyAction(Remove , null, "Delete object")},
            { "ctrl+s", new KeyAction(Save , null, "Save")},
            { "ctrl+a", new KeyAction(SelectAll , null, "Select All")},
            { "escape", new KeyAction(Cancel , null)},
            { "ctrl+p", new KeyAction(InsPoint , null, "Insert Point to segment")},
            { "p", new KeyAction(AddPoint , null, "Add Point to cursor pos")},
            { "f3", new KeyAction(SearchNearPoint , null, "Search near Point")},
            { "f2", new KeyAction(CursorLock , null, "Lock Cursor")},
            { "left", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to left")},
            { "right", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to right")},
            { "up", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to up")},
            { "down", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to down")},
            { "shift+left", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to left with 1/10 unit")},
            { "shift+right", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to right with 1/10 unit")},
            { "shift+up", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to up with 1/10 unit")},
            { "shift+down", new KeyAction(MoveKeyDown, MoveKeyUp, "Move selected object to down with 1/10 unit")},
            { "m", new KeyAction(AddMark, null, " Add snap point")},
            { "ctrl+m", new KeyAction(CleanMark, null, " Clear snap points")},
        };
    }

    public void ExecCommand(string cmd)
    {
        if (CommandMap.TryGetValue(cmd, out Action action))
        {
            action.Invoke();
        }
        else
        {
            MessageBox.Show("Unknow Command: " + cmd);
        }
    }

    public bool ExecShortcutKey(string keyCmd, bool down)
    {
        if (KeyMap.TryGetValue(keyCmd, out KeyAction ka))
        {
            if (down)
            {
                ka.Down?.Invoke();
            }
            else
            {
                ka.Up?.Invoke();
            }
        }

        return true;
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

    private static string GetModifyerKeysString()
    {
        ModifierKeys modifierKeys = Keyboard.Modifiers;

        string s = "";

        if ((modifierKeys & ModifierKeys.Shift) != ModifierKeys.None)
        {
            s += "shift+";
        }

        if ((modifierKeys & ModifierKeys.Control) != ModifierKeys.None)
        {
            s += "ctrl+";
        }

        if ((modifierKeys & ModifierKeys.Alt) != ModifierKeys.None)
        {
            s += "alt+";
        }

        return s;
    }

    private static string KeyString(KeyEventArgs e)
    {
        string ks = GetModifyerKeysString();

        ks += e.Key.ToString().ToLower();

        return ks;
    }

    private static string GetDisplayKeyString(string s)
    {
        string[] ss = s.Split('+');

        string t = ss[0];
        string p = Char.ToUpper(t[0]) + t[1..];

        string ret = p;

        for (int i = 1; i < ss.Length; i++)
        {
            t = ss[i];
            p = Char.ToUpper(t[0]) + t[1..];
            ret += "+" + p;
        }

        return ret;
    }

    public List<string> HelpOfKey(string keyword)
    {
        List<string> ret = new();

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

        Regex re = new(keyword, RegexOptions.IgnoreCase);

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

    #region Actions
    public void Undo()
    {
        Controller.Undo();
        Redraw();
    }

    public void Redo()
    {
        Controller.Redo();
        Redraw();
    }

    public void Remove()
    {
        Controller.CommandProc.Remove();
        Redraw();
    }

    public void SeparateFigure()
    {
        Controller.Editor.SeparateFigures();
        Redraw();
    }

    public void BondFigure()
    {
        Controller.Editor.BondFigures();
        Redraw();
    }

    public void ToBezier()
    {
        Controller.Editor.ToBezier();
        Redraw();
    }

    public void CutSegment()
    {
        Controller.Editor.CutSegment();
        Redraw();
    }

    public void InsPoint()
    {
        Controller.CommandProc.InsPoint();
        Redraw();
    }

    public void AddPoint()
    {
        Controller.CommandProc.AddPointToCursorPos();
        Redraw();
    }

    public void ToLoop()
    {
        Controller.Editor.SetLoop(true);
        Redraw();
    }

    public void ToUnloop()
    {
        Controller.Editor.SetLoop(false);
        Redraw();
    }

    public void FlipWithVector()
    {
        Controller.Editor.FlipWithVector();
    }

    public void FlipAndCopyWithVector()
    {
        Controller.Editor.FlipAndCopyWithVector();
    }

    public void CutMeshWithVector()
    {
        Controller.Editor.CutMeshWithVector();
    }

    public void RotateWithPoint()
    {
        Controller.Editor.RotateWithPoint();
    }


    public void ClearLayer()
    {
        Controller.CommandProc.ClearLayer(0);
        Redraw();
    }

    public void Copy()
    {
        Controller.CommandProc.Copy();
        Redraw();
    }

    public void Paste()
    {
        Controller.CommandProc.Paste();
        Redraw();
    }

    public void NewDocument()
    {
        ViewModel.CurrentFileName = null;

        ViewModel.ViewManager.SetWorldScale((vcompo_t)(1.0));

        Controller.CommandProc.ClearAll();
        Redraw();
    }

    private static bool IsVaridDir(string path)
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
        System.Windows.Forms.OpenFileDialog ofd = new();

        if (IsVaridDir(SettingsHolder.Settings.LastDataDir))
        {
            ofd.InitialDirectory = SettingsHolder.Settings.LastDataDir;
        }

        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SettingsHolder.Settings.LastDataDir = Path.GetDirectoryName(ofd.FileName);

            try
            {
                CadFileAccessor.LoadFile(ofd.FileName, ViewModel);
                ViewModel.CurrentFileName = ofd.FileName;
            }
            catch (CadFileException cadFileException)
            {
                string text = cadFileException.getMessage(); ;
                string caption = "Load error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(text, caption, button, icon);
            }
        }
    }

    public void Save()
    {
        if (ViewModel.CurrentFileName != null)
        {
            CadFileAccessor.SaveFile(ViewModel.CurrentFileName, ViewModel);
            return;
        }

        SaveAs();
    }

    public void SaveAs()
    {
        System.Windows.Forms.SaveFileDialog sfd = new();

        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SettingsHolder.Settings.LastDataDir = Path.GetDirectoryName(sfd.FileName);

            CadFileAccessor.SaveFile(sfd.FileName, ViewModel);
            ViewModel.CurrentFileName = sfd.FileName;
        }
    }

    public void GridSettings()
    {
        GridSettingsDialog dlg = new()
        {
            GridSize = ViewModel.Settings.GridSize,
            Owner = Application.Current.MainWindow
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.GridSize = (vector3_t)dlg.GridSize;
            Redraw();
        }
    }

    public void SnapSettings()
    {
        SnapSettingsDialog dlg = new()
        {
            Owner = Application.Current.MainWindow,

            PointSnapRange = ViewModel.Settings.PointSnapRange,
            LineSnapRange = ViewModel.Settings.LineSnapRange
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.PointSnapRange = (vcompo_t)dlg.PointSnapRange;
            ViewModel.Settings.LineSnapRange = (vcompo_t)dlg.LineSnapRange;

            Redraw();
        }
    }

    public void PrintSettings()
    {
        PrintSettingsDialog dlg = new()
        {
            Owner = Application.Current.MainWindow,

            PrintWithBitmap = ViewModel.Settings.PrintWithBitmap,
            MagnificationBitmapPrinting = ViewModel.Settings.MagnificationBitmapPrinting,
            PrintLineSmooth = ViewModel.Settings.PrintLineSmooth
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.PrintWithBitmap = dlg.PrintWithBitmap;
            ViewModel.Settings.MagnificationBitmapPrinting = (vcompo_t)dlg.MagnificationBitmapPrinting;
            ViewModel.Settings.PrintLineSmooth = dlg.PrintLineSmooth;
        }
    }

    public void MoveKeySettings()
    {
        MoveKeySettingsDialog dlg = new()
        {
            Owner = Application.Current.MainWindow,

            MoveX = ViewModel.Settings.MoveKeyUnitX,
            MoveY = ViewModel.Settings.MoveKeyUnitY
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.MoveKeyUnitX = (vcompo_t)dlg.MoveX;
            ViewModel.Settings.MoveKeyUnitY = (vcompo_t)dlg.MoveY;
        }
    }

    public void SetLineColor()
    {
        CadFigure fig = Controller.Input.CurrentFigure;

        if (fig == null)
        {
            return;
        }

        ColorPickerDialog dlg = new()
        {
            SelectedColor = fig.LinePen.Color4,

            Owner = Application.Current.MainWindow
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            DrawPen oldPen = fig.LinePen;
            DrawPen newPen = default;

            if (dlg.InvalidColor)
            {
                fig.LinePen.Color4 = DrawPen.InvalidPen.Color4;
            }
            else
            {
                fig.LinePen.Color4 = dlg.SelectedColor;
            }

            newPen = fig.LinePen;
            CadOpe ope = new CadChangeFilgLinePen(fig.ID, oldPen, newPen);
            Controller.HistoryMan.foward(ope);
        }
    }

    public void SetFillColor()
    {
        CadFigure fig = Controller.Input.CurrentFigure;

        if (fig == null)
        {
            return;
        }

        ColorPickerDialog dlg = new()
        {
            SelectedColor = fig.FillBrush.Color4,

            Owner = Application.Current.MainWindow
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            DrawBrush oldBrush = fig.FillBrush;
            DrawBrush newBrush = default;

            if (dlg.InvalidColor)
            {
                Controller.Input.CurrentFigure.FillBrush.Color4 = DrawPen.InvalidPen.Color4;
            }
            else
            {
                Controller.Input.CurrentFigure.FillBrush.Color4 = dlg.SelectedColor;
            }

            newBrush = fig.FillBrush;
            CadOpe ope = new CadChangeFilgFillBrush(fig.ID, oldBrush, newBrush);
            Controller.HistoryMan.foward(ope);
        }
    }

    public void Group()
    {
        Group(Controller.GetSelectedFigureList());
    }

    public void Group(List<CadFigure> targetList)
    {
        List<CadFigure> list = FilterRootFigure(targetList);

        if (list.Count < 2)
        {
            ItConsole.println(
                Properties.Resources.error_select_2_or_more
                );

            return;
        }

        CadFigure parent = Controller.DB.NewFigure(CadFigure.Types.GROUP);

        CadOpeList opeRoot = new CadOpeList();
        CadOpe ope;

        foreach (CadFigure fig in list)
        {
            int idx = Controller.CurrentLayer.GetFigureIndex(fig.ID);

            if (idx < 0)
            {
                continue;
            }

            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.RemoveFigureByIndex(idx);

            parent.AddChild(fig);
        }

        Controller.CurrentLayer.AddFigure(parent);

        ope = new CadOpeAddChildlen(parent, parent.ChildList);
        opeRoot.Add(ope);

        ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, parent.ID);
        opeRoot.Add(ope);

        Controller.HistoryMan.foward(opeRoot);

        ItConsole.println(
                Properties.Resources.notice_was_grouped
            );
        Controller.UpdateObjectTree(true);
    }

    public void Ungroup()
    {
        Ungroup(Controller.GetSelectedFigureList());
    }

    public void Ungroup(List<CadFigure> targetList)
    {
        List<CadFigure> list = FilterRootFigure(targetList);

        CadOpeList opeRoot = new CadOpeList();

        CadOpe ope;

        foreach (CadFigure root in list)
        {
            root.ForEachFig((fig) =>
            {
                if (fig.Parent == null)
                {
                    return;
                }

                fig.Parent = null;

                if (fig.PointCount > 0)
                {
                    ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                    opeRoot.Add(ope);
                    Controller.CurrentLayer.AddFigure(fig);
                }
            });

            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, root.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.RemoveFigureByID(root.ID);
        }

        Controller.HistoryMan.foward(opeRoot);

        ItConsole.println(
            Properties.Resources.notice_was_ungrouped
            );

        Controller.UpdateObjectTree(true);
    }

    public List<CadFigure> FilterRootFigure(List<CadFigure> srcList)
    {
        HashSet<CadFigure> set = new();

        foreach (CadFigure fig in srcList)
        {
            set.Add(FigUtil.GetRootFig(fig));
        }

        List<CadFigure> ret = new List<CadFigure>();

        ret.AddRange(set);

        return ret;
    }


    private void EnterCommand()
    {
        ViewModel.CommandTextBox.Enter();
    }

    public void Test()
    {
        ColorPickerDialog dlg = new()
        {
            Owner = Application.Current.MainWindow
        };

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
        }
    }

    public void ShowEditor()
    {
        if (mEditorWindow == null)
        {
            mEditorWindow = new EditorWindow(Controller.ScriptEnv)
            {
                Owner = Application.Current.MainWindow
            };

            mEditorWindow.Show();

            mEditorWindow.Closed += (_, _) =>
            {
                mEditorWindow = null;
                Application.Current.MainWindow.Activate();
            };
        }
        else
        {
            if (mEditorWindow.WindowState == WindowState.Minimized)
            {
                mEditorWindow.WindowState = WindowState.Normal;
            }
            mEditorWindow.Activate();
        }
    }

    public void ExportSVG()
    {
        System.Windows.Forms.SaveFileDialog sfd = new();

        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            List<CadFigure> figList = Controller.DB.GetSelectedFigList();

            SvgExporter exporter = new();

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
        Controller.CommandProc.ObjOrderDown();
    }

    public void ObjOrderUp()
    {
        Controller.CommandProc.ObjOrderUp();
    }

    public void ObjOrderBottom()
    {
        Controller.CommandProc.ObjOrderBottom();
    }

    public void ObjOrderTop()
    {
        Controller.CommandProc.ObjOrderTop();
    }

    public void ResetCamera()
    {
        ViewModel.ViewManager.ResetCamera();
        Redraw();
    }

    public void AddLayer()
    {
        Controller.CommandProc.AddLayer(null);
        Redraw();
    }

    public void RemoveLayer()
    {
        Controller.CommandProc.RemoveLayer(Controller.CurrentLayer.ID);
        Redraw();
    }

    public void AddCentroid()
    {
        Controller.Editor.AddCentroid();
        Redraw();
    }

    public void SelectAll()
    {
        Controller.CommandProc.SelectAllInCurrentLayer();
        Redraw();
    }

    public void Cancel()
    {
        Controller.EditManager.Cancel();
        Redraw();
    }

    public void SearchNearPoint()
    {
        Controller.Input.MoveCursorToNearPoint(ViewModel.ViewManager.View.DrawContext);
        Redraw();
    }

    public void CursorLock()
    {
        Controller.Input.LockCursor();
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
        Controller.Input.AddExtendSnapPoint();
        Redraw();
    }

    public void CleanMark()
    {
        Controller.Input.ClearExtendSnapPointList();
        Redraw();
    }

    #endregion

    #region print
    public void StartPrint()
    {
        PrintDocument pd = new();

        PageSettings storePageSettings = pd.DefaultPageSettings;

        pd.DefaultPageSettings.PaperSize = Controller.PageSize.GetPaperSize();

        pd.DefaultPageSettings.Landscape = Controller.PageSize.mLandscape;

        pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

        pd.PrintPage += PrintPage;

        System.Windows.Forms.PrintDialog pdlg = new()
        {
            Document = pd
        };

        if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            pd.Print();
        }

        pd.DefaultPageSettings = storePageSettings;
    }

    private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
        Graphics g = e.Graphics;
        CadSize2D deviceSize = new(e.PageBounds.Size.Width, e.PageBounds.Size.Height);
        CadSize2D pageSize = new(Controller.PageSize.Width, Controller.PageSize.Height);

        Controller.PrintPage(g, pageSize, deviceSize);
    }
    #endregion print


    [SupportedOSPlatform("windows")]
    public void PageSetting()
    {
        System.Windows.Forms.PageSetupDialog pageDlg = new();

        PageSettings pageSettings = new()
        {
            PaperSize = Controller.PageSize.GetPaperSize(),
            Landscape = Controller.PageSize.mLandscape,
            Margins = new Margins(0, 0, 0, 0)
        };

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
        DocumentSettingsDialog dlg = new()
        {
            Owner = Application.Current.MainWindow,

            WorldScale = ViewModel.ViewManager.View.DrawContext.WorldScale
        };

        bool? result = dlg.ShowDialog();

        if (result ?? false)
        {
            ViewModel.SetWorldScale(((vcompo_t)dlg.WorldScale));
            Redraw();
        }
    }

    public void Redraw()
    {
        ThreadUtil.RunOnMainThread(Controller.Drawer.Redraw, true);
    }
}
