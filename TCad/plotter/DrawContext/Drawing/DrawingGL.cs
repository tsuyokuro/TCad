using CadDataTypes;
using GLFont;
using MyCollections;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Plotter.Settings;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TCad.Plotter.Model.HalfEdgeModel;

namespace Plotter;

public class DrawingGL : IDrawing
{
    private DrawContextGL DC;

    private FontFaceW mFontFaceW;
    private FontRenderer mFontRenderer;

    private vcompo_t FontTexW;
    private vcompo_t FontTexH;

    public DrawingGL(DrawContextGL dc)
    {
        DC = dc;

        /*
        mFontFaceW = new FontFaceW();
        //mFontFaceW.SetFont(@"C:\Windows\Fonts\msgothic.ttc", 0);
        mFontFaceW.SetResourceFont("/Fonts/mplus-1m-regular.ttf");
        mFontFaceW.SetSize(24);
        */

        mFontFaceW = GLUtilContainer.FontFaceProvider.Instance.FromResource("/Fonts/mplus-1m-regular.ttf", 24, 0);

        mFontRenderer = GLUtilContainer.FontRenderer.Instance;

        FontTex tex = mFontFaceW.CreateTexture('X', true);
        FontTexW = tex.ImgW;
        FontTexH = tex.ImgH;
    }

    public void Dispose()
    {
    }

    public void Clear(DrawBrush brush)
    {
        GL.ClearColor(brush.Color4);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void DrawLine(DrawPen pen, vector3_t a, vector3_t b)
    {
        GL.Color4(pen.Color4);
        GL.LineWidth(pen.Width);

        GL.Begin(PrimitiveType.LineStrip);

        GL.Vertex3(a);
        GL.Vertex3(b);

        GL.End();
    }

    public void DrawHarfEdgeModel(
        DrawBrush brush, DrawPen pen, DrawPen edgePen, vcompo_t edgeThreshold, HeModel model)
    {
        //DrawHeFaces(brush, model);
        DrawHeFacesVBO(brush, model);

        //DrawHeEdges(pen, edgePen, edgeThreshold, model);
        DrawHeEdgesVBO(pen, edgePen, edgeThreshold, model);

        if (SettingsHolder.Settings.DrawNormal)
        {
            DrawHeFacesNormal(model);
        }
    }

    private void DrawHeFaces(DrawBrush brush, HeModel model)
    {
        if (brush.IsInvalid)
        {
            return;
        }

        EnableLight();
        GL.Enable(EnableCap.ColorMaterial);
        GL.Color4(brush.Color4);

        GL.Begin(PrimitiveType.Triangles);
        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore.Data[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            if (f.Normal != HeModel.INVALID_INDEX)
            {
                vector3_t nv = model.NormalStore.Data[f.Normal];
                GL.Normal3(nv);
            }

            for (; ; )
            {
                GL.Vertex3((model.VertexStore.Data[c.Vertex].vector));

                c = c.Next;

                if (c == head)
                {
                    break;
                }
            }
        }
        GL.End();
    }

    public struct VboVertex
    {
        public Vector3 Pos;
        public Vector3 Normal;
    }

    FlexArray<int> indexes = new(1000);
    FlexArray<VboVertex> vboVertices = new();

    FlexArray<vector3_t> vboPoints = new();
    FlexArray<int> ptIndexes1 = new(1000);
    FlexArray<int> ptIndexes2 = new(1000);


    private void DrawHeFacesVBO(DrawBrush brush, HeModel model)
    {
        if (brush.IsInvalid)
        {
            return;
        }


        vboVertices.Clear();
        indexes.Clear();
        int vIndex = 0;
        VboVertex v = new();

        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore.Data[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            for (; ; )
            {
                v.Pos = (vector3_t)model.VertexStore.Data[c.Vertex].vector;
                v.Normal = (vector3_t)model.NormalStore[c.Normal];
                vboVertices.Add(v);
                indexes.Add(vIndex);
                vIndex++;

                c = c.Next;

                if (c == head)
                {
                    break;
                }
            }
        }


        EnableLight();
        GL.Enable(EnableCap.ColorMaterial);
        GL.Color4(brush.Color4);


        int stride = Marshal.SizeOf(typeof(VboVertex));


        int vCnt = vboVertices.Count;
        int vertexBufferId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
        unsafe
        {
            fixed (VboVertex* ptr = &vboVertices.Data[0])
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vCnt * stride, (nint)ptr, BufferUsageHint.StaticDraw);
            }

            GL.VertexPointer(3, VertexPointerType.Float, stride, 0);
            GL.NormalPointer(NormalPointerType.Float, stride, sizeof(vector3_t));
        }


