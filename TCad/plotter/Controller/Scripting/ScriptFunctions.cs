using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using CadDataTypes;
using HalfEdgeNS;
using CarveWapper;
using MeshUtilNS;
using MeshMakerNS;
using TCad.Controls;
using static Plotter.CadFigure;
using LibiglWrapper;
using GLUtil;
using OpenTK;
using OpenTK.Mathematics;
using System.Security.Cryptography;
using Microsoft.Scripting.Hosting;
using OpenTK.Graphics.OpenGL;
using OpenTK.WinForms;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using TCad.ViewModel;
using static IronPython.Modules.CTypes;

namespace Plotter.Controller
{
    public class ScriptFunctions
    {
        private PlotterController Controller;

        private ScriptEnvironment Env;

        private ScriptSession Session;

        public ScriptFunctions()
        {
        }

        public void Init(ScriptEnvironment env, ScriptScope scope)
        {
            Env = env;
            Controller = env.Controller;

            Session = new ScriptSession(Env);

            scope.SetVariable("normal", new Func<Vector3d, Vector3d, Vector3d>(CadMath.Normal));
            Env.AutoCompleteList.Add("normal(v1, v2)");

            scope.SetVariable("dot_product", new Func<Vector3d, Vector3d, double>(CadMath.InnerProduct));
            Env.AutoCompleteList.Add("dot_product(v1, v2)");

            scope.SetVariable("cross_product", new Func<Vector3d, Vector3d, Vector3d>(CadMath.CrossProduct));
            Env.AutoCompleteList.Add("cross_product(v1, v2)");
        }

        public Vector3d FigVertexAt(CadFigure fig, int index)
        {
            return fig.GetPointAt(index).vector;
        }

        public CadFigure GetFigure(uint id)
        {
            return Controller.DB.GetFigure(id);
        }

        public void StartSession(bool snapshotDB = false)
        {
            Session.Start(snapshotDB);
        }

        public void EndSession()
        {
            Session.End();
        }

        public void Println(string s)
        {
            ItConsole.println(s);
        }

        public void Print(string s)
        {
            ItConsole.print(s);
        }

        public void CursorAngleX(double d)
        {
            double t = -CadMath.Deg2Rad(d);

            Controller.CrossCursor.DirX.X = Math.Cos(t);
            Controller.CrossCursor.DirX.Y = Math.Sin(t);
        }

        public void CursorAngleY(double d)
        {
            double t = -CadMath.Deg2Rad(d) + Math.PI / 2;

            Controller.CrossCursor.DirY.X = Math.Cos(t);
            Controller.CrossCursor.DirY.Y = Math.Sin(t);
        }

