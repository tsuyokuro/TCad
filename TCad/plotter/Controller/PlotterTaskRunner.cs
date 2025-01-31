using CadDataTypes;
using HalfEdgeNS;
using MeshUtilNS;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCad.Controls;
using TCad.Dialogs;
using TCad.ViewModel;

namespace Plotter.Controller.TaskRunner;

public class PlotterTaskRunner
{
    public PlotterController Controller;

    public PlotterTaskRunner(PlotterController controller)
    {
        Controller = controller;
    }

    public async void FlipWithInteractive(List<CadFigure> rootFigList)
    {
        await Task.Run(() =>
        {
            Controller.StartEdit();
            var res = InputLine("Input flip axis");

            if (res.state != InteractCtrl.States.END)
            {
                Controller.AbendEdit();
                return;
            }

            if ((res.p1 - res.p0).IsZero())
            {
                Controller.AbendEdit();
                ItConsole.println("Error: Same point");
                return;
            }

            vector3_t normal = CadMath.Normal(
                res.p1 - res.p0, (Controller.DC.ViewDir));

            FlipWithPlane(rootFigList, res.p0, normal);

            RunOnMainThread(() =>
            {
                Controller.EndEdit();
                Controller.Redraw();
            });
        });
    }

    public void FlipWithPlane(List<CadFigure> rootFigList, vector3_t p0, vector3_t normal)
    {
        foreach (CadFigure fig in rootFigList)
        {
            fig.ForEachFig(f =>
            {
                FlipWithPlane(f, p0, normal);
            });
        }
    }

    public void FlipWithPlane(CadFigure fig, vector3_t p0, vector3_t normal)
    {
        fig.FlipWithPlane(p0, normal);
    }

    public async void FlipAndCopyWithInteractive(List<CadFigure> rootFigList)
    {
        await Task.Run(() =>
        {
            var res = InputLine("Input flip axis");

            if (res.state != InteractCtrl.States.END)
            {
                return;
            }

            if ((res.p1 - res.p0).IsZero())
            {
                ItConsole.println("Error: Same point");
                return;
            }

            vector3_t normal = CadMath.Normal(
                res.p1 - res.p0, Controller.DC.ViewDir);

            FlipAndCopyWithPlane(rootFigList, res.p0, normal);
        });
    }

    public void FlipAndCopyWithPlane(List<CadFigure> rootFigList, vector3_t p0, vector3_t normal)
    {
        List<CadFigure> cpy = PlotterClipboard.CopyFigures(rootFigList);

        CadOpeList opeRoot = new CadOpeList();

        CadLayer layer = Controller.CurrentLayer;

        foreach (CadFigure fig in cpy)
        {
            fig.ForEachFig(f =>
            {
                FlipWithPlane(f, p0, normal);
                Controller.DB.AddFigure(f);
            });

            layer.AddFigure(fig);

            CadOpe ope = new CadOpeAddFigure(layer.ID, fig.ID);
            opeRoot.OpeList.Add(ope);
        }

        Controller.HistoryMan.foward(opeRoot);

        RunOnMainThread(() =>
        {
            Controller.Redraw();
            Controller.UpdateObjectTree(remakeTree : true);
        });
    }

    public async void CutMeshWithInteractive(CadFigure fig)
    {
        await Task.Run(() =>
        {
            var res = InputLine("Input Cut line");

            if (res.state != InteractCtrl.States.END)
            {
                Controller.AbendEdit();
                return;
            }

            if ((res.p1 - res.p0).IsZero())
            {
                Controller.AbendEdit();
                ItConsole.println("Error: Same point");
                return;
            }

            CadFigureMesh mesh = fig as CadFigureMesh;
            
            if (mesh == null)
            {
                Controller.AbendEdit();
                ItConsole.println("Error: Target is not mesh");
                return;
            }

            vector3_t normal = CadMath.Normal(
                res.p1 - res.p0, (Controller.DC.ViewDir));

            CutMeshWithVector(mesh, res.p0, res.p1, normal);

            RunOnMainThread(() =>
            {
                Controller.ClearSelection();
                Controller.Redraw();
            });
        });
    }

