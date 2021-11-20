using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using HalfEdgeNS;
using GLFont;
using OpenTK.Graphics;
using CadDataTypes;
using Plotter.Settings;

namespace Plotter
{
    public class DrawingGL : IDrawing
    {
        private DrawContextGL DC;

        private FontFaceW mFontFaceW;
        private FontRenderer mFontRenderer;

        private double FontTexW;
        private double FontTexH;

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;

            mFontFaceW = new FontFaceW();
            //mFontFaceW.SetFont(@"C:\Windows\Fonts\msgothic.ttc", 0);
            mFontFaceW.SetResourceFont("/Fonts/mplus-1m-regular.ttf");
            mFontFaceW.SetSize(24);

            mFontRenderer = new FontRenderer();
            mFontRenderer.Init();

            FontTex tex = mFontFaceW.CreateTexture("X");
            FontTexW = tex.ImgW;
            FontTexH = tex.ImgH;
        }

        public void Dispose()
        {
            mFontRenderer.Dispose();
        }

        public void Clear(DrawBrush brush)
        {
            GL.ClearColor(brush.Color4());
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void DrawSelected(List<CadFigure> list)
        {
            DisableLight();

            foreach (CadFigure fig in list)
            {
                fig.DrawSelectedEach(DC);
            }
        }

        public void DrawLine(DrawPen pen, Vector3d a, Vector3d b)
        {
            a *= DC.WorldScale;
            b *= DC.WorldScale;

            GL.Color4(pen.Color4());

            GL.Begin(PrimitiveType.LineStrip);

            GL.Vertex3(a);
            GL.Vertex3(b);

            GL.End();
        }

        private void DrawLineRaw(DrawPen pen, Vector3d a, Vector3d b)
        {
            GL.Color4(pen.Color4());

            GL.Begin(PrimitiveType.LineStrip);

            GL.Vertex3(a);
            GL.Vertex3(b);

            GL.End();
        }

        public void DrawHarfEdgeModel(
            DrawBrush brush, DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            DrawHeFaces(brush, model);
            DrawHeEdges(pen, edgePen, edgeThreshold, model);

            if (SettingsHolder.Settings.DrawNormal)
            {
                DrawHeFacesNormal(model);
            }
        }

        private void DrawHeFaces(DrawBrush brush, HeModel model)
        {
            if (brush.IsNullBrush)
            {
                return;
            }

            EnableLight();

            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                GL.Begin(PrimitiveType.Polygon);
                GL.Color4(brush.Color4());

                if (f.Normal != HeModel.INVALID_INDEX)
                {
                    Vector3d nv = model.NormalStore[f.Normal];
                    GL.Normal3(nv);
                }

                for (; ; )
                {
                    GL.Vertex3((model.VertexStore.Ref(c.Vertex).vector * DC.WorldScale));

                    c = c.Next;

                    if (c == head)
                    {
                        break;
                    }
                }

                GL.End();
            }

            DisableLight();
        }

        private void DrawHeFacesNormal(HeModel model)
        {
            DisableLight();

            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;


                for (; ; )
                {
                    HalfEdge next = c.Next;

                    Vector3d p = model.VertexStore.Ref(c.Vertex).vector;

                    if (c.Normal != HeModel.INVALID_INDEX)
                    {
                        Vector3d nv = model.NormalStore[c.Normal];
                        Vector3d np0 = p;
                        Vector3d np1 = p + (nv * 10);

                        DrawArrow(DC.GetPen(DrawTools.PEN_NORMAL), np0, np1, ArrowTypes.CROSS, ArrowPos.END, 3, 3);
                    }

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }

            EnableLight();
        }

