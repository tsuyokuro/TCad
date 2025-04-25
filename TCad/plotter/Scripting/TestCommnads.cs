using CadDataTypes;
using CarveWapper;
using GLFont;
using GLUtil;
using LibiglWrapper;
using MeshMakerNS;
using OpenGL.GLU;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Plotter;
using Plotter.Controller;
using Plotter.svg;
using SharpFont;
using SplineCurve;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TCad.Controls;
using TCad.plotter.Model.HalfEdge;
using TCad.plotter.undo;

namespace TCad.plotter.Scripting;

public class TestCommands
{
    IPlotterController Controller;

    public TestCommands(IPlotterController controller)
    {
        Controller = controller;
    }

    private void test001()
    {
    }

    private void test002()
    {
        CadMesh cm = MeshMaker.CreateSphere(new vector3_t(0, 0, 0), 20, 16, 16);

        HeModel hem = HeModelConverter.ToHeModel(cm);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
        Controller.HistoryMan.foward(ope);
        Controller.CurrentLayer.AddFigure(fig);
        Controller.UpdateObjectTree(true);
    }

    private void test003()
    {
        CadFigure tfig = GetTargetFigure();

        if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
        {
            return;
        }


        CadMesh cm = MeshMaker.CreateExtruded(tfig.GetPoints(16), vector3_t.UnitZ * -20);

        HeModel hem = HeModelConverter.ToHeModel(cm);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
        Controller.HistoryMan.foward(ope);
        Controller.CurrentLayer.AddFigure(fig);
        Controller.UpdateObjectTree(true);
    }

    private void test004()
    {
        ItConsole.println("\x1b[33mTest1\x1b[00mテスト\x1b[36mTest3");
    }

    private void test005()
    {
        CadFigure fig = GetTargetFigure();

        if (fig.PointCount < 3)
        {
            return;
        }

        vector3_t v1 = fig.PointList[0].vector - fig.PointList[1].vector;
        vector3_t v2 = fig.PointList[2].vector - fig.PointList[1].vector;

        vcompo_t t = CadMath.AngleOfVector(v1, v2);

        vcompo_t a = CadMath.Rad2Deg(t);

        ItConsole.println(string.Format("angle:{0}(deg)", a));
    }

    private void test006()
    {
        DrawContext dc = Controller.DC;

        CadObjectDB db = Controller.DB;

        List<uint> idlist = Controller.DB.GetSelectedFigIDList();

        int i;
        for (i = 0; i < idlist.Count; i++)
        {
            CadFigure fig = db.GetFigure(idlist[i]);

            if (fig == null)
            {
                continue;
            }

            int j;
            for (j = 0; j < fig.PointList.Count; j++)
            {
                if (!fig.PointList[j].Selected)
                {
                    continue;
                }

                CadVertex p = fig.PointList[j];

                p.Z = 0;

                fig.PointList[j] = p;
            }
        }
    }

    private void test009()
    {
        DrawContext dc = Controller.DC;

        CadObjectDB db = Controller.DB;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        int i = 0;
        int layerCnt = db.LayerList.Count;

        for (; i < layerCnt; i++)
        {
            CadLayer layer = db.LayerList[i];

            int j = 0;
            int figCnt = layer.FigureList.Count;

            for (; j < figCnt; j++)
            {
                CadFigure fig = layer.FigureList[j];

                int k = 0;
                int pcnt = fig.PointList.Count;

                for (; k < pcnt; k++)
                {
                    CadVertex p = fig.PointList[k];
                    CadVertex sp = dc.WorldPointToDevPoint(p);
                }
            }
        }

        sw.Stop();
        Log.pl(sw.ElapsedMilliseconds.ToString() + " milli sec");
    }

    private void test010()
    {
        MinMax2D mm = MinMax2D.Create();
    }

    private void formatTest(string format, params object[] args)
    {
        string s = string.Format(format, args);
    }

