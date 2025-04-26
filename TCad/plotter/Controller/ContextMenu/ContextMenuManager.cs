using TCad.Plotter.Model.Figure;
using TCad.Plotter.searcher;
using TCad.Properties;

namespace TCad.Plotter.Controller;

public class ContextMenuManager
{
    MenuInfo mContextMenuInfo = new MenuInfo();

    IPlotterController Controller;

    public static MenuInfo.Item CreatingFigureQuit = new MenuInfo.Item(Resources.menu_quit_create, "quit_create");
    public static MenuInfo.Item CreatingFigureEnd = new MenuInfo.Item(Resources.menu_end_create, "end_create");
    public static MenuInfo.Item CreatingFigureClose = new MenuInfo.Item(Resources.menu_to_loop, "to_loop");
    public static MenuInfo.Item Copy = new MenuInfo.Item(Resources.menu_copy, "copy");
    public static MenuInfo.Item Paste = new MenuInfo.Item(Resources.menu_paste, "paste");
    public static MenuInfo.Item InsertPoint = new MenuInfo.Item(Resources.menu_insert_point, "insert_point");

    public ContextMenuManager(IPlotterController controller)
    {
        Controller = controller;
    }

    public void RequestContextMenu(vcompo_t x, vcompo_t y)
    {
        mContextMenuInfo.Items.Clear();


        if (Controller.FigureCreator != null)
        {
            switch (Controller.CreatingFigType)
            {
                case CadFigure.Types.POLY_LINES:
                    if (Controller.FigureCreator.Figure.PointCount > 2)
                    {
                        mContextMenuInfo.Items.Add(CreatingFigureClose);
                    }

                    mContextMenuInfo.Items.Add(CreatingFigureEnd);

                    break;

                case CadFigure.Types.RECT:
                    mContextMenuInfo.Items.Add(CreatingFigureQuit);
                    break;
            }
        }
        else
        {
            if (SegSelected())
            {
                mContextMenuInfo.Items.Add(InsertPoint);
            }

            bool hasSelect = Controller.HasSelect();
            bool hasCopyData = PlotterClipboard.HasCopyData();

            if (hasSelect)
            {
                mContextMenuInfo.Items.Add(Copy);
            }

            if (hasCopyData)
            {
                mContextMenuInfo.Items.Add(Paste);
            }
        }

        if (mContextMenuInfo.Items.Count > 0)
        {
            Controller.ShowContextMenu(mContextMenuInfo, (int)x, (int)y);
        }
    }

    private bool SegSelected()
    {
        if (Controller.Input.LastSelSegment == null)
        {
            return false;
        }

        MarkSegment seg = Controller.Input.LastSelSegment.Value;

        CadFigure fig = Controller.DB.GetFigure(seg.FigureID);

        if (fig == null)
        {
            return false;
        }

        if (fig.Type != CadFigure.Types.POLY_LINES)
        {
            return false;
        }

        bool handle = false;

        handle |= fig.GetPointAt(seg.PtIndexA).IsHandle;
        handle |= fig.GetPointAt(seg.PtIndexB).IsHandle;

        if (handle)
        {
            return false;
        }

        return true;
    }


    public void ContextMenuEvent(MenuInfo.Item menuItem)
    {
        string tag = (string)menuItem.Tag;

        switch (tag)
        {
            case "to_loop":
                Controller.CloseFigure();
                break;

            case "end_create":
                Controller.EndCreateFigure();

                break;
            case "quit_create":
                Controller.EndCreateFigure();
                break;

            case "copy":
                Controller.CommandProc.Copy();
                break;

            case "paste":
                Controller.CommandProc.Paste();
                break;

            case "insert_point":
                Controller.CommandProc.InsPoint();
                break;
        }
    }
}
