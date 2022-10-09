using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using Plotter.Settings;

namespace TCad.ViewModel
{
    public class MoveKeyHandler
    {
        PlotterController Controller;

        public bool IsStarted;

        private List<CadFigure> EditFigList;

        private Vector3d Delta = default;

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
            Delta = Vector3d.Zero;
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
                Delta = Vector3d.Zero;
                IsStarted = true;
            }

            bool moveLittle = CadKeyboard.IsKeyPressed(System.Windows.Forms.Keys.ShiftKey);
            double a = moveLittle ? 0.1 : 1.0;

            //DOut.pl("MoveKeyDown a:" + a);

            Vector3d wx = Controller.DC.DevVectorToWorldVector(Vector3d.UnitX);
            Vector3d wy = Controller.DC.DevVectorToWorldVector(Vector3d.UnitY);

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
                    Vector3d p = Controller.GetCursorPos();
                    Controller.SetCursorWoldPos(p + Delta);
                    Controller.Redraw();
                }
            }
        }
    }
}