    private void test011()
    {
        //formatTest("{0},{1}", 10, 20);

        ItConsole.printf("{0},{1}\n", 10, 20);
        ItConsole.print("test");
        ItConsole.println("_test");
        ItConsole.println("abc\ndef");
        ItConsole.println("end");
    }

    private void test012()
    {
        CadFigure fig = GetTargetFigure();

        if (fig == null)
        {
            return;
        }

        List<Vector3List> triangles = TriangleSplitter.Split(fig);

        foreach (Vector3List triangle in triangles)
        {
            CadFigure tfig = CadFigure.Create(CadFigure.Types.POLY_LINES);
            foreach (vector3_t v in triangle)
            {
                tfig.AddPoint(new CadVertex(v));
            }

            Controller.TempFigureList.Add(tfig);
        }
    }

    private void testMesh()
    {
        var figlist = Controller.DB.GetSelectedFigList();

        CadOpeList opeRoot = new CadOpeList();

        CadOpe ope;

        for (int i = 0; i < figlist.Count; i++)
        {
            CadFigure fig = figlist[i];

            if (fig == null)
            {
                continue;
            }

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                continue;
            }

            CadFigureMesh mesh = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            mesh.CreateModel(fig);


            ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, mesh.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.AddFigure(mesh);


            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.RemoveFigureByID(fig.ID);
        }

        if (opeRoot.OpeList.Count > 0)
        {
            Controller.HistoryMan.foward(opeRoot);
        }

