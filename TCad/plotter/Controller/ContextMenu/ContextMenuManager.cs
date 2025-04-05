using TCad.Properties;

namespace Plotter.Controller;

public class ContextMenuManager
{
    MenuInfo mContextMenuInfo = new MenuInfo();

    PlotterController mController;

    public static MenuInfo.Item CreatingFigureQuit = new MenuInfo.Item(Resources.menu_quit_create, "quit_create");
    public static MenuInfo.Item CreatingFigureEnd = new MenuInfo.Item(Resources.menu_end_create, "end_create");
    public static MenuInfo.Item CreatingFigureClose = new MenuInfo.Item(Resources.menu_to_loop, "to_loop");
    public static MenuInfo.Item Copy = new MenuInfo.Item(Resources.menu_copy, "copy");
    public static MenuInfo.Item Paste = new MenuInfo.Item(Resources.menu_paste, "paste");
    public static MenuInfo.Item InsertPoint = new MenuInfo.Item(Resources.menu_insert_point, "insert_point");

    public ContextMenuManager(PlotterController controller)
    {
        mController = controller;
    }

    public void RequestContextMenu(vcompo_t x, vcompo_t y)
    {
        mContextMenuInfo.Items.Clear();


        if (mController.FigureCreator != null)
        {
            switch (mController.CreatingFigType)
            {
                case CadFigure.Types.POLY_LINES:
                    if (mController.FigureCreator.Figure.PointCount > 2)
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

            bool hasSelect = mController.HasSelect();
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
            mController.ViewModelIF.ShowContextMenu(mContextMenuInfo, (int)x, (int)y);
        }
    }

    private bool SegSelected()
    {
        if (mController.Input.LastSelSegment == null)
        {
            return false;
        }

        MarkSegment seg = mController.Input.LastSelSegment.Value;

        CadFigure fig = mController.DB.GetFigure(seg.FigureID);

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
                mController.CloseFigure();
                break;

            case "end_create":
                mController.EndCreateFigure();

                break;
            case "quit_create":
                mController.EndCreateFigure();
                break;

            case "copy":
                mController.Copy();
                break;

            case "paste":
                mController.Paste();
                break;

            case "insert_point":
                mController.InsPoint();
                break;
        }
    }
}
