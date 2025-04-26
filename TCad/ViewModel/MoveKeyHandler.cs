using TCad.Plotter;
using TCad.Plotter.Controller;
using Plotter.Settings;
using TCad.Plotter.Model.Figure;

namespace TCad.ViewModel;

public class MoveKeyHandler
{
    IPlotterController Controller;

    public bool IsStarted;

    private vector3_t Delta = default;

    public MoveKeyHandler(IPlotterController controller)
    {
        Controller = controller;
    }

    public void MoveKeyUp()
    {
        if (IsStarted)
        {
            Controller.CurrentState.MoveKeyUp();
        }

        IsStarted = false;
        Delta = vector3_t.Zero;
    }

    public void MoveKeyDown()
    {
        MoveInfo moveInfo = new MoveInfo();

        if (!IsStarted)
        {
            Delta = vector3_t.Zero;
            IsStarted = true;
            moveInfo.Delta = Delta;

            Controller.CurrentState.MoveKeyDown(moveInfo, true);
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

        moveInfo.Delta = Delta;

        Controller.CurrentState.MoveKeyDown(moveInfo, false);
    }
}