        Controller.UpdateObjectTree(true);
    }

    private void test013()
    {
        ItConsole.println("test013 start");
        test013sub();
        ItConsole.println("test013 end");
    }

    private async void test013sub()
    {
        ItConsole.println("test013Sub start");

        await Task.Run(() =>
        {
            ItConsole.println("Run");
            Thread.Sleep(2000);
            ItConsole.println("Run end");
        });

        ItConsole.println("test013Sub end");
    }

    private void testLoadOff()
    {
        string fname = @"F:\TestFiles\bunny.off";

        CadMesh cm = IglW.ReadOFF(fname);

        HeModel hem = HeModelConverter.ToHeModel(cm);

        for (int i = 0; i < hem.VertexStore.Count; i++)
        {
            hem.VertexStore[i] *= (vcompo_t)500.0;
        }

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        Controller.CurrentLayer.AddFigure(fig);
    }

    private void testAminusB()
    {
        List<CadFigure> figList = Controller.DB.GetSelectedFigList();

        if (figList.Count < 2)
        {
            return;
        }

        if (figList[0].Type != CadFigure.Types.MESH)
        {
            return;
        }

        if (figList[1].Type != CadFigure.Types.MESH)
        {
            return;
        }

        CadFigureMesh fig_a = (CadFigureMesh)figList[0];
        CadFigureMesh fig_b = (CadFigureMesh)figList[1];

        if (fig_a.Current)
        {
            CadFigureMesh t = fig_a;
            fig_a = fig_b;
            fig_b = t;
        }

        ItConsole.println("ID:" + fig_a.ID.ToString() + " - ID:" + fig_b.ID.ToString());

        HeModel he_a = fig_a.mHeModel;
        HeModel he_b = fig_b.mHeModel;

        CadMesh a = HeModelConverter.ToCadMesh(he_a);
        CadMesh b = HeModelConverter.ToCadMesh(he_b);

        CadMesh c = CarveW.AMinusB(a, b);


        HeModel hem = HeModelConverter.ToHeModel(c);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        Controller.CurrentLayer.AddFigure(fig);
    }

    private void testInvert()
    {
        CadFigure fig = GetTargetFigure();

        fig.InvertDir();
    }


    private void testLoadDxf()
    {
        CadDxfLoader loader = new CadDxfLoader();

        CadMesh cm = loader.Load(@"H:\work\恐竜.DXF", (vcompo_t)20.0);

        HeModel hem = HeModelConverter.ToHeModel(cm);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        Controller.CurrentLayer.AddFigure(fig);

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
        });

        Redraw();
    }

    private void testNu()
    {
    }

    private void testNus()
    {
        CadFigureNurbsSurface nfig = (CadFigureNurbsSurface)Controller.DB.NewFigure(CadFigure.Types.NURBS_SURFACE);

        int ucnt = 8;
        int vcnt = 5;

        VertexList vl = SplineUtil.CreateFlatControlPoints(ucnt, vcnt, vector3_t.UnitX * (vcompo_t)20.0, vector3_t.UnitZ * (vcompo_t)20.0);

        nfig.Setup(2, ucnt, vcnt, vl, null, 16, 16);


        Controller.CurrentLayer.AddFigure(nfig);

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
        });
    }

    private void testNus2()
    {
        CadFigureNurbsSurface nfig = (CadFigureNurbsSurface)Controller.DB.NewFigure(CadFigure.Types.NURBS_SURFACE);

        int ucnt = 4;
        int vcnt = 4;

        VertexList vl = SplineUtil.CreateBoxControlPoints(
            ucnt, vcnt, vector3_t.UnitX * (vcompo_t)20.0, vector3_t.UnitZ * (vcompo_t)20.0, vector3_t.UnitY * (vcompo_t)(-20.0));

        nfig.Setup(2, ucnt * 2, vcnt, vl, null, 16, 16, false, false, true, true);

        Controller.CurrentLayer.AddFigure(nfig);

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
        });
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

    private void GetPointsTest()
    {
        CadFigure fig = Controller.Input.CurrentFigure;

        if (fig == null) return;

        VertexList vl = fig.GetPoints(4);

        int a = vl.Count;

        CadFigurePolyLines tmpFig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

        tmpFig.AddPoints(vl);

        Controller.CurrentLayer.AddFigure(tmpFig);

        Controller.UpdateObjectTree(true);
    }

    private void testTriangulate()
    {
        CadFigure tfig = GetTargetFigure();
        if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
        {
            return;
        }

        if (tfig.PointCount < 3)
        {
            return;
        }

        VertexList vl = tfig.GetPoints(12);

        CadMesh m = IglW.Triangulate(vl, "a1000q");

        HeModel hem = HeModelConverter.ToHeModel(m);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        Controller.CurrentLayer.AddFigure(fig);
    }

    private void testSvg()
    {
        List<CadFigure> figList = Controller.DB.GetSelectedFigList();

        SvgExporter svgExporter = new SvgExporter();

        XDocument doc = svgExporter.ToSvg(figList,
                    Controller.DC,
                    Controller.PageSize.Width,
                    Controller.PageSize.Height);

        Log.pl(doc.ToString());
        doc.Save(@"f:\work2\test.svg");
    }

    private void Test()
    {
        CadFigure fig = Controller.Input.CurrentFigure;

        fig.LinePen = new DrawPen(new Color4(0.5f, 0.5f, 1.0f, 1f), 1.0f);
        fig.FillBrush = new DrawBrush(new Color4(0.5f, 0.5f, 0.5f, 1f));
    }

    private void Test2()
    {
        DrawPen dpen1 = new DrawPen(new Color4(0.25f, 0.25f, 0.25f, 1), 1);
        DrawPen dpen2 = new DrawPen(new Color4(0.25f, 0.25f, 0.25f, 1), 1);
        DrawPen dpen3 = new DrawPen(new Color4(0.25f, 0.25f, 0.2f, 1), 1);

        DrawBrush dbr1 = new DrawBrush(new Color4(0.25f, 0.25f, 0.25f, 1));
        DrawBrush dbr2 = new DrawBrush(new Color4(0.25f, 0.25f, 0.25f, 1));
        DrawBrush dbr3 = new DrawBrush(new Color4(0.25f, 0.25f, 0.2f, 1));

        Pen pen1 = dpen1.GdiPen;
        Pen pen2 = dpen2.GdiPen;
        Pen pen3 = dpen3.GdiPen;

        Log.pl("pen1==pen2: " + ReferenceEquals(pen1, pen2));
        Log.pl("pen1==pen3: " + ReferenceEquals(pen1, pen3));


        SolidBrush br1 = dbr1.GdiBrush;
        SolidBrush br2 = dbr2.GdiBrush;
        SolidBrush br3 = dbr3.GdiBrush;

        Log.pl("br1==br2: " + ReferenceEquals(pen1, pen2));
        Log.pl("br1==br3: " + ReferenceEquals(pen1, pen3));
    }

    private void Test3()
    {
        //FontFaceW fw = FontFaceW.Provider.GetFromResource("/Fonts/mplus-1m-regular.ttf", 48, 0);
        FontFaceW fw = GLUtilContainer.FontFaceProvider.Instance.FromFile("C:\\Windows\\Fonts\\msgothic.ttc", 48, 0);
        GlyphSlot glyph = fw.GetGlyph('A');

        Outline outline = glyph.Outline;

        int idx = 0;

        CadVertex v = new CadVertex();

        for (int i = 0; i < outline.ContoursCount; i++)
        {
            CadFigurePolyLines tmpFig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = outline.Points[idx];
                v.X = (vcompo_t)fv.X * (vcompo_t)100.0;
                v.Y = (vcompo_t)fv.Y * (vcompo_t)100.0;
                v.Z = 0;

                tmpFig.AddPoint(v);

                idx++;
            }

            tmpFig.IsLoop = true;
            Controller.CurrentLayer.AddFigure(tmpFig);
        }

        Controller.UpdateObjectTree(true);

        //IntPtr htess = Glu.NewTess();
        //ItConsole.println("test3 htess:" + htess.ToString("x16"));

        //Glu.DeleteTess(htess);
    }



    public void BeginCB(int mode)
    {
        BeginMode beginMode = (BeginMode)mode;
        Log.pl("MeshBegin mode:" + beginMode.ToString());
    }

    public void EndCB()
    {
        Log.pl("MeshEnd");
    }

    public void VertexCB(nint data)
    {
        int vIndex = (int)data;
        Log.pl("VertexCB vIndex:" + vIndex);
    }

    private void CombineCB([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] double[] coords,
                                [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] double[] data,
                                [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] weight,
                                ref nint dataOut)
    {
        Log.pl("MeshCombine");
        dataOut = nint.Zero;
    }

    void ErrorCB(int err)
    {
        Log.pl("MeshError err:" + err);
    }

    private void Test4()
    {
        FontFaceW fw = GLUtilContainer.FontFaceProvider.Instance.FromResource("/Fonts/mplus-1m-regular.ttf", 48, 0);
        //FontFaceW fw = FontFaceW.Provider.GetFromFile("C:\\Windows\\Fonts\\msgothic.ttc", 48, 0);
        GlyphSlot glyph = fw.GetGlyph('A');

        Outline outline = glyph.Outline;


        nint htess = Glu.NewTess();
        ItConsole.println("test4 htess:" + htess.ToString("x16"));

        Glu.TessCallback(htess, GluTessCallback.Begin, new Glu.TessBeginCallback(BeginCB));
        Glu.TessCallback(htess, GluTessCallback.End, new Glu.TessEndCallback(EndCB));
        Glu.TessCallback(htess, GluTessCallback.Vertex, new Glu.TessVertexCallback(VertexCB));
        Glu.TessCallback(htess, GluTessCallback.TessCombine, new Glu.TessCombineCallback(CombineCB));
        Glu.TessCallback(htess, GluTessCallback.TessError, new Glu.TessErrorCallback(ErrorCB));

        vcompo_t[] va = new vcompo_t[3];

        Glu.TessNormal(htess, new vector3_t(0f, 0f, 1f));

        Glu.TessBeginPolygon(htess, 128);

        double[] tv = new double[3];

        int idx = 0;
        for (int i = 0; i < outline.ContoursCount; i++)
        {
            Glu.TessBeginContour(htess);

            int n = outline.Contours[i];
            for (; idx <= n;)
            {
                FTVector fv = outline.Points[idx];
                tv[0] = (vcompo_t)fv.X * (vcompo_t)100.0;
                tv[1] = (vcompo_t)fv.Y * (vcompo_t)100.0;
                tv[2] = 0;

                Glu.TessVertex(htess, tv, idx);

                idx++;
            }

            Glu.TessEndContour(htess);
        }

        Glu.TessEndPolygon(htess);

        Glu.DeleteTess(htess);
    }

    public void Test5()
    {
        FontFaceW fw = GLUtilContainer.FontFaceProvider.Instance.FromResource("/Fonts/mplus-1m-regular.ttf", 48, 0);
        GlyphSlot glyph = fw.GetGlyph('あ');

        Tessellator tesse = new();

        FontPoly fontPoly = FontTessellator.TessellateRaw(glyph, 4, tesse);

        tesse.Dispose();

        for (int i = 0; i < fontPoly.Mesh.VertexStore.Count; i++)
        {
            fontPoly.Mesh.VertexStore[i] *= (vcompo_t)400.0;
        }

        HeModel hem = HeModelConverter.ToHeModel(fontPoly.Mesh);

        CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

        fig.SetMesh(hem);

        Controller.CurrentLayer.AddFigure(fig);

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
            Controller.Drawer.Redraw();
        });
    }

    private void Test6()
    {
        CadFigure cfig = Controller.Input.CurrentFigure;
        List<vector3_t> vl = CadUtil.Getvector3_tListFrom(cfig);

        vector3_t startv = vl[0];

        List<vector3_t> splineList = FontTessellator.BSpline2D(vl, 4);

        CadFigurePolyLines tmpFig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

        CadUtil.SetVertexListTo(tmpFig, splineList);

        Controller.CurrentLayer.AddFigure(tmpFig);
    }

    private void Test7()
    {
        string fontFName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msmincho.ttc");
        FontFaceW fw = GLUtilContainer.FontFaceProvider.Instance.FromFile(fontFName, 48, 0);

        GlyphSlot glyph = fw.GetGlyph('い');

        List<vector3_t> cvl = new();

        Tessellator tesse = new();

        FontPoly fontPoly = FontTessellator.Tessellate(glyph, 4, tesse);

        tesse?.Dispose();

        //FontPoly cpyFontpoly = new(fontPoly);

        cvl.Clear();
        for (int i = 0; i < fontPoly.ContourList.Count; i++)
        {
            List<int> cont = fontPoly.ContourList[i];

            cvl.Clear();
            for (int j = 0; j < cont.Count; j++)
            {
                cvl.Add(fontPoly.VertexList[cont[j]]);
            }

            CreatePolyLines(cvl, (vcompo_t)400.0, true);
        }

        if (fontPoly.Mesh != null)
        {
            for (int i = 0; i < fontPoly.Mesh.VertexStore.Count; i++)
            {
                fontPoly.Mesh.VertexStore[i] *= (vcompo_t)400.0;
            }

            HeModel hem = HeModelConverter.ToHeModel(fontPoly.Mesh);
            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
            fig.SetMesh(hem);
            Controller.CurrentLayer.AddFigure(fig);
        }

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
            Controller.Drawer.Redraw();
        });
    }


    private void Test8()
    {
        FontFaceW fw = GLUtilContainer.FontFaceProvider.Instance.FromResource("/Fonts/mplus-1m-regular.ttf", 48, 0);

        //string fontFName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msgothic.ttc");
        //string fontFName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msmincho.ttc");
        //FontFaceW fw = FontFaceW.Provider.GetFromFile(fontFName, 48, 0);

        FontPoly fontPoly = fw.CreatePoly('お');

        if (fontPoly.Mesh != null)
        {
            for (int i = 0; i < fontPoly.Mesh.VertexStore.Count; i++)
            {
                fontPoly.Mesh.VertexStore[i] *= (vcompo_t)400.0;
            }

            HeModel hem = HeModelConverter.ToHeModel(fontPoly.Mesh);
            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);
            fig.SetMesh(hem);
            Controller.CurrentLayer.AddFigure(fig);
        }

        RunOnMainThread(() =>
        {
            Controller.UpdateObjectTree(true);
            Controller.Drawer.Redraw();
        });
    }

    private void CreatePolyLines(List<vector3_t> vl, vcompo_t scale, bool isLoop)
    {
        CadFigurePolyLines tmpFig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

        tmpFig.IsLoop = isLoop;

        CadUtil.SetVertexListTo(tmpFig, vl);

        for (int i = 0; i < tmpFig.PointList.Count; i++)
        {
            tmpFig.PointList[i] *= scale;
        }

        Controller.CurrentLayer.AddFigure(tmpFig);
    }

    private void Test9()
    {
        RunOnMainThread(() =>
        {
            int name1 = GLUtilContainer.TextureProvider.Instance.GetNew();
            int name2 = GLUtilContainer.TextureProvider.Instance.GetNew();
            Log.pl("end");
        });
    }

    public bool ExecCommand(string s)
    {
        string[] ss = Regex.Split(s, @"[ \t]+");

        string cmd = ss[0];

        if (cmd == "@loadOff")
        {
            testLoadOff();
        }

        else if (cmd == "@svg")
        {
            testSvg();
        }

        else if (cmd == "@triangle")
        {
            testTriangulate();
        }

        else if (cmd == "@nu")
        {
            testNu();
        }
        else if (cmd == "@nus")
        {
            testNus();
        }
        else if (cmd == "@nus2")
        {
            testNus2();
        }
        else if (cmd == "@testMesh")
        {
            testMesh();
        }
        else if (cmd == "@testInvert")
        {
            testInvert();
        }

        else if (cmd == "@loadDxf")
        {
            testLoadDxf();

        }

        else if (cmd == "@test")
        {
            Test();
        }

        else if (cmd == "@test2")
        {
            Test2();
        }
        else if (cmd == "@test3")
        {
            Test3();
        }
        else if (cmd == "@test4")
        {
            Test4();
        }
        else if (cmd == "@test5")
        {
            Test5();
        }
        else if (cmd == "@test6")
        {
            Test6();
        }
        else if (cmd == "@test7")
        {
            Test7();
        }
        else if (cmd == "@test8")
        {
            Test8();
        }
        else if (cmd == "@test9")
        {
            Test9();
        }

        else if (cmd == "@tcons1")
        {
            ItConsole.println("test");
        }
        else if (cmd == "@tcons2")
        {
            ItConsole.println("test" + AnsiEsc.BCyan + "-cyan-" + AnsiEsc.Reset + "abc");
        }
        else if (cmd == "@tcons3")
        {
            ItConsole.print("test");
            ItConsole.print(AnsiEsc.BGreen + "xx");
            ItConsole.print("-Green!!!");
            ItConsole.print(AnsiEsc.Reset);
            ItConsole.print("abc");
            ItConsole.print("\n");
        }
        else if (cmd == "@tcons4")
        {
            ItConsole.print("1/5");
            Thread.Sleep(1000);
            ItConsole.print("\r2/5");
            Thread.Sleep(1000);
            ItConsole.print("\r3/5");
            Thread.Sleep(1000);
            ItConsole.print("\r4/5");
            Thread.Sleep(1000);
            ItConsole.print("\r5/5");
            ItConsole.print("\nFinish!\n");
        }
        else
        {
            return false;
        }

        return true;
    }

    public void Redraw()
    {
        RunOnMainThread(() =>
        {
            Controller.Drawer.Clear();
            Controller.Drawer.DrawAll();
            Controller.Drawer.UpdateView();
        });
    }

    public void RunOnMainThread(Action action)
    {
        ThreadUtil.RunOnMainThread(action, true);
    }
}