        public void PrintVector(Vector3d v)
        {
            var sb = new StringBuilder();

            sb.Append(CadUtil.ValToString(v.X));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.Y));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.Z));

            ItConsole.println(sb.ToString());
        }

        public Vector3d WorldPToDevP(Vector3d w)
        {
            return Controller.DC.WorldPointToDevPoint(w);
        }

        public Vector3d DevPToWorldP(Vector3d d)
        {
            return Controller.DC.DevPointToWorldPoint(d);
        }

        public void DumpVector(Vector3d v)
        {
            string s = v.CoordString();
            ItConsole.println(s);
        }

        public Vector3d GetLastDownPoint()
        {
            return Controller.LastDownPoint;
        }

        public CadVertex CreateVertex(double x, double y, double z)
        {
            return CadVertex.Create(x, y, z);
        }

        public Vector3d CreateVector(double x, double y, double z)
        {
            return new Vector3d(x, y, z);
        }

        public Vector3d GetProjectionDir()
        {
            return -Controller.DC.ViewDir;
        }

        public int GetTreeViewPos(uint id)
        {
            int idx = Controller.FindObjectTreeItem(id);

            if (idx < 0)
            {
                ItConsole.println(
                    String.Format("ID:{0} is not found", id));
                return -1;
            }

            return idx;
        }

        public void SetTreeViewPos(int pos)
        {
            Controller.SetObjectTreePos(pos);
        }

        public void LayerList()
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                ItConsole.println("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
            }
        }

        public void SelectFigure(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        public void Scale(uint id, Vector3d org, double scale)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            fig.Select();

            Controller.StartEdit();

            fig.ForEachFig((f) => {
                CadUtil.ScaleFigure(f, org, scale);
            });

            Controller.EndEdit();

            Session.PostRedraw();
        }

        public List<CadFigure> FilterRootFigure(List<CadFigure> srcList)
        {
            HashSet<CadFigure> set = new HashSet<CadFigure>();

            foreach (CadFigure fig in srcList)
            {
                set.Add(FigUtil.GetRootFig(fig));
            }

            List<CadFigure> ret = new List<CadFigure>();

            ret.AddRange(set);

            return ret;
        }

        // Scriptから使いやすいようにintで受ける
        public List<CadFigure> ToFigList(IList<int> idList)
        {
            var figList = new List<CadFigure>();

            foreach (uint id in idList)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig != null)
                {
                    figList.Add(fig);
                }
            }

            return figList;
        }

        public List<CadFigure> GetSelectedFigList()
        {
            return Controller.DB.GetSelectedFigList();
        }

        public void Group(IList<int> idList)
        {
            List<CadFigure> figList = ToFigList(idList);
            Group(figList);
        }

        public void Group(List<CadFigure> targetList)
        {
            List<CadFigure> list = FilterRootFigure(targetList);

            if (list.Count < 2)
            {
                ItConsole.println(
                    global::TCad.Properties.Resources.error_select_2_or_more
                    );

                return;
            }

            CadFigure parent = Controller.DB.NewFigure(CadFigure.Types.GROUP);

            CadOpeList opeRoot = Session.StartWithSnapshotDB? null : new CadOpeList();
            CadOpe ope;

            foreach (CadFigure fig in list)
            {
                int idx = Controller.CurrentLayer.GetFigureIndex(fig.ID);

                if (idx < 0)
                {
                    continue;
                }

                if (!Session.StartWithSnapshotDB)
                {
                    ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
                    opeRoot.Add(ope);
                }

                Controller.CurrentLayer.RemoveFigureByIndex(idx);

                parent.AddChild(fig);
            }

            Controller.CurrentLayer.AddFigure(parent);

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeAddChildlen(parent, parent.ChildList);
                opeRoot.Add(ope);

                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, parent.ID);
                opeRoot.Add(ope);

                Session.AddOpe(opeRoot);
            }

            ItConsole.println(
                    global::TCad.Properties.Resources.notice_was_grouped
                );

            Session.PostRemakeObjectTree();
        }

        public void Ungroup(IList<int> idList)
        {
            List<CadFigure> figList = ToFigList(idList);
            Ungroup(figList);
        }

        public void Ungroup(int id)
        {
            List<CadFigure> figList = new List<CadFigure>();

            CadFigure fig = Controller.DB.GetFigure((uint)id);

            if (fig == null)
            {
                return;
            }

            figList.Add(fig);

            Ungroup(figList);
        }

        public void Ungroup(List<CadFigure> targetList)
        {
            List<CadFigure> list = FilterRootFigure(targetList);

            CadOpeList opeList = Session.StartWithSnapshotDB? null : new CadOpeList();

            CadOpe ope;

            foreach (CadFigure root in list)
            {
                root.ForEachFig((fig) => {
                    if (fig.Parent == null)
                    {
                        return;
                    }

                    fig.Parent = null;
                    
                    if (fig.PointCount > 0)
                    {
                        ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                        opeList.Add(ope);
                        Controller.CurrentLayer.AddFigure(fig);
                    }
                });

                if (!Session.StartWithSnapshotDB)
                {
                    ope = new CadOpeRemoveFigure(Controller.CurrentLayer, root.ID);
                    opeList.Add(ope);
                }
                Controller.CurrentLayer.RemoveFigureByID(root.ID);
            }

            if (!Session.StartWithSnapshotDB)
            {
                Session.AddOpe(opeList);
            }

            ItConsole.println(
                TCad.Properties.Resources.notice_was_ungrouped
                );

            Session.PostRemakeObjectTree();
        }

        public void MoveLastDownPoint(double x, double y, double z)
        {
            Vector3d p = Controller.LastDownPoint;

            Vector3d delta = new Vector3d(x, y, z);

            p += delta;

            Controller.LastDownPoint = p;

            Session.PostRedraw();
        }

        public void SetLastDownPoint(double x, double y, double z)
        {
            Vector3d p = new Vector3d(x, y, z);

            Controller.LastDownPoint = p;

            Session.PostRedraw();
        }

        public Vector3d GetPoint(uint figID, int index)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);
            if (fig == null)
            {
                return VectorExt.InvalidVector3d;
            }

            return fig.GetPointAt(index).vector;
        }

        public bool IsValidVector(Vector3d v)
        {
            return v.IsValid();
        }

        public bool IsInvalidVector(Vector3d v)
        {
            return v.IsInvalid();
        }

        public void SetPoint(uint figID, int index, Vector3d v)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);
            if (fig == null)
            {
                return;
            }

            fig.PointList.Ref(index).vector = v;
        }

        public CadFigure AddLine(Vector3d v0, Vector3d v1)
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            fig.AddPoint((CadVertex)v0);
            fig.AddPoint((CadVertex)v1);

            fig.EndCreate(Controller.DC);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            return fig;
        }

        public CadFigure AddPoint(double x, double y, double z)
        {
            Vector3d p = new Vector3d(x, y, z);
            return AddPoint(p);
        }

        public CadFigure AddPoint(Vector3d p)
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POINT);
            fig.AddPoint((CadVertex)p);

            fig.EndCreate(Controller.DC);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            return fig;
        }

        public CadFigure AddRect(double w, double h)
        {
            return AddRectAt(Controller.LastDownPoint, w, h);
        }

        public CadFigure AddRectAt(Vector3d p, double w, double h)
        {
            Vector3d viewDir = Controller.DC.ViewDir;
            Vector3d upDir = Controller.DC.UpVector;

            Vector3d wd = CadMath.Normal(viewDir, upDir) * w;
            Vector3d hd = upDir.UnitVector() * h;

            CadVertex p0 = (CadVertex)p;
            CadVertex p1 = (CadVertex)p;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.RECT);

            fig.AddPoint(p0);

            p1 = p0 + wd;
            fig.AddPoint(p1);

            p1 = p0 + wd + hd;
            fig.AddPoint(p1);

            p1 = p0 + hd;
            fig.AddPoint(p1);

            fig.IsLoop = true;

            fig.EndCreate(Controller.DC);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddRectChamfer(double w, double h, double c)
        {
            return AddRectRectChamferAt(Controller.LastDownPoint, w, h, c);
        }

        public CadFigure AddRectRectChamferAt(Vector3d p, double w, double h, double c)
        {
            Vector3d viewDir = Controller.DC.ViewDir;
            Vector3d upDir = Controller.DC.UpVector;

            Vector3d wdir = CadMath.Normal(viewDir, upDir);
            Vector3d hdir = upDir.UnitVector();

            Vector3d wd = wdir * w;
            Vector3d hd = hdir * h;

            CadVertex sp = (CadVertex)p;
            CadVertex tp = sp;

            Vector3d wc = wdir * c;
            Vector3d hc = hdir * c;


            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.RECT);

            fig.AddPoint(tp + wc);

            tp += wd;

            fig.AddPoint(tp - wc);

            fig.AddPoint(tp + hc);

            tp += hd;

            fig.AddPoint(tp - hc);

            fig.AddPoint(tp - wc);

            tp = sp + hd;

            fig.AddPoint(tp + wc);

            fig.AddPoint(tp - hc);

            tp = sp;

            fig.AddPoint(tp + hc);

            fig.IsLoop = true;

            fig.EndCreate(Controller.DC);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddCircle(double r)
        {
            return AddCircleAt(Controller.LastDownPoint, r);
        }

        public CadFigure AddCircleAt(Vector3d p, double r)
        {
            Vector3d viewDir = Controller.DC.ViewDir;
            Vector3d upDir = Controller.DC.UpVector;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.CIRCLE);

            Vector3d p0 = p;
            Vector3d p1 = p0 + CadMath.Normal(viewDir, upDir) * r;

            fig.AddPoint((CadVertex)p0);
            fig.AddPoint((CadVertex)p1);
            fig.EndCreate(Controller.DC);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadMesh CreateCadMesh(List<Vector3d> plist, List<CadFace> flist)
        {
            CadMesh cm = new CadMesh(plist.Count, flist.Count);

            foreach (Vector3d p in plist)
            {
                cm.VertexStore.Add(new CadVertex(p));
            }

            foreach (CadFace f in flist)
            {
                cm.FaceStore.Add(f);
            }

            return cm;
        }

        public CadMesh CreateCadMesh(List<CadVertex> plist, List<CadFace> flist)
        {
            CadMesh cm = new CadMesh(plist.Count, flist.Count);

            foreach (CadVertex p in plist)
            {
                cm.VertexStore.Add(p);
            }

            foreach (CadFace f in flist)
            {
                cm.FaceStore.Add(f);
            }

            return cm;
        }

        public CadFigure MesthToFig(CadMesh cm)
        {
            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            return fig;
        }

        public CadFigure CreateMeshFig(List<Vector3d> plist, List<CadFace> flist)
        {
            var cm = CreateCadMesh(plist, flist);
            return MesthToFig(cm);
        }

        public CadFigure CreateMeshFig(List<CadVertex> plist, List<CadFace> flist)
        {
            var cm = CreateCadMesh(plist, flist);
            return MesthToFig(cm);
        }

        public CadFigure AddBox(Vector3d pos, double x, double y, double z)
        {
            CadMesh cm =
                MeshMaker.CreateBox(pos, new Vector3d(x, y, z), MeshMaker.FaceType.TRIANGLE);

            CadFigure fig = MesthToFig(cm);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddTetrahedron(Vector3d pos, double x, double y, double z)
        {
            CadMesh cm =
                MeshMaker.CreateTetrahedron(pos, new Vector3d(x, y, z));

            CadFigure fig = MesthToFig(cm);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddOctahedron(Vector3d pos, double x, double y, double z)
        {
            CadMesh cm =
                MeshMaker.CreateOctahedron(pos, new Vector3d(x, y, z));

            CadFigure fig = MesthToFig(cm);
            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddCylinder(Vector3d pos, int circleDiv, int slices, double r, double len)
        {
            CadMesh cm = MeshMaker.CreateCylinder(pos, circleDiv, slices, r, len);

            CadFigure fig = MesthToFig(cm);
            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddSphere(Vector3d pos, int slices, double r)
        {
            CadMesh cm = MeshMaker.CreateSphere(pos, r, slices, slices);

            CadFigure fig = MesthToFig(cm);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }

        public CadFigure AddPicture(Vector3d pos, string fname)
        {
            CadFigurePicture fig = (CadFigurePicture)Controller.DB.NewFigure(CadFigure.Types.PICTURE);

            fig.Setup(Controller.PageSize, pos, fname);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();

            return fig;
        }


        public void MakeRotatingBody(uint baseFigID, Vector3d org, Vector3d axis, bool topCap, bool btmCap)
        {
            CadFigure baseFig = GetFigure(baseFigID);

            if (baseFig == null)
            {
                return;
            }

            if (baseFig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            CadMesh cm = MeshMaker.CreateRotatingBody(32, org, axis, baseFig.PointList, topCap, btmCap, MeshMaker.FaceType.TRIANGLE);

            CadFigure fig = MesthToFig(cm);

            CadOpe ope;
            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, baseFig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.RemoveFigureByID(baseFig.ID);

            Session.PostRemakeObjectTree();
        }

        public void AddLayer(string name)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                Controller.AddLayer(name);
            }, true);
        }

        public void Move(uint figID, double x, double y, double z)
        {
            Vector3d delta = new Vector3d(x, y, z);

            CadFigure fig = Controller.DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            var list = new List<CadFigure>() { fig };

            Controller.StartEdit(list);

            fig.ForEachFig((f) =>
            {
                f.MoveAllPoints(delta);
            });

            Controller.EndEdit(list);

            Session.PostRedraw();
        }

        public void MoveSelectedPoint(double x, double y, double z)
        {
            var figList = Controller.GetSelectedFigureList();

            Controller.StartEdit(figList);

            Vector3d d = new Vector3d(x, y, z);

            foreach (CadFigure fig in figList)
            {
                int i;
                for (i=0; i<fig.PointCount; i++)
                {
                    CadVertex v = fig.PointList[i];
                    if (v.Selected)
                    {
                        v.vector += d;
                        fig.PointList[i] = v;
                    }
                }
            }

            Controller.EndEdit(figList);
        }

        public void SetSelectedSegLen(double len)
        {
            if (Controller.LastSelSegment == null)
            {
                return;
            }

            MarkSegment seg = Controller.LastSelSegment.Value;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = Controller.DB.GetFigure(seg.FigureID);

            CadVertex pa = fig.GetPointAt(seg.PtIndexA);
            CadVertex pb = fig.GetPointAt(seg.PtIndexB);

            Vector3d v;

            v = pa.vector - Controller.LastDownPoint;
            double da = v.Norm();

            v = pb.vector - Controller.LastDownPoint;
            double db = v.Norm();


            if (da < db)
            {
                Vector3d np = CadMath.LinePoint(pb.vector, pa.vector, len);
                Controller.StartEdit();

                pa.vector = np;

                fig.SetPointAt(seg.PtIndexA, pa);

                Controller.EndEdit();
            }
            else
            {
                Vector3d np = CadMath.LinePoint(pa.vector, pb.vector, len);
                Controller.StartEdit();

                pb.vector = np;

                fig.SetPointAt(seg.PtIndexB, pb);

                Controller.EndEdit();
            }
        }

        public void InsPoint()
        {
            Controller.StartEdit();

            if (!Controller.InsPointToLastSelectedSeg())
            {
                Controller.AbendEdit();

                ItConsole.println(
                    global::TCad.Properties.Resources.error_operation_failed
                    );
                return;
            }

            Controller.EndEdit();

            ItConsole.println(
                global::TCad.Properties.Resources.notice_operation_success
                );
        }

        public double AreaOfSelected()
        {
            double area = PlotterUtil.Area(Controller.DB.GetSelectedFigList());
            ItConsole.println("Area: " + AnsiEsc.BYellow + (area / 100).ToString());

            return area;
        }

        public Vector3d CentroidOfSelected()
        {
            Centroid c = PlotterUtil.Centroid(Controller.DB.GetSelectedFigList());
            return c.Point;
        }

        //public CadVertex NewPoint()
        //{
        //    return default(CadVertex);
        //}

        //public CadFigure NewPolyLines()
        //{
        //    CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
        //    return fig;
        //}

        public void Rotate(uint figID, Vector3d org, Vector3d axisDir, double angle)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            if (axisDir.IsZero())
            {
                return;
            }

            //Controller.SelectFigure(figID);

            axisDir = axisDir.UnitVector();

            var list = new List<CadFigure>() { fig };

            Controller.StartEdit(list);

            RotateWithAxis(fig, org, axisDir, CadMath.Deg2Rad(angle));

            Controller.EndEdit(list);

            Session.PostRedraw();
        }

        public void RotateWithAxis(CadFigure fig, Vector3d org, Vector3d axisDir, double angle)
        {
            fig.ForEachFig(f =>
            {
                CadUtil.RotateFigure(f, org, axisDir, angle);
            });
        }

        public void CreateBitmapGDI(List<CadFigure> figList, int w, int h, uint argb, int lineW, string fname)
        {
            DrawContext dc = Controller.DC;

            CadRect r = CadUtil.GetContainsRectScrn(dc, figList);

            CadRect wr = default(CadRect);
            wr.p0 = dc.DevPointToWorldPoint(r.p0);
            wr.p1 = dc.DevPointToWorldPoint(r.p1);

            DrawContextGDIBmp tdc = new DrawContextGDIBmp();

            tdc.WorldScale = dc.WorldScale;

            tdc.SetCamera(dc.Eye, dc.LookAt, dc.UpVector);
            tdc.CalcProjectionMatrix();

            tdc.SetViewSize(w, h);

            tdc.SetViewOrg(new Vector3d(w / 2, h / 2, 0));

            tdc.SetupTools(DrawTools.DrawMode.DARK);

            DrawPen drawPen = new DrawPen((int)argb, lineW);

            double sw = r.p1.X - r.p0.X;
            double sh = r.p1.Y - r.p0.Y;

            double a = Math.Min(w, h) / (Math.Max(sw, sh) + lineW);

            tdc.DeviceScaleX *= a;
            tdc.DeviceScaleY *= a;

            CadRect tr = CadUtil.GetContainsRectScrn(tdc, figList);

            Vector3d trcp = (Vector3d)((tr.p1 - tr.p0) / 2 + tr.p0);

            Vector3d d = trcp - tdc.ViewOrg;

            tdc.SetViewOrg(tdc.ViewOrg - d);

            DrawOption dp = default;

            dp.LinePen = drawPen;
            dp.MeshEdgePen = drawPen;
            dp.MeshLinePen = drawPen;

            Env.RunOnMainThread((Action)(() =>
            {
                tdc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_TRANSPARENT));

                tdc.GdiGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (CadFigure fig in figList)
                {
                    fig.Draw(tdc, dp);
                }

                if (fname.Length > 0)
                {
                    tdc.Image.Save(fname);
                }
                else
                {
                    BitmapUtil.BitmapToClipboardAsPNG(tdc.Image);
                }

                tdc.Dispose();
            }));
        }

        public void CreateBitmap(int w, int h, uint argb, int lineW, string fname)
        {
            CadObjectDB db = Controller.DB;


            // Create figure list
            List<uint> idlist = Controller.DB.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                ItConsole.println("No Objects selected");
                return;
            }

            var figList = new List<CadFigure>();

            idlist.ForEach(id =>
            {
                figList.Add(db.GetFigure(id));
            });


            ThreadUtil.RunOnMainThread(() =>
            {
                CreateBitmapGLOrtho(figList, w, h, argb, lineW, fname);
            }, true);
        }

        private void CreateBitmapGLOrtho(List<CadFigure> figList, int w, int h, uint argb, int lineW, string fname)
        {
            //fname = @"F:\work\test.bmp";

            //GLControl tmpGLControl = new GLControl();
            //tmpGLControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            //tmpGLControl.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;
            //tmpGLControl.MakeCurrent();

            NativeWindowSettings settings = new NativeWindowSettings();
            settings.Profile = ContextProfile.Compatability;
            settings.Flags = ContextFlags.Default;

            NativeWindow window = new NativeWindow(settings);
            window.MakeCurrent();

            int paddingX = 4;
            int paddingY = 4;

            DrawContext orgDC = Controller.DC;


            DrawContextGLOrtho tdc = new DrawContextGLOrtho();

            tdc.SetupTools(DrawTools.DrawMode.LIGHT);
            tdc.CopyCamera(orgDC);
            tdc.SetViewSize(w, h);
            tdc.SetViewOrg(new Vector3d(w / 2, h / 2, 0));

            CadRect r2 = CadUtil.GetContainsRectScrn(tdc, figList);

            double ow = Math.Abs(r2.p1.X - r2.p0.X);
            double oh = Math.Abs(r2.p1.Y - r2.p0.Y);

            double scale = Math.Min((w - paddingX) / ow, (h -paddingY) / oh);

            tdc.WorldScale = scale;

            CadRect r3 = CadUtil.GetContainsRectScrn(tdc, figList);

            Vector3d center = r3.Center();
            ViewUtil.AdjustOrigin(tdc, center.X, center.Y, w, h);


            DrawPen drawPen = new DrawPen((int)argb, lineW);

            DrawOption drawParams = default;
            drawParams.LinePen = drawPen;
            drawParams.MeshLinePen = DrawPen.Invalid;
            drawParams.MeshEdgePen = drawPen;


            FrameBufferW fb = new FrameBufferW();
            fb.Create(w, h);

            fb.Begin();

            tdc.StartDraw();

            GL.Disable(EnableCap.LineSmooth);

            tdc.Drawing.Clear(new DrawBrush(Color.Blue.ToArgb()));
            //tdc.Drawing.Clear(tdc.GetBrush(DrawTools.BRUSH_TRANSPARENT));

            foreach (CadFigure fig in figList)
            {
                fig.Draw(tdc, drawParams);
            }

            tdc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            if (fname.Length > 0)
            {
                bmp.Save(fname);
            }
            else
            {
                BitmapUtil.BitmapToClipboardAsPNG(bmp);
            }

            tdc.Dispose();

            orgDC.MakeCurrent();

            //tmpGLControl.Dispose();
            window.Dispose();
        }


        public void FaceToDirection(Vector3d dir)
        {
            DrawContext dc = Controller.DC;

            CadObjectDB db = Controller.DB;

            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            FaceToDirection(fig, Controller.LastDownPoint, dir);
        }

        private void FaceToDirection(CadFigure fig, Vector3d org, Vector3d dir)
        {
            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            Vector3d faceNormal = CadUtil.TypicalNormal(fig.PointList);

            if (faceNormal.EqualsThreshold(dir) || (-faceNormal).EqualsThreshold(dir))
            {
                // Face is already target direction
                return;
            }

            //   | 回転軸 rv
            //   |
            //   |
            //   | --------->向けたい方向 dir
            //   /
            //  /
            // 面の法線 faceNormal
            Vector3d rv = CadMath.Normal(faceNormal, dir);

            double t = CadMath.AngleOfVector(faceNormal, dir);

            CadUtil.RotateFigure(fig, org, rv, t);
        }

        public void Triangulate(uint figID, double minArea, double maxDegree)
        {
            string option = $"a{minArea}q{maxDegree}";

            Triangulate(figID, option);
        }

        // option:
        // e.g.
        // a100q30 max area = 100, min degree = 30
        // a100q max area = 100, min degree = default (20)
        // min degree < 34
        // Other options see
        // https://www.cs.cmu.edu/~quake/triangle.switch.html
        //
        public void Triangulate(uint figID, string option)
        {
            CadFigure tfig = Controller.DB.GetFigure(figID);

            if (tfig == null || tfig.Type != Types.POLY_LINES)
            {
                return;
            }

            if (tfig.PointCount < 3)
            {
                return;
            }

            CadFigure cfig = FigUtil.Clone(tfig);

            Vector3d org = cfig.PointList[0].vector;
            Vector3d dir = Vector3d.UnitZ;

            Vector3d faceNormal = CadUtil.TypicalNormal(cfig.PointList);

            Vector3d rotateV = default;

            double t = 0;

            if (!faceNormal.EqualsThreshold(dir) && !(-faceNormal).EqualsThreshold(dir))
            {
                rotateV = CadMath.Normal(faceNormal, dir);
                t = -CadMath.AngleOfVector(faceNormal, dir);
                CadUtil.RotateFigure(cfig, org, rotateV, t);
            }

            //Controller.CurrentLayer.AddFigure(cfig);

            VertexList vl = cfig.GetPoints(12);

            CadMesh m = IglW.Triangulate(vl, option);

            HeModel hem = HeModelConverter.ToHeModel(m);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            for (int i = 0; i<fig.PointCount; i++)
            {
                CadVertex v = fig.PointList[i];
                v.Z = org.Z;
                fig.PointList[i] = v;
            }

            if (t != 0)
            {
                CadUtil.RotateFigure(fig, org, rotateV, -t);
            }

            CadOpeList root = Session.StartWithSnapshotDB? null : new CadOpeList();
            CadOpe ope;

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, figID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.RemoveFigureByID(figID);
            Controller.CurrentFigure = null;

            Session.PostRemakeObjectTree();
        }

        // 押し出し
        public void Extrude(uint id, Vector3d v, double d, int divide)
        {
            CadFigure tfig = Controller.DB.GetFigure(id);

            if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            v = v.UnitVector();

            v *= -d;

            CadMesh cm = MeshMaker.CreateExtruded(tfig.GetPoints(16), v, divide);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.RecalcNormal();

            fig.SetMesh(hem);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpeList root = new CadOpeList();
                CadOpe ope;

                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                root.Add(ope);

                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, tfig.ID);
                root.Add(ope);

                Session.AddOpe(root);
            }

            Controller.CurrentLayer.AddFigure(fig);
            Controller.CurrentLayer.RemoveFigureByID(tfig.ID);

            Session.PostRemakeObjectTree();
        }

        public void ToPolyLine(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (!(fig is CadFigureMesh))
            {
                return;
            }

            CadFigureMesh figMesh = (CadFigureMesh)fig;

            HeModel hm = figMesh.mHeModel;


            CadFigure figPoly = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

            hm.ForReachEdgePoint(v =>
            {
                figPoly.AddPoint(v);
            });

            if (figPoly.PointCount < 1)
            {
                return;
            }

            figPoly.IsLoop = true;

            if (!Session.StartWithSnapshotDB)
            {
                CadOpeList opeRoot = new CadOpeList();
                CadOpe ope;

                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, figPoly.ID);
                opeRoot.Add(ope);

                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
                opeRoot.Add(ope);
                Session.AddOpe(opeRoot);
            }

            Controller.CurrentLayer.AddFigure(figPoly);

            Controller.CurrentLayer.RemoveFigureByID(fig.ID);

            Env.RunOnMainThread(() =>
            {
                Controller.ClearSelection();
            });

            Session.PostRemakeObjectTree();
        }

        public void ToMesh(uint id)
        {
            CadOpeList opeRoot = Session.StartWithSnapshotDB? null: new CadOpeList();

            CadFigure orgFig = Controller.DB.GetFigure(id);


            if (orgFig == null)
            {
                return;    
            }

            if (orgFig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            CadFigureMesh mesh = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            mesh.CreateModel(orgFig);

            foreach (CadFigure fig in orgFig.ChildList)
            {
                mesh.AddChild(fig);
            }

            CadFigure parent = orgFig.Parent;

            if (parent != null)
            {
                int index = orgFig.Parent.ChildList.IndexOf(orgFig);

                // Remove original poly lines object
                opeRoot?.Add(new CadOpeRemoveChild(parent, orgFig, index));

                orgFig.Parent.ChildList.Remove(orgFig);

                // Insert mesh object
                opeRoot?.Add(new CadOpeAddChild(parent, mesh, index));

                orgFig.Parent.ChildList.Insert(index, mesh);
                mesh.Parent = parent;
            }
            else
            {
                // Remove original poly lines object
                opeRoot?.Add(new CadOpeAddFigure(Controller.CurrentLayer.ID, mesh.ID));
                Controller.CurrentLayer.AddFigure(mesh);

                // Insert mesh object
                opeRoot?.Add(new CadOpeRemoveFigure(Controller.CurrentLayer, orgFig.ID));
                Controller.CurrentLayer.RemoveFigureByID(orgFig.ID);
            }

            Session.AddOpe(opeRoot);

            Env.RunOnMainThread(() =>
            {
                Controller.ClearSelection();
            });

            Session.PostRemakeObjectTree();
            //PrintSuccess();
        }

        public void InvertDir()
        {
            List<CadFigure> figList = Controller.DB.GetSelectedFigList();

            CadOpeList opeRoot = Session.StartWithSnapshotDB? null : new CadOpeList();

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure fig = figList[i];
                fig.InvertDir();

                opeRoot?.Add(new CadOpeInvertDir(fig.ID));
            }

            if (!Session.StartWithSnapshotDB)
            {
                Session.AddOpe(opeRoot);
            }
        }

        public void SetFigName(uint id, string name)
        {
            CadFigure fig = Controller.DB.GetFigure(id);
            if (fig == null)
            {
                return;
            }

            if (name == "") name = null;
            fig.Name = name;

            UpdateTV();
        }

        private CadFigureMesh GetCadFigureMesh(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);
            if (fig == null || fig.Type != CadFigure.Types.MESH) return null;

            return (CadFigureMesh)fig;
        }


        public void AsubB(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.AMinusB(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();
        }

        public void Union(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.Union(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();
        }

        public void Intersection(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.Intersection(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();
        }

        public void CutMesh(uint id)
        {
            CadFigureMesh tfig = GetCadFigureMesh(id);
            if (tfig == null)
            {
                ItConsole.println("invalid ID");
                return;
            }

            (Vector3d p0, Vector3d p1) = InputLine();

            if (p0.IsInvalid() || p1.IsInvalid())
            {
                return;
            }

            
            HeModel he = tfig.mHeModel;
            CadMesh src = HeModelConverter.ToCadMesh(he);

            Vector3d normal = CadMath.Normal(
                p1 - p0, (Controller.DC.ViewDir));

            (CadMesh m1, CadMesh m2) = MeshUtil.CutMeshWithVector(src, p0, p1, normal);

            if (m1 == null || m2 == null)
            {
                ItConsole.println("Can not cut a mesh. id:" + id);
                return;
            }

            CadFigureMesh fig1 = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
            fig1.SetMesh(HeModelConverter.ToHeModel(m1));

            CadFigureMesh fig2 = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
            fig2.SetMesh(HeModelConverter.ToHeModel(m2));

            CadOpe ope;

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig1.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig1);

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig2.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.AddFigure(fig2);

            if (!Session.StartWithSnapshotDB)
            {
                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, tfig.ID);
                Session.AddOpe(ope);
            }

            Controller.CurrentLayer.RemoveFigureByID(tfig.ID);

            Controller.ClearSelection();

            Session.PostRemakeObjectTree();
        }

        public void DumpMesh(uint id)
        {
            CadFigureMesh fig = GetCadFigureMesh(id);

            if (fig == null)
            {
                ItConsole.println("dumpMesh(id) error: invalid ID");
                return;
            }

            CadMesh cm = HeModelConverter.ToCadMesh(fig.mHeModel);

            for (int i = 0; i < cm.VertexStore.Count; i++)
            {
                CadVertex v = cm.VertexStore[i];
                ItConsole.printf("{0}:{1},{2},{3}\n", i, v.X, v.Y, v.Z);
            }

            for (int i = 0; i < cm.FaceStore.Count; i++)
            {
                CadFace f = cm.FaceStore[i];

                string s = "";

                for (int j = 0; j < f.VList.Count; j++)
                {
                    s += f.VList[j].ToString() + ",";
                }

                ItConsole.println(s);
            }
        }

        public Vector3d RotateVector(Vector3d v, Vector3d axis, double angle)
        {
            axis = axis.UnitVector();

            double t = CadMath.Deg2Rad(angle);

            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, t);
            CadQuaternion r = q.Conjugate(); ;

            CadQuaternion qp;

            qp = CadQuaternion.FromPoint(v);

            qp = r * qp;
            qp = qp * q;

            Vector3d rv = v;

            rv = qp.ToPoint();

            return rv;
        }

        public uint GetCurrentFigureID()
        {
            CadFigure fig = Controller.CurrentFigure;

            if (fig == null)
            {
                return 0;
            }

            return fig.ID;
        }

        public CadFigure GetCurrentFigure()
        {
            return Controller.CurrentFigure;
        }

        public Vector3d InputPoint()
        {
            Env.OpenPopupMessage("Input point", UITypes.MessageType.INPUT);

            InteractCtrl ctrl = Controller.InteractCtrl;

            ctrl.Start();

            ItConsole.print(AnsiEsc.Yellow + "Input point >> " + AnsiEsc.Reset);

            InteractCtrl.States ret = ctrl.WaitPoint();
            ctrl.End();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                Env.ClosePopupMessage();
                ItConsole.println("Cancel!");
                return VectorExt.InvalidVector3d;
            }

            Vector3d p = ctrl.PointList[0];

            ItConsole.println(p.CoordString());

            Env.ClosePopupMessage();

            return p;
        }

        public string GetString(string msg, string defStr)
        {
            string s = null;

            ThreadUtil.RunOnMainThread(() => {
                    s = ItConsole.getString(msg, defStr);
                }, true);

            return s;
        }

        public Vector3d ViewDir()
        {
            return Controller.DC.ViewDir;
        }

        public Vector3d InputUnitVector()
        {
            InteractCtrl ctrl = Controller.InteractCtrl;

            ctrl.Start();

            ItConsole.println(AnsiEsc.Yellow + "Input point 1 >>");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return VectorExt.InvalidVector3d;
            }

            Vector3d p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());

            ItConsole.println(AnsiEsc.Yellow + "Input point 2 >>");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return VectorExt.InvalidVector3d;
            }

            Vector3d p1 = Controller.InteractCtrl.PointList[1];

            ctrl.End();


            Vector3d v = p1 - p0;

            v = v.UnitVector();

            ItConsole.println(p1.CoordString());

            return v;
        }

        public (Vector3d, Vector3d) InputLine()
        {
            InteractCtrl ctrl = Controller.InteractCtrl;

            ctrl.Start();

            ItConsole.print(AnsiEsc.Yellow + "Input point 1 >>" + AnsiEsc.Reset + " ");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return (VectorExt.InvalidVector3d, VectorExt.InvalidVector3d);
            }

            Vector3d p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());

            ItConsole.print(AnsiEsc.Yellow + "Input point 2 >>" + AnsiEsc.Reset + " ");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return (VectorExt.InvalidVector3d, VectorExt.InvalidVector3d);
            }

            Vector3d p1 = Controller.InteractCtrl.PointList[1];
            ItConsole.println(p1.CoordString());

            ctrl.End();

            return (p0, p1);
        }

        public void UpdateTV()
        {
            Env.RunOnMainThread(() =>
            {
                Controller.UpdateObjectTree(true);
            });
        }

        public void Redraw()
        {
            Env.RunOnMainThread(() =>
            {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushToView();
            });
        }

        public CadFigure CreatePolyLines()
        {
            CadFigurePolyLines fig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            return fig;
        }

        public void AddFigure(CadFigure fig)
        {
            lock (Controller.DB)
            {
                Controller.DB.AddFigure(fig);
                Controller.CurrentLayer.AddFigure(fig);

                if (!Session.StartWithSnapshotDB)
                {
                    CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                    Session.AddOpe(ope);
                }
            }
        }

        public void SetColor(float r, float g, float b)
        {
            List<CadFigure> figs = GetSlectedFigList();

            Color4 color = new Color4(r, g, b, 1.0f);

            foreach (CadFigure fig in figs)
            {
                for (int i = 0; i < fig.PointList.Count; i++)
                {
                    if (!fig.PointList[i].Selected) continue;
                    fig.PointList[i].Attr.Color1 = color;
                    fig.PointList[i].Attr.IsColor1Valid = true;
                    fig.PointList[i].Attr.Color2 = color;
                    fig.PointList[i].Attr.IsColor2Valid = true;
                }
            }
        }

        public void SetColor(uint figID, float r, float g, float b)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);
            if (fig == null)
            {
                return;
            }

            float w = 1.0f;

            if (fig.LinePen.IsInvalid)
            {
                w = fig.LinePen.Width;
            }

            fig.LinePen = new DrawPen(new Color4(r, g, b, 1.0f), w);
        }

        public void SetFillColor(uint figID, float r, float g, float b)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);
            if (fig == null)
            {
                return;
            }

            fig.FillBrush = new DrawBrush(new Color4(r, g, b, 1.0f));
        }

        public List<CadFigure> GetSlectedFigList()
        {
            return Controller.DB.GetSelectedFigList();
        }

        public void Test()
        {
            CadFigure baseFig = GetTargetFigure();

            if (baseFig == null)
            {
                return;
            }

            if (baseFig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            (Vector3d p1, Vector3d p2) = InputLine();

            Vector3d org = p1;
            Vector3d axis = (p2 - p1).Normalized();

            CadMesh cm = MeshMaker.CreateRotatingBody(32, org, axis, baseFig.PointList, false, false, MeshMaker.FaceType.QUADRANGLE);

            CadFigure fig = MesthToFig(cm);

            if (!Session.StartWithSnapshotDB)
            {
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeObjectTree();
        }

        private void testDraw()
        {
            CadSize2D deviceSize = new CadSize2D(827, 1169);
            CadSize2D pageSize = new CadSize2D(210, 297);

            DrawContext dc = Controller.DC.CreatePrinterContext(pageSize, deviceSize);
            dc.SetupTools(DrawTools.DrawMode.PRINTER);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            Controller.DrawFiguresRaw(dc);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            bmp.Save(@"F:\work\test2.bmp");
        }

        public CadFigure GetTargetFigure()
        {
            List<uint> idlist = Controller.DB.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                return null;
            }

            return Controller.DB.GetFigure(idlist[0]);
        }
    }
}