        int idxCnt = indexes.Count;
        int idxBufferId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxBufferId);
        unsafe
        {
            fixed (int* ptr = &indexes.Data[0])
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, idxCnt * sizeof(int), (nint)ptr, BufferUsageHint.StaticDraw);
            }
        }


        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.NormalArray);


        GL.DrawElements(BeginMode.Triangles, indexes.Count, DrawElementsType.UnsignedInt, 0);


        // 後処理
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.DisableClientState(ArrayCap.VertexArray);
        GL.DisableClientState(ArrayCap.NormalArray);

        GL.DeleteBuffer(idxBufferId);
        GL.DeleteBuffer(vertexBufferId);
    }


    private void DrawHeFacesNormal(HeModel model)
    {
        DisableLight();

        vcompo_t len = DC.DevSizeToWoldSize(DrawSizes.NormalLen);
        vcompo_t arrowLen = DC.DevSizeToWoldSize(DrawSizes.NormalArrowLen);
        vcompo_t arrowW = DC.DevSizeToWoldSize(DrawSizes.NormalArrowWidth);

        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;


            GL.Begin(PrimitiveType.Lines);

            for (; ; )
            {
                HalfEdge next = c.Next;

                vector3_t p = model.VertexStore[c.Vertex].vector;

                if (c.Normal != HeModel.INVALID_INDEX)
                {
                    vector3_t nv = model.NormalStore[c.Normal];
                    vector3_t np0 = p;
                    vector3_t np1 = p + (nv * len);

                    DrawArrowGL(DC.GetPen(DrawTools.PEN_NORMAL), np0, np1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW, true);
                }

                c = next;

                if (c == head)
                {
                    break;
                }
            }

            GL.End();
        }

        EnableLight();
    }

    private void DrawHeEdges(DrawPen borderPen, DrawPen edgePen, vcompo_t edgeThreshold, HeModel model)
    {
        if (edgePen.IsNull)
        {
            return;
        }

        bool drawBorder = !borderPen.IsInvalid;
        bool drawEdge = !edgePen.IsInvalid;

        if (!drawBorder && !drawEdge)
        {
            return;
        }

        DisableLight();

        GL.LineWidth(1.0f);

        Color4 color = borderPen.Color4;
        Color4 edgeColor = edgePen.Color4;

        vector3_t shift = GetShiftForOutLine();

        vector3_t p0;
        vector3_t p1;

        GL.Begin(PrimitiveType.Lines);

        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            HalfEdge pair;

            p0 = model.VertexStore[c.Vertex].vector + shift;

            for (; ; )
            {
                bool drawAsEdge = false;
                bool isEdge = false;

                pair = c.Pair;

                if (drawEdge)
                {
                    if (pair == null)
                    {
                        drawAsEdge = true;
                        isEdge = true;
                    }
                    else
                    {
                        if (edgeThreshold != 0)
                        {
                            vcompo_t s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                            if (Math.Abs(s) <= edgeThreshold)
                            {
                                drawAsEdge = true;
                            }
                        }
                    }
                }

                p1 = model.VertexStore[c.Next.Vertex].vector + shift;

                if (drawAsEdge)
                {
                    bool draw = true;
                    if (!isEdge)
                    {
                        if (pair != null)
                        {
                            if (pair.Face < c.Face)
                            {
                                // Already drawed by pair
                                draw = false;
                            }
                        }
                    }

                    if (draw)
                    {
                        GL.Color4(edgeColor);
                        GL.Vertex3(p0);
                        GL.Vertex3(p1);

                    }
                }
                else if (drawBorder)
                {
                    bool draw = true;
                    if (pair != null)
                    {
                        if (pair.Face < c.Face)
                        {
                            // Already drawed by pair
                            draw = false;
                        }
                    }

                    if (draw)
                    {
                        GL.Color4(color);
                        GL.Vertex3(p0);
                        GL.Vertex3(p1);
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

        GL.End();
    }


    private void DrawHeEdgesVBO(DrawPen borderPen, DrawPen edgePen, vcompo_t edgeThreshold, HeModel model)
    {
        if (edgePen.IsNull)
        {
            return;
        }

        bool drawBorder = !borderPen.IsInvalid;
        bool drawEdge = !edgePen.IsInvalid;

        if (!drawBorder && !drawEdge)
        {
            return;
        }

        vboPoints.Clear();  // 頂点バッファ
        ptIndexes1.Clear(); // エッジ線用index buffer
        ptIndexes2.Clear(); // ポリゴン境界線用index buffer

        int vIdx0;
        int vIdx1;

        // エッジ線と境界線のIndex bufferを作成
        for (int i = 0; i < model.FaceStore.Count; i++)
        {
            HeFace f = model.FaceStore[i];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            HalfEdge pair;


            vIdx0 = c.Vertex;

            for (; ; )
            {
                bool drawAsEdge = false;
                bool isEdge = false;

                pair = c.Pair;

                if (drawEdge)
                {
                    if (pair == null)
                    {
                        drawAsEdge = true;
                        isEdge = true;
                    }
                    else
                    {
                        if (edgeThreshold != 0)
                        {
                            vcompo_t s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                            if (Math.Abs(s) <= edgeThreshold)
                            {
                                drawAsEdge = true;
                            }
                        }
                    }
                }

                vIdx1 = c.Next.Vertex;

                if (drawAsEdge)
                {
                    bool draw = true;
                    if (!isEdge)
                    {
                        if (pair != null)
                        {
                            if (pair.Face < c.Face)
                            {
                                // Already drawed by pair
                                draw = false;
                            }
                        }
                    }

                    if (draw)
                    {
                        // エッジ線用Indexを追加
                        ptIndexes1.Add(vIdx0);
                        ptIndexes1.Add(vIdx1);
                    }
                }
                else if (drawBorder)
                {
                    bool draw = true;
                    if (pair != null)
                    {
                        if (pair.Face < c.Face)
                        {
                            // Already drawed by pair
                            draw = false;
                        }
                    }

                    if (draw)
                    {
                        // 境界線用Indexを追加
                        ptIndexes2.Add(vIdx0);
                        ptIndexes2.Add(vIdx1);
                    }
                }

                vIdx0 = vIdx1;

                c = c.Next;

                if (c == head)
                {
                    break;
                }
            }
        }

        // エッジ線、境界線が存在しない場合、終了
        if (ptIndexes1.Count < 2 && ptIndexes2.Count < 2)
        {
            return;
        }

        vector3_t shift = GetShiftForOutLine();

        // 面に埋もれないように少し手前にシフトした頂点Bufferを作成
        for (int i = 0; i < model.VertexStore.Count; i++)
        {
            vboPoints.Add(model.VertexStore[i].vector + shift);
        }


        Color4 color = borderPen.Color4;
        Color4 edgeColor = edgePen.Color4;


        int stride = Marshal.SizeOf(typeof(vector3_t));

        int vCnt = vboPoints.Count;

        int vertexBufferId = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
        unsafe
        {
            fixed (vector3_t* ptr = &vboPoints.Data[0])
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vCnt * stride, (nint)ptr, BufferUsageHint.StaticDraw);
            }
        }
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        int idxCnt1 = ptIndexes1.Count;
        int idxBufferId1 = 0;

        if (idxCnt1 > 1)
        {
            idxBufferId1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxBufferId1);
            unsafe
            {
                fixed (int* ptr = &ptIndexes1.Data[0])
                {
                    GL.BufferData(BufferTarget.ElementArrayBuffer, idxCnt1 * sizeof(int), (nint)ptr, BufferUsageHint.StaticDraw);
                }
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        int idxCnt2 = ptIndexes2.Count;
        int idxBufferId2 = 0;

        if (idxCnt2 > 1)
        {
            idxBufferId2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxBufferId2);
            unsafe
            {
                fixed (int* ptr = &ptIndexes2.Data[0])
                {
                    GL.BufferData(BufferTarget.ElementArrayBuffer, idxCnt2 * sizeof(int), (nint)ptr, BufferUsageHint.StaticDraw);
                }
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        GL.EnableClientState(ArrayCap.VertexArray);
        GL.DisableClientState(ArrayCap.NormalArray);


        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
        unsafe
        {
            GL.VertexPointer(3, VertexPointerType.Float, stride, 0);
        }


        DisableLight();
        GL.LineWidth(1.0f);

        if (idxCnt1 > 1)
        {
            GL.Color4(edgeColor);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxBufferId1);
            GL.DrawElements(BeginMode.Lines, idxCnt1, DrawElementsType.UnsignedInt, 0);
        }

        if (idxCnt2 > 1)
        {
            GL.Color4(color);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxBufferId2);
            GL.DrawElements(BeginMode.Lines, idxCnt2, DrawElementsType.UnsignedInt, 0);
        }


        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.DisableClientState(ArrayCap.VertexArray);

        GL.DeleteBuffer(idxBufferId1);
        GL.DeleteBuffer(idxBufferId2);
        GL.DeleteBuffer(vertexBufferId);
    }


    private const vcompo_t AXIS_MARGIN = 24;
    private vcompo_t AxisLength()
    {
        vcompo_t viewMax = (vcompo_t)Math.Min(DC.ViewWidth, DC.ViewHeight) / 2;
        vcompo_t len = DC.DevSizeToWoldSize(viewMax - AXIS_MARGIN);
        return len;
    }

    public void DrawAxis()
    {
        vector3_t p0;
        vector3_t p1;

        vcompo_t len = AxisLength();
        vcompo_t arrowLen = DC.DevSizeToWoldSize(16);
        vcompo_t arrowW2 = DC.DevSizeToWoldSize(8);

        GL.Begin(PrimitiveType.Lines);

        // X軸
        p0 = new vector3_t(-len, 0, 0);
        p1 = new vector3_t(len, 0, 0);

        if (!CadMath.IsParallel(p1 - p0, (vector3_t)DC.ViewDir))
        {
            DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);
        }

        // Y軸
        p0 = new vector3_t(0, -len, 0);
        p1 = new vector3_t(0, len, 0);

        if (!CadMath.IsParallel(p1 - p0, (vector3_t)DC.ViewDir))
        {
            DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);
        }

        // Z軸
        p0 = new vector3_t(0, 0, -len);
        p1 = new vector3_t(0, 0, len);

        if (!CadMath.IsParallel(p1 - p0, (vector3_t)DC.ViewDir))
        {
            DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);
        }

        GL.End();
    }

    public void DrawAxisLabel()
    {
        vector3_t p;

        vcompo_t len = AxisLength();

        vcompo_t fontScale = (vcompo_t)(0.6);
        vcompo_t fw = FontTexW * fontScale;
        vcompo_t fh = FontTexH * fontScale;

        DrawTextOption opt = default;

        vcompo_t labelOffset = DC.DevSizeToWoldSize(12);

        // X軸
        p = vector3_t.UnitX * len;
        p.X += labelOffset;
        p = DC.WorldPointToDevPoint(p);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2 - 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_X), p, vector3_t.UnitX, -vector3_t.UnitY, "X", fontScale, opt);

        // Y軸
        p = vector3_t.UnitY * len;
        p.Y += labelOffset;
        p = DC.WorldPointToDevPoint(p);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_Y), p, vector3_t.UnitX, -vector3_t.UnitY, "Y", fontScale, opt);

        // Z軸
        p = vector3_t.UnitZ * len;
        p.Z += labelOffset;
        p = DC.WorldPointToDevPoint(p);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2 - 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_AXIS_LABEL_Z), p, vector3_t.UnitX, -vector3_t.UnitY, "Z", fontScale, opt);
    }

    public void DrawCompass()
    {
        DrawCompassPers();
        //DrawCompassOrtho();
    }

    private void DrawCompassPers()
    {
        PushMatrixes();

        vcompo_t size = 80;

        vcompo_t vw = DC.ViewWidth;
        vcompo_t vh = DC.ViewHeight;

        vcompo_t cx = size / 2 + 8;
        vcompo_t cy = size / 2 + 20;

        vcompo_t left = -cx;
        vcompo_t right = vw - cx;
        vcompo_t top = cy;
        vcompo_t bottom = -(vh - cy);

        vcompo_t arrowLen = 20;
        vcompo_t arrowW2 = 10;

        matrix4_t prjm = matrix4_t.CreatePerspectiveOffCenter(left, right, bottom, top, 100, 10000);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref prjm);

        GL.MatrixMode(MatrixMode.Modelview);
        vector3_t lookAt = vector3_t.Zero;
        vector3_t eye = -DC.ViewDir * 220;

        matrix4_t mdlm = matrix4_t.LookAt(eye, lookAt, DC.UpVector);

        GL.LoadMatrix(ref mdlm);

        vector3_t p0;
        vector3_t p1;

        GL.LineWidth(1);
        GL.Disable(EnableCap.DepthTest);

        GL.Begin(PrimitiveType.Lines);

        p0 = vector3_t.UnitX * -size;
        p1 = vector3_t.UnitX * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_COMPASS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        p0 = vector3_t.UnitY * -size;
        p1 = vector3_t.UnitY * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_COMPASS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        p0 = vector3_t.UnitZ * -size;
        p1 = vector3_t.UnitZ * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_COMPASS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        GL.End();
        GL.LineWidth(1);

        vcompo_t fontScale = (vcompo_t)(0.6);
        DrawTextOption opt = default;

        vcompo_t fw = FontTexW * fontScale;
        vcompo_t fh = FontTexH * fontScale;

        vector3_t p;

        vcompo_t labelOffset = 20;

        p = vector3_t.UnitX * size;
        p.X += labelOffset;
        p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2 - 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_X),
            p, vector3_t.UnitX, -vector3_t.UnitY, "X", fontScale, opt);

        p = vector3_t.UnitY * size;
        p.Y += labelOffset;
        p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2 - 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Y),
            p, vector3_t.UnitX, -vector3_t.UnitY, "Y", fontScale, opt);

        p = vector3_t.UnitZ * size;
        p.Z += labelOffset;
        p = WorldPointToDevPoint(p, vw, vh, mdlm, prjm);
        p.X = p.X - fw / 2;
        p.Y = p.Y + fh / 2 - 2;
        DrawTextScrn(DrawTools.FONT_SMALL, DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Z),
            p, vector3_t.UnitX, -vector3_t.UnitY, "Z", fontScale, opt);

        PopMatrixes();
    }

    private vector3_t WorldPointToDevPoint(vector3_t pt, vcompo_t vw, vcompo_t vh, matrix4_t modelV, matrix4_t projV)
    {
        vector4_t wv = pt.ToVector4((vcompo_t)(1.0));

        vector4_t sv = vector4_t.TransformRow(wv, modelV);
        vector4_t pv = vector4_t.TransformRow(sv, projV);

        vector4_t dv;

        dv.X = pv.X / pv.W;
        dv.Y = pv.Y / pv.W;
        dv.Z = pv.Z / pv.W;
        dv.W = pv.W;

        vcompo_t vw2 = vw / 2;
        vcompo_t vh2 = vh / 2;

        dv.X *= vw2;
        dv.Y *= -vh2;

        dv.Z = 0;

        dv.X += vw2;
        dv.Y += vh2;

        return dv.ToVector3();
    }

    private void DrawCompassOrtho()
    {
        PushMatrixes();

        vcompo_t size = 40;

        vcompo_t vw = DC.ViewWidth;
        vcompo_t vh = DC.ViewHeight;

        vcompo_t cx = size / 2 + 24;
        vcompo_t cy = size / 2 + 40;

        vcompo_t left = -cx;
        vcompo_t right = vw - cx;
        vcompo_t top = cy;
        vcompo_t bottom = -(vh - cy);

        vcompo_t arrowLen = 10;
        vcompo_t arrowW2 = 5;

        matrix4_t prjm = matrix4_t.CreateOrthographicOffCenter(left, right, bottom, top, 100, 10000);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref prjm);

        GL.MatrixMode(MatrixMode.Modelview);
        vector3_t lookAt = vector3_t.Zero;
        vector3_t eye = -DC.ViewDir * 300;

        matrix4_t mdlm = matrix4_t.LookAt(eye, lookAt, DC.UpVector);

        GL.LoadMatrix(ref mdlm);

        vector3_t p0;
        vector3_t p1;

        GL.LineWidth(2);
        GL.Begin(PrimitiveType.Lines);

        p0 = vector3_t.UnitX * -size;
        p1 = vector3_t.UnitX * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_X), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        p0 = vector3_t.UnitY * -size;
        p1 = vector3_t.UnitY * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_Y), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        p0 = vector3_t.UnitZ * -size;
        p1 = vector3_t.UnitZ * size;
        DrawArrowGL(DC.GetPen(DrawTools.PEN_AXIS_Z), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2, true);

        GL.LineWidth(1);
        GL.End();

        FontTex tex;

        vector3_t xv = CadMath.Normal(DC.ViewDir, DC.UpVector);
        vector3_t yv = CadMath.Normal(DC.ViewDir, DC.UpVector);
        vcompo_t fs = (vcompo_t)(0.6);

        tex = mFontFaceW.CreateTexture("X");
        p1 = vector3_t.UnitX * size;
        GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_X).Color4);
        mFontRenderer.Render(tex, p1, xv * tex.ImgW * fs, DC.UpVector * tex.ImgH * fs);

        tex = mFontFaceW.CreateTexture("Y");
        p1 = vector3_t.UnitY * size;
        GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Y).Color4);
        mFontRenderer.Render(tex, p1, xv * tex.ImgW * fs, DC.UpVector * tex.ImgH * fs);

        tex = mFontFaceW.CreateTexture("Z");
        p1 = vector3_t.UnitZ * size;
        GL.Color4(DC.GetBrush(DrawTools.BRUSH_COMPASS_LABEL_Z).Color4);
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


    public void DrawSelectedPoint(vector3_t pt, DrawPen pen)
    {
        vector3_t p = DC.WorldPointToDevPoint(pt);
        Start2D();
        GL.Color4(pen.Color4);
        GL.PointSize(DrawSizes.SelectedPointSize);

        GL.Begin(PrimitiveType.Points);

        GL.Vertex3(p);

        GL.End();
        End2D();
    }
    public void DrawLastSelectedPoint(vector3_t pt, DrawPen pen)
    {
        vector3_t p = DC.WorldPointToDevPoint(pt);
        GL.Color4(pen.Color4);

        Start2D();
        GL.Color4(pen.Color4);

        DrawX2D(p, 3);

        GL.End();
        End2D();
    }

    public void DrawSelectedPoints_(VertexList pointList, DrawPen pen)
    {
        //Start2D();
        GL.Color4(pen.Color4);
        GL.PointSize(3);
        GL.Disable(EnableCap.DepthTest);
        GL.Begin(PrimitiveType.Points);

        unsafe
        {
            int num = pointList.Data.Length;
            fixed (CadVertex* ptr = &pointList.Data[0])
            {
                CadVertex* p = ptr;
                for (int i = 0; i < num; i++)
                {
                    if (p->Selected)
                    {
                        //GL.Vertex3(DC.WorldPointToDevPoint(p->vector));
                        GL.Vertex3(p->vector);
                    }

                    p++;
                }
            }
        }

        //for (int i = 0; i < pointList.Data.Length; i++)
        //{
        //    if (pointList[i].Selected)
        //    {
        //        GL.Vertex3(DC.WorldPointToDevPoint(pointList[i].vector));
        //    }
        //}

        GL.End();
        //End2D();
        GL.Enable(EnableCap.DepthTest);
    }


    public void DrawSelectedPoints(VertexList pointList, DrawPen pen)
    {
        vboPoints.Clear();
        ptIndexes1.Clear();

        unsafe
        {
            int num = pointList.Data.Length;
            fixed (CadVertex* ptr = &pointList.Data[0])
            {
                CadVertex* p = ptr;
                UInt64 ep = ((UInt64)p) + ((UInt64)sizeof(CadVertex) * (UInt64)num);

                for (; (UInt64)p < ep;)
                {
                    if (p->Selected)
                    {
                        vboPoints.Add(p->vector);
                    }

                    p++;
                }
            }
        }

        if (vboPoints.Count == 0)
        {
            return;
        }


        int stride = Marshal.SizeOf(typeof(vector3_t));

        int vCnt = vboPoints.Count;

        int pointBufferId = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, pointBufferId);
        unsafe
        {
            fixed (vector3_t* ptr = &vboPoints.Data[0])
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vCnt * stride, (nint)ptr, BufferUsageHint.StaticDraw);
            }
        }
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        GL.BindBuffer(BufferTarget.ArrayBuffer, pointBufferId);
        unsafe
        {
            GL.VertexPointer(3, VertexPointerType.Float, stride, 0);
        }


        GL.Disable(EnableCap.DepthTest);
        GL.Color4(pen.Color4);
        GL.PointSize(DrawSizes.SelectedPointSize);

        GL.EnableClientState(ArrayCap.VertexArray);


        GL.DrawArrays(PrimitiveType.Points, 0, vCnt);


        GL.DisableClientState(ArrayCap.VertexArray);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        GL.Enable(EnableCap.DepthTest);
    }



    private void DrawRect2D(vector3_t p0, vector3_t p1, DrawPen pen)
    {
        vector3_t v0 = vector3_t.Zero;
        vector3_t v1 = vector3_t.Zero;
        vector3_t v2 = vector3_t.Zero;
        vector3_t v3 = vector3_t.Zero;

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

        GL.Color4(pen.Color4);
        GL.Vertex3(v0);
        GL.Vertex3(v1);
        GL.Vertex3(v2);
        GL.Vertex3(v3);
        GL.Vertex3(v0);

        GL.End();

        End2D();
    }

    public void DrawCross(DrawPen pen, vector3_t p, vcompo_t size)
    {
        GL.Disable(EnableCap.Lighting);
        GL.Disable(EnableCap.Light0);

        vcompo_t hs = size;

        vector3_t px0 = p;
        px0.X -= hs;
        vector3_t px1 = p;
        px1.X += hs;

        vector3_t py0 = p;
        py0.Y -= hs;
        vector3_t py1 = p;
        py1.Y += hs;

        vector3_t pz0 = p;
        pz0.Z -= hs;
        vector3_t pz1 = p;
        pz1.Z += hs;

        //DrawLine(pen, px0, px1);
        //DrawLine(pen, py0, py1);
        //DrawLine(pen, pz0, pz1);

        GL.Color4(pen.Color4);
        GL.Begin(PrimitiveType.Lines);
        GL.Vertex3(px0);
        GL.Vertex3(px1);
        GL.Vertex3(py0);
        GL.Vertex3(py1);
        GL.Vertex3(pz0);
        GL.Vertex3(pz1);
        GL.End();
    }

    private vector3_t GetShiftForOutLine()
    {
        vcompo_t shift = DC.DevSizeToWoldSize((vcompo_t)(0.1));
        vector3_t vv = -DC.ViewDir * shift;

        return vv;
    }

    public void DrawText(int font, DrawBrush brush, vector3_t a, vector3_t xdir, vector3_t ydir, DrawTextOption opt, vcompo_t scale, string s)
    {
        FontTex tex = mFontFaceW.CreateTexture(s);

        vector3_t xv = xdir.UnitVector() * tex.ImgW * (vcompo_t)(0.15) * scale;
        vector3_t yv = ydir.UnitVector() * tex.ImgH * (vcompo_t)(0.15) * scale;

        if (xv.IsZero() || yv.IsZero())
        {
            return;
        }

        if ((opt.Option & DrawTextOption.H_CENTER) != 0)
        {
            a -= (xv / 2);
        }

        GL.Color4(brush.Color4);

        mFontRenderer.Render(tex, a, xv, yv);
    }

    private void DrawTextScrn(int font, DrawBrush brush, vector3_t a, vector3_t xdir, vector3_t ydir, string s, vcompo_t imgScale, DrawTextOption opt)
    {
        Start2D();

        FontTex tex = null;

        if (s.Length == 1)
        {
            tex = mFontFaceW.CreateTexture(s[0], true);
        }
        else
        {
            tex = mFontFaceW.CreateTexture(s);
        }

        vector3_t xv = xdir.UnitVector() * tex.ImgW * imgScale;
        vector3_t yv = ydir.UnitVector() * tex.ImgH * imgScale;

        if ((opt.Option & DrawTextOption.H_CENTER) != 0)
        {
            a -= (xv / 2);
        }

        if (xv.IsZero() || yv.IsZero())
        {
            return;
        }

        GL.Color4(brush.Color4);

        mFontRenderer.Render(tex, a, xv, yv);

        End2D();
    }


    public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
    {
        DrawCrossCursorScrn(pp, pen, -1, -1);
    }

    public void DrawCrossCursorScrn(CadCursor pp, DrawPen pen, vcompo_t xsize, vcompo_t ysize)
    {
        vcompo_t size = (vcompo_t)Math.Max(DC.ViewWidth, DC.ViewHeight);

        if (xsize == -1)
        {
            xsize = size;
        }

        if (ysize == -1)
        {
            ysize = size;
        }

        vector3_t p0 = pp.Pos - (pp.DirX * xsize);
        vector3_t p1 = pp.Pos + (pp.DirX * xsize);

        p0 = DC.DevPointToWorldPoint(p0);
        p1 = DC.DevPointToWorldPoint(p1);

        GL.Disable(EnableCap.DepthTest);

        GL.Begin(PrimitiveType.Lines);

        GL.Color4(pen.Color4);
        GL.Vertex3(p0);
        GL.Vertex3(p1);

        p0 = pp.Pos - (pp.DirY * ysize);
        p1 = pp.Pos + (pp.DirY * ysize);

        p0 = DC.DevPointToWorldPoint(p0);
        p1 = DC.DevPointToWorldPoint(p1);

        GL.Vertex3(p0);
        GL.Vertex3(p1);

        GL.End();

        GL.Enable(EnableCap.DepthTest);
    }



    public void DrawMarkCursor(DrawPen pen, vector3_t p, vcompo_t pix_size)
    {
        GL.Disable(EnableCap.DepthTest);

        //vector3_t size = DC.DevVectorToWorldVector(vector3_t.UnitX * pix_size);
        //DrawCross(pen, p, size.Norm());

        vcompo_t size = DC.DevSizeToWoldSize(pix_size);
        DrawCross(pen, p, size);

        GL.Enable(EnableCap.DepthTest);
    }

    public void DrawRect(DrawPen pen, vector3_t p0, vector3_t p1)
    {
        GL.Disable(EnableCap.DepthTest);

        vector3_t pp0 = DC.WorldPointToDevPoint(p0);
        vector3_t pp2 = DC.WorldPointToDevPoint(p1);

        vector3_t pp1 = pp0;
        pp1.Y = pp2.Y;

        vector3_t pp3 = pp0;
        pp3.X = pp2.X;

        pp0 = DC.DevPointToWorldPoint(pp0);
        pp1 = DC.DevPointToWorldPoint(pp1);
        pp2 = DC.DevPointToWorldPoint(pp2);
        pp3 = DC.DevPointToWorldPoint(pp3);

        //DrawLine(pen, pp0, pp1);
        //DrawLine(pen, pp1, pp2);
        //DrawLine(pen, pp2, pp3);
        //DrawLine(pen, pp3, pp0);

        GL.Color4(pen.Color4);
        GL.Begin(PrimitiveType.Lines);
        GL.Vertex3(pp0); GL.Vertex3(pp1);
        GL.Vertex3(pp1); GL.Vertex3(pp2);
        GL.Vertex3(pp2); GL.Vertex3(pp3);
        GL.Vertex3(pp3); GL.Vertex3(pp0);
        GL.End();
        GL.Enable(EnableCap.DepthTest);
    }

    // Snap時にハイライトされるポイントを描画する
    public void DrawHighlightPoint(vector3_t pt, DrawPen pen)
    {
        GL.LineWidth(DrawSizes.HighlightPointLineWidth);

        Start2D();

        GL.Color4(pen.Color4);
        //DrawCross2D(DC.WorldPointToDevPoint(pt), DrawingConst.HighlightPointLineLength);
        DrawX2D(DC.WorldPointToDevPoint(pt), DrawSizes.HighlightPointLineLength);

        End2D();

        GL.LineWidth(1);
    }

    // Snap時にハイライトされるポイントを描画する
    public void DrawHighlightPoints(List<HighlightPointListItem> list)
    {
        GL.Disable(EnableCap.Lighting);
        GL.Disable(EnableCap.Light0);

        Start2D();

        GL.LineWidth(DrawSizes.HighlightPointLineWidth);

        list.ForEach(item =>
        {
            GL.Color4(item.Pen.Color4);
            //DrawCross2D(DC.WorldPointToDevPoint(item.Point), DrawingConst.HighlightPointLineLength);
            DrawX2D(DC.WorldPointToDevPoint(item.Point), DrawSizes.HighlightPointLineLength);
        });

        GL.LineWidth(1);

        End2D();
    }

    // Point sizeをそのまま使って十字を描画
    private void DrawCross2D(vector3_t p, vcompo_t size)
    {
        vcompo_t hs = size;

        vector3_t px0 = p;
        px0.X -= hs;
        vector3_t px1 = p;
        px1.X += hs;

        vector3_t py0 = p;
        py0.Y -= hs;
        vector3_t py1 = p;
        py1.Y += hs;

        //vector3_t pz0 = p;
        //pz0.Z -= hs;
        //vector3_t pz1 = p;
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

    // Point sizeをそのまま使ってXを描画
    private void DrawX2D(vector3_t p, vcompo_t size)
    {
        vcompo_t hs = size;

        vector3_t pa = p;
        pa.X -= hs;
        pa.Y += hs;

        vector3_t pa_ = p;
        pa_.X += hs;
        pa_.Y -= hs;

        vector3_t pb = p;
        pb.X += hs;
        pb.Y += hs;

        vector3_t pb_ = p;
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

    public void DrawDot(DrawPen pen, vector3_t p)
    {
        GL.Color4(pen.Color4);

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
        vector3_t lt = vector3_t.Zero;
        vector3_t rb = new vector3_t(DC.ViewWidth, DC.ViewHeight, 0);

        vector3_t ltw = DC.DevPointToWorldPoint(lt);
        vector3_t rbw = DC.DevPointToWorldPoint(rb);

        vcompo_t minx = (vcompo_t)Math.Min(ltw.X, rbw.X);
        vcompo_t maxx = (vcompo_t)Math.Max(ltw.X, rbw.X);

        vcompo_t miny = (vcompo_t)Math.Min(ltw.Y, rbw.Y);
        vcompo_t maxy = (vcompo_t)Math.Max(ltw.Y, rbw.Y);

        vcompo_t minz = (vcompo_t)Math.Min(ltw.Z, rbw.Z);
        vcompo_t maxz = (vcompo_t)Math.Max(ltw.Z, rbw.Z);

        DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

        vector3_t p = default;

        vcompo_t n = grid.Decimate(DC, grid, 8);

        vcompo_t x, y, z;
        vcompo_t sx, sy, sz;
        vcompo_t szx = grid.GridSize.X * n;
        vcompo_t szy = grid.GridSize.Y * n;
        vcompo_t szz = grid.GridSize.Z * n;

        sx = (vcompo_t)Math.Round(minx / szx) * szx;
        sy = (vcompo_t)Math.Round(miny / szy) * szy;
        sz = (vcompo_t)Math.Round(minz / szz) * szz;

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
        //vcompo_t minx = -100;
        //vcompo_t maxx = 100;

        //vcompo_t miny = -100;
        //vcompo_t maxy = 100;

        //vcompo_t minz = -100;
        //vcompo_t maxz = 100;

        //DrawPen pen = DC.GetPen(DrawTools.PEN_GRID);

        //vector3_t p = default;

        //vcompo_t x, y, z;
        //vcompo_t sx, sy, sz;
        //vcompo_t szx = grid.GridSize.X;
        //vcompo_t szy = grid.GridSize.Y;
        //vcompo_t szz = grid.GridSize.Z;

        //sx = (vcompo_t)Math.Round(minx / szx) * szx;
        //sy = (vcompo_t)Math.Round(miny / szy) * szy;
        //sz = (vcompo_t)Math.Round(minz / szz) * szz;

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

    public void DrawRectScrn(DrawPen pen, vector3_t pp0, vector3_t pp1)
    {
        vector3_t p0 = DC.DevPointToWorldPoint(pp0);
        vector3_t p1 = DC.DevPointToWorldPoint(pp1);

        DrawRect(pen, p0, p1);
    }

    public void DrawPageFrame(vcompo_t w, vcompo_t h, vector3_t center)
    {
        if (!(DC is DrawContextGLOrtho))
        {
            return;
        }

        vector3_t pt = default;

        // p0
        pt.X = -w / 2 + center.X;
        pt.Y = h / 2 + center.Y;
        pt.Z = 0;

        vector3_t p0 = default;
        p0.X = pt.X * DC.UnitPerMilli;
        p0.Y = pt.Y * DC.UnitPerMilli;

        p0 += DC.ViewOrg;

        // p1
        pt.X = w / 2 + center.X;
        pt.Y = -h / 2 + center.Y;
        pt.Z = 0;

        vector3_t p1 = default;
        p1.X = pt.X * DC.UnitPerMilli;
        p1.Y = pt.Y * DC.UnitPerMilli;

        p1 += DC.ViewOrg;

        GL.Enable(EnableCap.LineStipple);
        //GL.LineStipple(1, 0b1100110011001100);

        DrawRectScrn(DC.GetPen(DrawTools.PEN_PAGE_FRAME), p0, p1);

        GL.Disable(EnableCap.LineStipple);
    }

    public void EnableLight()
    {
        DC.EnableLight();
    }

    public void DisableLight()
    {
        DC.DisableLight();
    }

    public void DrawBouncingBox(DrawPen pen, MinMax3D mm)
    {
        vector3_t p0 = new vector3_t(mm.Min.X, mm.Min.Y, mm.Min.Z);
        vector3_t p1 = new vector3_t(mm.Min.X, mm.Min.Y, mm.Max.Z);
        vector3_t p2 = new vector3_t(mm.Max.X, mm.Min.Y, mm.Max.Z);
        vector3_t p3 = new vector3_t(mm.Max.X, mm.Min.Y, mm.Min.Z);

        vector3_t p4 = new vector3_t(mm.Min.X, mm.Max.Y, mm.Min.Z);
        vector3_t p5 = new vector3_t(mm.Min.X, mm.Max.Y, mm.Max.Z);
        vector3_t p6 = new vector3_t(mm.Max.X, mm.Max.Y, mm.Max.Z);
        vector3_t p7 = new vector3_t(mm.Max.X, mm.Max.Y, mm.Min.Z);

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

    public void DrawCrossScrn(DrawPen pen, vector3_t p, vcompo_t size)
    {
        Start2D();
        DrawCross(pen, p, size);
        End2D();
    }

    public void DrawArrow(DrawPen pen, vector3_t pt0, vector3_t pt1, ArrowTypes type, ArrowPos pos, vcompo_t len, vcompo_t width)
    {
        GL.Begin(PrimitiveType.Lines);
        DrawArrowGL(pen, pt0, pt1, type, pos, len, width, false);
        GL.End();
    }

    public void DrawExtSnapPoints(Vector3List pointList, DrawPen pen)
    {
        GL.Disable(EnableCap.Lighting);
        GL.Disable(EnableCap.Light0);

        Start2D();

        GL.LineWidth(DrawSizes.ExtSnapPointLineWidth);
        GL.Color4(pen.Color4);

        pointList.ForEach(v =>
        {
            DrawCross2D(DC.WorldPointToDevPoint(v), DrawSizes.ExtSnapPointLineLength);
        });

        GL.LineWidth(1);

        End2D();
    }

    public void DrawArrowGL(
        in DrawPen pen,
        vector3_t pt0,
        vector3_t pt1,
        ArrowTypes type,
        ArrowPos pos,
        vcompo_t len,
        vcompo_t width,
        bool continious)
    {
        GL.Color4(pen.Color4);
        if (!continious)
        {
            GL.Begin(PrimitiveType.Lines);
        }

        //drawing.DrawLine(pen, pt0, pt1);
        GL.Vertex3(pt0);
        GL.Vertex3(pt1);

        vector3_t d = pt1 - pt0;

        vcompo_t dl = d.Length;

        if (dl < (vcompo_t)(0.00001))
        {
            return;
        }


        vector3_t tmp = new vector3_t(dl, 0, 0);

        vcompo_t angle = vector3_t.CalculateAngle(tmp, d);

        vector3_t normal = CadMath.OuterProduct(tmp, d);  // 回転軸

        if (normal.Length < (vcompo_t)(0.0001))
        {
            normal = new vector3_t(0, 0, 1);
        }
        else
        {
            normal = normal.UnitVector();
            normal = CadMath.Normal(tmp, d);
        }

        CadQuaternion q = CadQuaternion.RotateQuaternion(normal, -angle);
        CadQuaternion r = q.Conjugate();

        ArrowHead a;

        if (pos == ArrowPos.END || pos == ArrowPos.START_END)
        {
            a = ArrowHead.Create(type, ArrowPos.END, len, width);

            a.Rotate(q, r);

            a += pt1;

            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p1.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p2.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p3.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p4.vector);

            //drawing.DrawLine(pen, a.p0.vector, a.p1.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p2.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p3.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p4.vector);
        }

        if (pos == ArrowPos.START || pos == ArrowPos.START_END)
        {
            a = ArrowHead.Create(type, ArrowPos.START, len, width);

            a.Rotate(q, r);

            a += pt0;

            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p1.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p2.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p3.vector);
            GL.Vertex3(a.p0.vector); GL.Vertex3(a.p4.vector);

            //drawing.DrawLine(pen, a.p0.vector, a.p1.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p2.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p3.vector);
            //drawing.DrawLine(pen, a.p0.vector, a.p4.vector);
        }

        if (!continious)
        {
            GL.End();
        }
    }
}