        private void DrawHeEdges(DrawPen borderPen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            bool drawBorder = !borderPen.IsNullPen;
            bool drawEdge = !edgePen.IsNullPen;

            if (!drawBorder && !drawEdge)
            {
                return;
            }

            DisableLight();

            GL.LineWidth(1.0f);

            Color4 color = borderPen.Color4();
            Color4 edgeColor = edgePen.Color4();

            Vector3d shift = GetShiftForOutLine();

            Vector3d p0;
            Vector3d p1;

            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                HalfEdge pair;

                p0 = model.VertexStore.Ref(c.Vertex).vector * DC.WorldScale + shift;

                for (; ; )
                {
                    bool drawAsEdge = false;

                    pair = c.Pair;

                    if (drawEdge)
                    {
                        if (pair == null)
                        {
                            drawAsEdge = true;
                        }
                        else
                        {
                            if (edgeThreshold != 0)
                            {
                                double s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                                if (Math.Abs(s) <= edgeThreshold)
                                {
                                    drawAsEdge = true;
                                }
                            }
                        }
                    }

                    p1 = model.VertexStore.Ref(c.Next.Vertex).vector * DC.WorldScale + shift;

                    if (drawAsEdge)
                    {
                        GL.Color4(edgeColor);
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(p0);
                        GL.Vertex3(p1);
                        GL.End();
                    }
                    else
                    {
                        if (drawBorder)
                        {
                            GL.Color4(color);
                            GL.Begin(PrimitiveType.Lines);
                            GL.Vertex3(p0);
                            GL.Vertex3(p1);
                            GL.End();
                        }
                    }

                    p0 = p1;

                    c = c.Next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        private const double AXIS_MARGIN = 24;

        public void DrawAxis()
        {
            Vector3d p0;
            Vector3d p1;

            double wh = Math.Min(DC.ViewWidth, DC.ViewHeight)/2;


            //double len = DrawingConst.AxisLength;
            //double arrowLen = 4.0 / DC.WorldScale;
            //double arrowW2 = 2.0 / DC.WorldScale;

            double len = DC.DevSizeToWoldSize(wh - AXIS_MARGIN) * DC.WorldScale;
            double arrowLen = DC.DevSizeToWoldSize(16) * DC.WorldScale;
            double arrowW2 = DC.DevSizeToWoldSize(8) * DC.WorldScale;

            // X軸
            p0 = new Vector3d(-len, 0, 0) / DC.WorldScale;
            p1 = new Vector3d(len, 0, 0) / DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }

            // Y軸
            p0 = new Vector3d(0, -len, 0) / DC.WorldScale;
            p1 = new Vector3d(0, len, 0) / DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }

            // Z軸
            p0 = new Vector3d(0, 0, -len) / DC.WorldScale;
            p1 = new Vector3d(0, 0, len) / DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }
        }

        public void DrawAxisLabel()
        {
            Vector3d p1;
            Vector3d pp;

            //double len = DrawingConst.AxisLength;
            double wh = Math.Min(DC.ViewWidth, DC.ViewHeight) / 2;
            double len = DC.DevSizeToWoldSize(wh - AXIS_MARGIN) * DC.WorldScale;

            DrawTextOption opt = default;

            // X軸
            p1 = new Vector3d(len, 0, 0) / DC.WorldScale;

            pp = DC.WorldPointToDevPoint(p1);
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_X), pp, Vector3d.UnitX, -Vector3d.UnitY, "X", 0.6, opt);

            // Y軸
            p1 = new Vector3d(0, len, 0) / DC.WorldScale;

            pp = DC.WorldPointToDevPoint(p1);
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_Y), pp, Vector3d.UnitX, -Vector3d.UnitY, "Y", 0.6, opt);

            // Z軸
            p1 = new Vector3d(0, 0, len) / DC.WorldScale;

            pp = DC.WorldPointToDevPoint(p1);
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_Z), pp, Vector3d.UnitX, -Vector3d.UnitY, "Z", 0.6, opt);
        }

        public void DrawCompass()
        {
            DrawCompassPers();
            //DrawCompassOrtho();
        }

        private void DrawCompassPers()
        {
            PushMatrixes();

            double size = 80;

            double vw = DC.ViewWidth;
            double vh = DC.ViewHeight;

            double cx = size / 2 + 8;
            double cy = size / 2 + 20;

            double left = -cx;
            double right = vw - cx;
            double top = cy;
            double bottom = -(vh - cy);

            double arrowLen = 20;
            double arrowW2 = 10;

            Matrix4d prjm = Matrix4d.CreatePerspectiveOffCenter(left, right, bottom, top, 100, 10000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref prjm);

            GL.MatrixMode(MatrixMode.Modelview);
            Vector3d lookAt = Vector3d.Zero;
            Vector3d eye = -DC.ViewDir * 220;

            Matrix4d mdlm = Matrix4d.LookAt(eye, lookAt, DC.UpVector);

            GL.LoadMatrix(ref mdlm);

            Vector3d p0;
            Vector3d p1;

            GL.LineWidth(1);

            p0 = Vector3d.UnitX * -size;
            p1 = Vector3d.UnitX * size;
            DrawArrowRaw(DC.GetPen(DrawTools.PEN_COMPASS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            p0 = Vector3d.UnitY * -size;
            p1 = Vector3d.UnitY * size;
            DrawArrowRaw(DC.GetPen(DrawTools.PEN_COMPASS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            p0 = Vector3d.UnitZ * -size;
            p1 = Vector3d.UnitZ * size;
            DrawArrowRaw(DC.GetPen(DrawTools.PEN_COMPASS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            GL.LineWidth(1);

            double fontScale = 0.6;
            DrawTextOption opt = default;

            double fw = FontTexW * fontScale;
            double fh = FontTexH * fontScale;
            double fw2 = fw/2;
            double fh2 = fh/2;

            Vector3d p;

            p = Vector3d.UnitX * size;
            p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
            p.X = p.X - fw2;
            p.Y = p.Y + fh2 - 2;
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_X),
                p, Vector3d.UnitX, -Vector3d.UnitY, "X", fontScale, opt);

            p = Vector3d.UnitY * size;
            p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
            p.X = p.X - fw2;
            p.Y = p.Y + fh2 - 2;
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Y),
                p, Vector3d.UnitX, -Vector3d.UnitY, "Y", fontScale, opt);

            p = Vector3d.UnitZ * size;
            p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
            p.X = p.X - fw2;
            p.Y = p.Y + fh2 - 2;
            DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Z),
                p, Vector3d.UnitX, -Vector3d.UnitY, "Z", fontScale, opt);

            PopMatrixes();
        }

        private Vector3d WorldPointToDevPoint(Vector3d pt, double vw, double vh, Matrix4d modelV, Matrix4d projV)
        {
            Vector4d wv = pt.ToVector4d(1.0);

            Vector4d sv = Vector4d.Transform(wv, modelV);
            Vector4d pv = Vector4d.Transform(sv, projV);

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            double vw2 = vw / 2;
            double vh2 = vh / 2;

            dv.X *= vw2;
            dv.Y *= -vh2;

            dv.Z = 0;

            dv.X += vw2;
            dv.Y += vh2;

            return dv.ToVector3d();
        }

        private void DrawCompassOrtho()
        {
            PushMatrixes();

            double size = 40;

            double vw = DC.ViewWidth;
            double vh = DC.ViewHeight;

            double cx = size / 2 + 24;
            double cy = size / 2 + 40;

            double left = -cx;
            double right = vw - cx;
            double top = cy;
            double bottom = -(vh - cy);

            double arrowLen = 10;
            double arrowW2 = 5;

            Matrix4d prjm = Matrix4d.CreateOrthographicOffCenter(left, right, bottom, top, 100, 10000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref prjm);

            GL.MatrixMode(MatrixMode.Modelview);
            Vector3d lookAt = Vector3d.Zero;
            Vector3d eye = -DC.ViewDir * 300;

            Matrix4d mdlm = Matrix4d.LookAt(eye, lookAt, DC.UpVector);

            GL.LoadMatrix(ref mdlm);

            Vector3d p0;
            Vector3d p1;

            GL.LineWidth(2);

            p0 = Vector3d.UnitX * -size;
            p1 = Vector3d.UnitX * size;
            DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            p0 = Vector3d.UnitY * -size;
            p1 = Vector3d.UnitY * size;
            DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            p0 = Vector3d.UnitZ * -size;
            p1 = Vector3d.UnitZ * size;
            DrawArrow(DC.GetPen(DrawTools.PEN_AXIS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            GL.LineWidth(1);

            FontTex tex;

            Vector3d xv = CadMath.Normal(DC.ViewDir, DC.UpVector);
            Vector3d yv = CadMath.Normal(DC.ViewDir, DC.UpVector);
            double fs = 0.6;

            tex = mFontFaceW.CreateTexture("X");
            p1 = Vector3d.UnitX * size;
            GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_X).Color4());
            mFontRenderer.Render(tex, p1, xv * tex.ImgW * fs, DC.UpVector * tex.ImgH * fs);

            tex = mFontFaceW.CreateTexture("Y");
            p1 = Vector3d.UnitY * size;
            GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Y).Color4());
            mFontRenderer.Render(tex, p1, xv * tex.ImgW * fs, DC.UpVector * tex.ImgH * fs);

            tex = mFontFaceW.CreateTexture("Z");
            p1 = Vector3d.UnitZ * size;
            GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Z).Color4());
            mFontRenderer.Render(tex, p1, xv * tex.ImgW * fs, DC.UpVector * tex.ImgH * fs);

            PopMatrixes();
        }

        private void PushMatrixes()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
        }

        private void PopMatrixes()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        private void Start2D()
        {
            PushMatrixes();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MultMatrix(ref DC.Matrix2D);
        }

        private void End2D()
        {
            PopMatrixes();
        }

        public void DrawSelectedPoint(Vector3d pt, DrawPen pen)
        {
            Vector3d p = DC.WorldPointToDevPoint(pt);
            Start2D();
            GL.Color4(pen.Color4());
            GL.PointSize(4);

            GL.Begin(PrimitiveType.Points);

            GL.Vertex3(p);

            GL.End();
            End2D();
        }

        public void DrawSelectedPoints(VertexList pointList, DrawPen pen)
        {
            Start2D();
            GL.Color4(pen.Color4());
            GL.PointSize(4);

            GL.Begin(PrimitiveType.Points);

            foreach (CadVertex p in pointList)
            {
                if (p.Selected)
                {
                    GL.Vertex3(DC.WorldPointToDevPoint(p.vector));
                }
            }

            GL.End();
            End2D();
        }

        private void DrawRect2D(Vector3d p0, Vector3d p1, DrawPen pen)
        {
            Vector3d v0 = Vector3d.Zero;
            Vector3d v1 = Vector3d.Zero;
            Vector3d v2 = Vector3d.Zero;
            Vector3d v3 = Vector3d.Zero;

            v0.X = System.Math.Max(p0.X, p1.X);
            v0.Y = System.Math.Min(p0.Y, p1.Y);

            v1.X = v0.X;
            v1.Y = System.Math.Max(p0.Y, p1.Y);

            v2.X = System.Math.Min(p0.X, p1.X);
            v2.Y = v1.Y;

            v3.X = v2.X;
            v3.Y = v0.Y;

            Start2D();

            GL.Begin(PrimitiveType.LineStrip);

            GL.Color4(pen.Color4());
            GL.Vertex3(v0);
            GL.Vertex3(v1);
            GL.Vertex3(v2);
            GL.Vertex3(v3);
            GL.Vertex3(v0);

            GL.End();

            End2D();
        }

        public void DrawCross(DrawPen pen, Vector3d p, double size)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            double hs = size;

            Vector3d px0 = p;
            px0.X -= hs;
            Vector3d px1 = p;
            px1.X += hs;

            Vector3d py0 = p;
            py0.Y -= hs;
            Vector3d py1 = p;
            py1.Y += hs;

            Vector3d pz0 = p;
            pz0.Z -= hs;
            Vector3d pz1 = p;
            pz1.Z += hs;

            DrawLine(pen, px0, px1);
            DrawLine(pen, py0, py1);
            DrawLine(pen, pz0, pz1);
        }

        private Vector3d GetShiftForOutLine()
        {
            double shift = DC.DevSizeToWoldSize(0.9);
            Vector3d vv = -DC.ViewDir * shift;

            return vv;
        }

        public void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s)
        {
            a *= DC.WorldScale;

            FontTex tex = mFontFaceW.CreateTexture(s);

            Vector3d xv = xdir.UnitVector() * tex.ImgW * 0.15;
            Vector3d yv = ydir.UnitVector() * tex.ImgH * 0.15;

            if (xv.IsZero() || yv.IsZero())
            {
                return;
            }

            if ((opt.Option & DrawTextOption.H_CENTER)!=0)
            {
                a -= (xv / 2);
            }

            GL.Color4(brush.Color4());
            
            mFontRenderer.Render(tex, a, xv, yv);
        }

        private void DrawTextScrn(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, string s, double imgScale, DrawTextOption opt)
        {
            Start2D();

            FontTex tex = mFontFaceW.CreateTexture(s);

            Vector3d xv = xdir.UnitVector() * tex.ImgW * imgScale;
            Vector3d yv = ydir.UnitVector() * tex.ImgH * imgScale;

            if ((opt.Option & DrawTextOption.H_CENTER) != 0)
            {
                a -= (xv / 2);
            }

            if (xv.IsZero() || yv.IsZero())
            {
                return;
            }

            GL.Color4(brush.Color4());

            mFontRenderer.Render(tex, a, xv, yv);

            End2D();
        }


        public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            Vector3d p0 = pp.Pos - (pp.DirX * size);
            Vector3d p1 = pp.Pos + (pp.DirX * size);

            p0 = DC.DevPointToWorldPoint(p0);
            p1 = DC.DevPointToWorldPoint(p1);

            GL.Disable(EnableCap.DepthTest);

            DrawLine(pen, p0, p1);

            p0 = pp.Pos - (pp.DirY * size);
            p1 = pp.Pos + (pp.DirY * size);

            p0 = DC.DevPointToWorldPoint(p0);
            p1 = DC.DevPointToWorldPoint(p1);

            DrawLine(pen, p0, p1);

            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size)
        {
            GL.Disable(EnableCap.DepthTest);

            //Vector3d size = DC.DevVectorToWorldVector(Vector3d.UnitX * pix_size);
            //DrawCross(pen, p, size.Norm());

            double size = DC.DevSizeToWoldSize(pix_size);
            DrawCross(pen, p, size);

            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1)
        {
            GL.Disable(EnableCap.DepthTest);

            Vector3d pp0 = DC.WorldPointToDevPoint(p0);
            Vector3d pp2 = DC.WorldPointToDevPoint(p1);

            Vector3d pp1 = pp0;
            pp1.Y = pp2.Y;

            Vector3d pp3 = pp0;
            pp3.X = pp2.X;

            pp0 = DC.DevPointToWorldPoint(pp0);
            pp1 = DC.DevPointToWorldPoint(pp1);
            pp2 = DC.DevPointToWorldPoint(pp2);
            pp3 = DC.DevPointToWorldPoint(pp3);

            DrawLine(pen, pp0, pp1);
            DrawLine(pen, pp1, pp2);
            DrawLine(pen, pp2, pp3);
            DrawLine(pen, pp3, pp0);

            GL.Enable(EnableCap.DepthTest);
        }

        // Snap時にハイライトされるポイントを描画する
        public void DrawHighlightPoint(Vector3d pt, DrawPen pen)
        {
            GL.LineWidth(DrawingConst.HighlightPointLineWidth);

            Start2D();

            GL.Color4(pen.Color4());
            //DrawCross2D(DC.WorldPointToDevPoint(pt), DrawingConst.HighlightPointLineLength);
            DrawX2D(DC.WorldPointToDevPoint(pt), DrawingConst.HighlightPointLineLength);

            End2D();

            GL.LineWidth(1);
        }

        // Snap時にハイライトされるポイントを描画する
        public void DrawHighlightPoints(List<HighlightPointListItem> list)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            Start2D();

            GL.LineWidth(DrawingConst.HighlightPointLineWidth);

            list.ForEach(item =>
            {
                GL.Color4(item.Pen.Color4());
                //DrawCross2D(DC.WorldPointToDevPoint(item.Point), DrawingConst.HighlightPointLineLength);
                DrawX2D(DC.WorldPointToDevPoint(item.Point), DrawingConst.HighlightPointLineLength);
            });

            GL.LineWidth(1);

            End2D();
        }

        // Point sizeをそのまま使って十字を描画
        private void DrawCross2D(Vector3d p, double size)
        {
            double hs = size;

            Vector3d px0 = p;
            px0.X -= hs;
            Vector3d px1 = p;
            px1.X += hs;

            Vector3d py0 = p;
            py0.Y -= hs;
            Vector3d py1 = p;
            py1.Y += hs;

            //Vector3d pz0 = p;
            //pz0.Z -= hs;
            //Vector3d pz1 = p;
            //pz1.Z += hs;

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(px0);
            GL.Vertex3(px1);
            GL.End();

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(py0);
            GL.Vertex3(py1);
            GL.End();

            //GL.Begin(PrimitiveType.LineStrip);
            //GL.Vertex3(pz0);
            //GL.Vertex3(pz1);
            //GL.End();
        }

        // Point sizeをそのまま使って十字を描画
        private void DrawX2D(Vector3d p, double size)
        {
            double hs = size;

            Vector3d pa = p;
            pa.X -= hs;
            pa.Y += hs;

            Vector3d pa_ = p;
            pa_.X += hs;
            pa_.Y -= hs;

            Vector3d pb = p;
            pb.X += hs;
            pb.Y += hs;

            Vector3d pb_ = p;
            pb_.X -= hs;
            pb_.Y -= hs;

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(pa);
            GL.Vertex3(pa_);
            GL.End();

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex3(pb);
            GL.Vertex3(pb_);
            GL.End();
        }

        public void DrawDot(DrawPen pen, Vector3d p)
        {
            GL.Color4(pen.Color4());

            GL.Begin(PrimitiveType.Points);

            GL.Vertex3(p);

            GL.End();
        }

        public void DrawGrid(Gridding grid)
        {
            GL.PointSize(1);

            if (DC is DrawContextGLOrtho)
            {
                DrawGridOrtho(grid);
            }
            else if (DC is DrawContextGL)
            {
                DrawGridPerse(grid);
            }
        }

        protected void DrawGridOrtho(Gridding grid)
        {
            Vector3d lt = Vector3d.Zero;
            Vector3d rb = new Vector3d(DC.ViewWidth, DC.ViewHeight, 0);

            Vector3d ltw = DC.DevPointToWorldPoint(lt);
            Vector3d rbw = DC.DevPointToWorldPoint(rb);

            double minx = Math.Min(ltw.X, rbw.X);
            double maxx = Math.Max(ltw.X, rbw.X);

            double miny = Math.Min(ltw.Y, rbw.Y);
            double maxy = Math.Max(ltw.Y, rbw.Y);

            double minz = Math.Min(ltw.Z, rbw.Z);
            double maxz = Math.Max(ltw.Z, rbw.Z);

            DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

            Vector3d p = default;

            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.X * n;
            double szy = grid.GridSize.Y * n;
            double szz = grid.GridSize.Z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            x = sx;
            while (x < maxx)
            {
                p.X = x;
                p.Z = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                x += szx;
            }

            z = sz;
            y = sy;

            while (z < maxz)
            {
                p.Z = z;
                p.X = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                z += szz;
            }

            z = sz;
            x = sx;

            while (x < maxx)
            {
                p.X = x;
                p.Y = 0;

                z = sz;

                while (z < maxz)
                {
                    p.Z = z;
                    DrawDot(pen, p);
                    z += szz;
                }

                x += szx;
            }
        }

        protected void DrawGridPerse(Gridding grid)
        {
            //double minx = -100;
            //double maxx = 100;

            //double miny = -100;
            //double maxy = 100;

            //double minz = -100;
            //double maxz = 100;

            //DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

            //Vector3d p = default;

            //double x, y, z;
            //double sx, sy, sz;
            //double szx = grid.GridSize.X;
            //double szy = grid.GridSize.Y;
            //double szz = grid.GridSize.Z;

            //sx = Math.Round(minx / szx) * szx;
            //sy = Math.Round(miny / szy) * szy;
            //sz = Math.Round(minz / szz) * szz;

            //x = sx;
            //while (x < maxx)
            //{
            //    p.X = x;
            //    p.Z = 0;

            //    y = sy;

            //    while (y < maxy)
            //    {
            //        p.Y = y;
            //        DrawDot(pen, p);
            //        y += szy;
            //    }

            //    x += szx;
            //}

            //z = sz;
            //y = sy;

            //while (z < maxz)
            //{
            //    p.Z = z;
            //    p.X = 0;

            //    y = sy;

            //    while (y < maxy)
            //    {
            //        p.Y = y;
            //        DrawDot(pen, p);
            //        y += szy;
            //    }

            //    z += szz;
            //}

            //z = sz;
            //x = sx;

            //while (x < maxx)
            //{
            //    p.X = x;
            //    p.Y = 0;

            //    z = sz;

            //    while (z < maxz)
            //    {
            //        p.Z = z;
            //        DrawDot(pen, p);
            //        z += szz;
            //    }

            //    x += szx;
            //}
        }

        public void DrawRectScrn(DrawPen pen, Vector3d pp0, Vector3d pp1)
        {
            Vector3d p0 = DC.DevPointToWorldPoint(pp0);
            Vector3d p1 = DC.DevPointToWorldPoint(pp1);

            DrawRect(pen, p0, p1);
        }

        public void DrawPageFrame(double w, double h, Vector3d center)
        {
            if (!(DC is DrawContextGLOrtho))
            {
                return;
            }

            Vector3d pt = default;

            // p0
            pt.X = -w / 2 + center.X;
            pt.Y = h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p0 = default;
            p0.X = pt.X * DC.UnitPerMilli;
            p0.Y = pt.Y * DC.UnitPerMilli;

            p0 += DC.ViewOrg;

            // p1
            pt.X = w / 2 + center.X;
            pt.Y = -h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p1 = default;
            p1.X = pt.X * DC.UnitPerMilli;
            p1.Y = pt.Y * DC.UnitPerMilli;

            p1 += DC.ViewOrg;

            GL.Enable(EnableCap.LineStipple);
            GL.LineStipple(1, 0b1100110011001100);

            DrawRectScrn(DC.GetPen(DrawTools.PEN_PAGE_FRAME), p0, p1);

            GL.Disable(EnableCap.LineStipple);
        }

        public void EnableLight()
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
        }

        public void DisableLight()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
        }

        public void DrawBouncingBox(DrawPen pen, MinMax3D mm)
        {
            Vector3d p0 = new Vector3d(mm.Min.X, mm.Min.Y, mm.Min.Z);
            Vector3d p1 = new Vector3d(mm.Min.X, mm.Min.Y, mm.Max.Z);
            Vector3d p2 = new Vector3d(mm.Max.X, mm.Min.Y, mm.Max.Z);
            Vector3d p3 = new Vector3d(mm.Max.X, mm.Min.Y, mm.Min.Z);

            Vector3d p4 = new Vector3d(mm.Min.X, mm.Max.Y, mm.Min.Z);
            Vector3d p5 = new Vector3d(mm.Min.X, mm.Max.Y, mm.Max.Z);
            Vector3d p6 = new Vector3d(mm.Max.X, mm.Max.Y, mm.Max.Z);
            Vector3d p7 = new Vector3d(mm.Max.X, mm.Max.Y, mm.Min.Z);

            DC.Drawing.DrawLine(pen, p0, p1);
            DC.Drawing.DrawLine(pen, p1, p2);
            DC.Drawing.DrawLine(pen, p2, p3);
            DC.Drawing.DrawLine(pen, p3, p0);

            DC.Drawing.DrawLine(pen, p4, p5);
            DC.Drawing.DrawLine(pen, p5, p6);
            DC.Drawing.DrawLine(pen, p6, p7);
            DC.Drawing.DrawLine(pen, p7, p4);

            DC.Drawing.DrawLine(pen, p0, p4);
            DC.Drawing.DrawLine(pen, p1, p5);
            DC.Drawing.DrawLine(pen, p2, p6);
            DC.Drawing.DrawLine(pen, p3, p7);
        }

        public void DrawCrossScrn(DrawPen pen, Vector3d p, double size)
        {
            Start2D();
            DrawCross(pen, p, size);
            End2D();
        }

        public void DrawArrow(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawUtil.DrawArrow(DrawLine, pen, pt0, pt1, type, pos, len/ DC.WorldScale, width / DC.WorldScale);
        }

        private void DrawArrowRaw(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawUtil.DrawArrow(DrawLineRaw, pen, pt0, pt1, type, pos, len, width);
        }

        public void DrawExtSnapPoints(Vector3dList pointList, DrawPen pen)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            Start2D();

            GL.LineWidth(DrawingConst.HighlightPointLineWidth);
            GL.Color4(pen.Color4());

            pointList.ForEach(v =>
            {
                DrawCross2D(DC.WorldPointToDevPoint(v), DrawingConst.HighlightPointLineLength);
            });

            GL.LineWidth(1);

            End2D();
        }
    }
}
