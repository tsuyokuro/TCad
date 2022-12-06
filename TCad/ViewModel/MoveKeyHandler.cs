using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using Plotter.Settings;
using System.Collections.Generic;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace TCad.ViewModel;

public class MoveKeyHandler
{
    PlotterController Controller;

    public bool IsStarted;

    private List<CadFigure> EditFigList;

    private vector3_t Delta = default;

    public MoveKeyHandler(PlotterController controller)
    {
        Controller = controller;
    }

    public void MoveKeyUp()
    {
        if (IsStarted)
        {
            Controller.EndEdit();
        }

        IsStarted = false;
        Delta = vector3_t.Zero;
        EditFigList = null;
    }

    public void MoveKeyDown()
    {
        if (Controller.GetSelectedFigureList().Count == 0)
        {
            return;
        }

        if (!IsStarted)
        {
            EditFigList = Controller.StartEdit();
            Delta = vector3_t.Zero;
            IsStarted = true;
        }

        bool moveLittle = CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.ShiftKey);
        vcompo_t a = moveLittle ? (vcompo_t)(0.1) : (vcompo_t)(1.0);

        //DOut.pl("MoveKeyDown a:" + a);

        vector3_t wx = Controller.DC.DevVectorToWorldVector(vector3_t.UnitX);
        vector3_t wy = Controller.DC.DevVectorToWorldVector(vector3_t.UnitY);

        wx = wx.UnitVector();
        wy = wy.UnitVector();

        wx *= (SettingsHolder.Settings.MoveKeyUnitX * a);
        wy *= (SettingsHolder.Settings.MoveKeyUnitY * a);

        if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Left))
        {
            Delta -= wx;
        }

        if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Right))
        {
            Delta += wx;
        }

        if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Up))
        {
            Delta -= wy;
        }

        if (CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.Down))
        {
            Delta += wy;
        }

        MoveInfo moveInfo = new MoveInfo();
        moveInfo.Delta = Delta;


        if (Controller.State == ControllerStates.SELECT)
        {
            if (EditFigList != null && EditFigList.Count > 0)
            {
                Controller.MovePointsFromStored(EditFigList, moveInfo);
                Controller.Redraw();
            }
            else
            {
                vector3_t p = Controller.GetCursorPos();
                Controller.SetCursorWoldPos(p + Delta);
                Controller.Redraw();
            }
        }
    }
}