    public void CutMeshWithVector(CadFigureMesh tfig, vector3_t p0, vector3_t p1, vector3_t normal)
    {
        HeModel he = tfig.mHeModel;
        CadMesh src = HeModelConverter.ToCadMesh(he);

        (CadMesh m1, CadMesh m2) = MeshUtil.CutMeshWithVector(src, p0, p1, normal);

        if (m1 == null || m2 == null)
        {
            return;
        }

        CadFigureMesh fig1 = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
        fig1.SetMesh(HeModelConverter.ToHeModel(m1));

        CadFigureMesh fig2 = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
        fig2.SetMesh(HeModelConverter.ToHeModel(m2));


        CadOpeList opeRoot = new CadOpeList();

        CadOpe ope;

        ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig1.ID);
        opeRoot.Add(ope);
        Controller.CurrentLayer.AddFigure(fig1);

        ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig2.ID);
        opeRoot.Add(ope);
        Controller.CurrentLayer.AddFigure(fig2);

        ope = new CadOpeRemoveFigure(Controller.CurrentLayer, tfig.ID);
        opeRoot.Add(ope);
        Controller.CurrentLayer.RemoveFigureByID(tfig.ID);

        Controller.HistoryMan.foward(opeRoot);
    }

    public async void RotateWithInteractive(List<CadFigure> rootFigList)
    {
        await Task.Run(() =>
        {
            Controller.StartEdit();
            var res = InputPoint();

            if (res.state != InteractCtrl.States.END)
            {
                Controller.AbendEdit();
                return;
            }

            vector3_t p0 = res.p0;

            vcompo_t angle = 0;

            bool ok = false;

            RunOnMainThread(() =>
            {
                AngleInputDialog dlg = new AngleInputDialog();
                bool? dlgRet = dlg.ShowDialog();

                ok = dlgRet.Value;

                if (ok)
                {
                    angle = (vcompo_t)dlg.GetAngle();
                }
            });

            if (!ok)
            {
                ItConsole.println("Cancel!");

                Controller.AbendEdit();
                return;
            }

            RotateWithAxis(
                rootFigList,
                p0,
                Controller.DC.ViewDir,
                CadMath.Deg2Rad(angle));

            Controller.EndEdit();

            RunOnMainThread(() =>
            {
                Controller.Redraw();
                Controller.UpdateObjectTree(remakeTree : false);
            });
        });
    }

    public void RotateWithAxis(List<CadFigure> rootFigList, vector3_t org, vector3_t axisDir, vcompo_t angle)
    {
        foreach (CadFigure fig in rootFigList)
        {
            fig.ForEachFig(f =>
            {
                CadUtil.RotateFigure(f, org, axisDir, angle);
            });
        }
    }

    public (vector3_t p0, InteractCtrl.States state) InputPoint()
    {
        InteractCtrl ctrl = Controller.InteractCtrl;

        ctrl.Start();

        OpenPopupMessage("Input rotate origin", UITypes.MessageType.INPUT);
        ItConsole.println(AnsiEsc.BYellow + "<< Input point >>");

        InteractCtrl.States ret;

        ret = ctrl.WaitPoint();

        if (ret != InteractCtrl.States.CONTINUE)
        {
            ctrl.End();
            ClosePopupMessage();
            ItConsole.println("Cancel!");
            return (
                VectorExt.InvalidVector3,
                InteractCtrl.States.CANCEL);
        }

        vector3_t p0 = ctrl.PointList[0];
        ItConsole.println(p0.CoordString());
        ctrl.End();
        ClosePopupMessage();

        return (p0, ctrl.State);
    }


    public (vector3_t p0, vector3_t p1, InteractCtrl.States state) InputLine(string message)
    {
        InteractCtrl ctrl = Controller.InteractCtrl;

        ctrl.Start();

        OpenPopupMessage(message, UITypes.MessageType.INPUT);
        ItConsole.println(AnsiEsc.BYellow + "<< Input point 1 >>");

        InteractCtrl.States ret;

        ret = ctrl.WaitPoint();

        if (ret != InteractCtrl.States.CONTINUE)
        {
            ctrl.End();
            ClosePopupMessage();
            ItConsole.println("Cancel!");
            return (
                VectorExt.InvalidVector3,
                VectorExt.InvalidVector3,
                InteractCtrl.States.CANCEL);
        }

        vector3_t p0 = ctrl.PointList[0];
        ItConsole.println(p0.CoordString());

        ItConsole.println(AnsiEsc.BYellow + "<< Input point 2 >>");

        ret = ctrl.WaitPoint();

        if (ret != InteractCtrl.States.CONTINUE)
        {
            ctrl.End();
            ClosePopupMessage();
            ItConsole.println("Cancel!");
            return (
                VectorExt.InvalidVector3,
                VectorExt.InvalidVector3,
                InteractCtrl.States.CANCEL);
        }

        vector3_t p1 = ctrl.PointList[1];
        ItConsole.println(p1.CoordString());

        ctrl.End();
        ClosePopupMessage();

        return (p0, p1, InteractCtrl.States.END);
    }

    public void OpenPopupMessage(string text, UITypes.MessageType type)
    {
        Controller.ViewModelIF.OpenPopupMessage(text, type);
    }

    public void ClosePopupMessage()
    {
        Controller.ViewModelIF.ClosePopupMessage();
    }

    private void RunOnMainThread(Action action)
    {
        ThreadUtil.RunOnMainThread(action: action, wait: true);
    }
}
