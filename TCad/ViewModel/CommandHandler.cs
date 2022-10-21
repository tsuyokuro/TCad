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

    PlotterController Controller;
    IPlotterViewModel ViewModel;

    private Dictionary<string, Action> CommandMap;
    private Dictionary<string, KeyAction> KeyMap;

    private Window mEditorWindow;

    private MoveKeyHandler mMoveKeyHandler;

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
            { "ctrl+p", new KeyAction(InsPoint , null, "Inser Point")},
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
        Action action;
        if (CommandMap.TryGetValue(cmd, out action))
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
        KeyAction ka;
        if (KeyMap.TryGetValue(keyCmd, out ka))
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

    private string GetModifyerKeysString()
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

    private string KeyString(KeyEventArgs e)
    {
        string ks = GetModifyerKeysString();

        ks += e.Key.ToString().ToLower();

        return ks;
    }

    private string GetDisplayKeyString(string s)
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
        Controller.Remove();
        Redraw();
    }

    public void SeparateFigure()
    {
        Controller.SeparateFigures();
        Redraw();
    }

    public void BondFigure()
    {
        Controller.BondFigures();
        Redraw();
    }

    public void ToBezier()
    {
        Controller.ToBezier();
        Redraw();
    }

    public void CutSegment()
    {
        Controller.CutSegment();
        Redraw();
    }

    public void InsPoint()
    {
        Controller.InsPoint();
        Redraw();
    }

    public void ToLoop()
    {
        Controller.SetLoop(true);
        Redraw();
    }

    public void ToUnloop()
    {
        Controller.SetLoop(false);
        Redraw();
    }

    public void FlipWithVector()
    {
        Controller.FlipWithVector();
    }

    public void FlipAndCopyWithVector()
    {
        Controller.FlipAndCopyWithVector();
    }

    public void CutMeshWithVector()
    {
        Controller.CutMeshWithVector();
    }

    public void RotateWithPoint()
    {
        Controller.RotateWithPoint();
    }

    public void FlipNormal()
    {
        Controller.FlipNormal();
        Redraw();
    }

    public void ClearLayer()
    {
        Controller.ClearLayer(0);
        Redraw();
    }

    public void Copy()
    {
        Controller.Copy();
        Redraw();
    }

    public void Paste()
    {
        Controller.Paste();
        Redraw();
    }

    public void NewDocument()
    {
        ViewModel.CurrentFileName = null;

        ViewModel.ViewManager.SetWorldScale(1.0);

        Controller.ClearAll();
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
        GridSettingsDialog dlg = new();

        dlg.GridSize = ViewModel.Settings.GridSize;
        dlg.Owner = ViewModel.MainWindow.GetWindow();

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.GridSize = dlg.GridSize;
            Redraw();
        }
    }

    public void SnapSettings()
    {
        SnapSettingsDialog dlg = new();

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        dlg.PointSnapRange = ViewModel.Settings.PointSnapRange;
        dlg.LineSnapRange = ViewModel.Settings.LineSnapRange;

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.PointSnapRange = dlg.PointSnapRange;
            ViewModel.Settings.LineSnapRange = dlg.LineSnapRange;

            Redraw();
        }
    }

    public void PrintSettings()
    {
        PrintSettingsDialog dlg = new();

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        dlg.PrintWithBitmap = ViewModel.Settings.PrintWithBitmap;
        dlg.MagnificationBitmapPrinting = ViewModel.Settings.MagnificationBitmapPrinting;
        dlg.PrintLineSmooth = ViewModel.Settings.PrintLineSmooth;

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.PrintWithBitmap = dlg.PrintWithBitmap;
            ViewModel.Settings.MagnificationBitmapPrinting = dlg.MagnificationBitmapPrinting;
            ViewModel.Settings.PrintLineSmooth = dlg.PrintLineSmooth;
        }
    }

    public void MoveKeySettings()
    {
        MoveKeySettingsDialog dlg = new();

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        dlg.MoveX = ViewModel.Settings.MoveKeyUnitX;
        dlg.MoveY = ViewModel.Settings.MoveKeyUnitY;

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            ViewModel.Settings.MoveKeyUnitX = dlg.MoveX;
            ViewModel.Settings.MoveKeyUnitY = dlg.MoveY;
        }
    }

    public void SetLineColor()
    {
        if (Controller.CurrentFigure == null)
        {
            return;
        }

        ColorPickerDialog dlg = new();

        dlg.SelectedColor = Controller.CurrentFigure.LinePen.Color4;

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            if (dlg.InvalidColor)
            {
                Controller.CurrentFigure.LinePen.Color4 = DrawPen.InvalidPen.Color4;
            }
            else
            {
                Controller.CurrentFigure.LinePen.Color4 = dlg.SelectedColor;
            }
        }
    }

    public void SetFillColor()
    {
        if (Controller.CurrentFigure == null)
        {
            return;
        }

        ColorPickerDialog dlg = new();

        dlg.SelectedColor = Controller.CurrentFigure.FillBrush.Color4;

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
            if (dlg.InvalidColor)
            {
                Controller.CurrentFigure.FillBrush.Color4 = DrawPen.InvalidPen.Color4;
            }
            else
            {
                Controller.CurrentFigure.FillBrush.Color4 = dlg.SelectedColor;
            }
        }
    }

    public void Test()
    {
        ColorPickerDialog dlg = new();

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        bool? result = dlg.ShowDialog();

        if (result.Value)
        {
        }
    }

    public void ShowEditor()
    {
        if (mEditorWindow == null)
        {
            mEditorWindow = new EditorWindow(Controller.ScriptEnv);
            mEditorWindow.Owner = ViewModel.MainWindow.GetWindow();
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
        Controller.ObjOrderDown();
    }

    public void ObjOrderUp()
    {
        Controller.ObjOrderUp();
    }

    public void ObjOrderBottom()
    {
        Controller.ObjOrderBottom();
    }

    public void ObjOrderTop()
    {
        Controller.ObjOrderTop();
    }

    public void ResetCamera()
    {
        ViewModel.ViewManager.ResetCamera();
        Redraw();
    }

    public void AddLayer()
    {
        Controller.AddLayer(null);
        Redraw();
    }

    public void RemoveLayer()
    {
        Controller.RemoveLayer(Controller.CurrentLayer.ID);
        Redraw();
    }

    public void AddCentroid()
    {
        Controller.AddCentroid();
        Redraw();
    }

    public void SelectAll()
    {
        Controller.SelectAllInCurrentLayer();
        Redraw();
    }

    public void Cancel()
    {
        Controller.Cancel();
        Redraw();
    }

    public void SearchNearPoint()
    {
        Controller.MoveCursorToNearPoint(ViewModel.ViewManager.DrawContext);
        Redraw();
    }

    public void CursorLock()
    {
        Controller.CursorLocked = true;
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
        Controller.AddExtendSnapPoint();
        Redraw();
    }

    public void CleanMark()
    {
        Controller.ClearExtendSnapPointList();
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

        System.Windows.Forms.PrintDialog pdlg = new();

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
        CadSize2D deviceSize = new(e.PageBounds.Size.Width, e.PageBounds.Size.Height);
        CadSize2D pageSize = new(Controller.PageSize.Width, Controller.PageSize.Height);

        Controller.PrintPage(g, pageSize, deviceSize);
    }
    #endregion print


    [SupportedOSPlatform("windows")]
    public void PageSetting()
    {
        System.Windows.Forms.PageSetupDialog pageDlg = new();

        PageSettings pageSettings = new();

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
        DocumentSettingsDialog dlg = new();

        dlg.Owner = ViewModel.MainWindow.GetWindow();

        dlg.WorldScale = ViewModel.ViewManager.DrawContext.WorldScale_;

        bool? result = dlg.ShowDialog();

        if (result ?? false)
        {
            ViewModel.SetWorldScale(dlg.WorldScale);
            Redraw();
        }
    }

    public void Redraw()
    {
        ThreadUtil.RunOnMainThread(Controller.Redraw, true);
    }
}
