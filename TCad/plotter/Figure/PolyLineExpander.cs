using CadDataTypes;
using System;

namespace Plotter
{
    public static class PolyLineExpander
    {
        public static VertexList GetExpandList(
            VertexList src,
            int curveSplitNum)
        {
            int cnt = src.Count;

            VertexList ret = new VertexList(curveSplitNum * ((cnt + 1) / 2));

            ForEachPoints<Object>(src, curveSplitNum, (v, d) => { ret.Add(v); }, null);

            return ret;
        }

        private enum ScanState
        {
            START,
            MAIN,
            HANDLE_1,
            HANDLE_2,

        }

        #region For each
        public static CadVertex ForEachPoints<T>(
            VertexList src,
            int curveSplitNum,
            Action<CadVertex, T> action, T param)
        {
            VertexList pl = src;

            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[0];

            int i = 0;

            ScanState state = ScanState.START;

            for (; i < cnt; i++)
            {
                switch (state)
                {
                    case ScanState.START:
                        p0 = src[i];
                        action(p0, param);

                        state = ScanState.MAIN;
                        break;

                    case ScanState.MAIN:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_1;
                        }
                        else
                        {
                            p0 = pl[i];
                            action(p0, param);
                        }
                        break;

                    case ScanState.HANDLE_1:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_2;
                        }
                        else
                        {
                            ForEachBezierPoints3<T>(pl[i - 2], pl[i - 1], pl[i], curveSplitNum, true, action, param);
                            p0 = pl[i];
                            action(p0, param);
                            state = ScanState.MAIN;
                        }
                        break;

                    case ScanState.HANDLE_2:
                        ForEachBezierPoints4<T>(pl[i - 3], pl[i - 2], pl[i - 1], pl[i], curveSplitNum, true, action, param);
                        p0 = pl[i];
                        action(p0, param);
                        state = ScanState.MAIN;
                        break;
                }
            }

            switch (state)
            {
                case ScanState.MAIN:
                    break;
                case ScanState.HANDLE_1:
                    p0 = ForEachBezierPoints3<T>(pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, true, action, param);
                    break;

                case ScanState.HANDLE_2:
                    p0 = ForEachBezierPoints4<T>(pl[cnt - 3], pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, true, action, param);
                    break;
            }

            return p0;
        }

        public static CadVertex ForEachSegs<T>(
            VertexList src, bool isloop,
            int curveSplitNum,
            Action<CadVertex, CadVertex, T> action, T param)
        {
            VertexList pl = src;

            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[0];

            int i = 0;

            ScanState state = ScanState.START;

            for (; i < cnt; i++)
            {
                switch (state)
                {
                    case ScanState.START:
                        p0 = src[i];
                        state = ScanState.MAIN;
                        break;

                    case ScanState.MAIN:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_1;
                        }
                        else
                        {
                            action(p0, pl[i], param);
                            p0 = pl[i];
                        }
                        break;

                    case ScanState.HANDLE_1:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_2;
                        }
                        else
                        {
                            p0 = ForEachBezierSegs3<T>(pl[i - 2], pl[i - 1], pl[i], curveSplitNum, action, param);
                            state = ScanState.MAIN;
                        }
                        break;

                    case ScanState.HANDLE_2:
                        p0 = ForEachBezierSegs4<T>(pl[i - 3], pl[i - 2], pl[i - 1], pl[i], curveSplitNum, action, param);
                        state = ScanState.MAIN;
                        break;
                }
            }

            switch (state)
            {
                case ScanState.MAIN:
                    if (isloop)
                    {
                        action(p0, pl[0], param);
                    }
                    break;

                case ScanState.HANDLE_1:
                    p0 = ForEachBezierSegs3<T>(pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
                    break;
                case ScanState.HANDLE_2:
                    p0 = ForEachBezierSegs4<T>(pl[cnt - 3], pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
                    break;
            }

            return p0;
        }

        private static CadVertex ForEachBezierPoints3<T>(
            CadVertex p0, CadVertex p1, CadVertex p2, int scnt, bool excludeEdge, Action<CadVertex, T> action, T param)
        {
            double t = 0;
            double d = 1.0 / (double)scnt;
            double e = 1.0;

            t = d;


            int n = 3;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            if (excludeEdge)
            {
                e -= d;
            }
            else
            {
                action(t0, param);
            }

            while (t <= e)
            {
                t1 = default(CadVertex);
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);

                action(t1, param);

                t += d;
            }

            return t1;
        }

        private static CadVertex ForEachBezierPoints4<T>(
            CadVertex p0, CadVertex p1, CadVertex p2, CadVertex p3, int scnt, bool excludeEdge, Action<CadVertex, T> action, T param)
        {
            double t = 0;
            double d = 1.0 / (double)scnt;
            double e = 1.0;

            t = d;

            int n = 4;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            if (excludeEdge)
            {
                e -= d;
            }
            else
            {
                action(t0, param);
            }

            while (t <= e)
            {
                t1 = default(CadVertex);
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * BezierFuncs.BernsteinBasisF(n - 1, 3, t);

                action(t1, param);

                t += d;
            }

            return t1;
        }

        private static CadVertex ForEachBezierSegs3<T>(
            CadVertex p0, CadVertex p1, CadVertex p2, int s, Action<CadVertex, CadVertex, T> action, T param)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 3;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            while (t <= 1.0)
            {
                t1 = default;
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);

                action(t0, t1, param);

                t0 = t1;

                t += d;
            }

            return t1;
        }

        private static CadVertex ForEachBezierSegs4<T>(
            CadVertex p0, CadVertex p1, CadVertex p2, CadVertex p3, int s, Action<CadVertex, CadVertex, T> action, T param)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 4;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            while (t <= 1.0)
            {
                t1 = default;
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * BezierFuncs.BernsteinBasisF(n - 1, 3, t);

                action(t0, t1, param);

                t0 = t1;

                t += d;
            }

            return t1;
        }
        #endregion

        #region Draw
        public static CadVertex Draw(
            VertexList src, bool isloop,
            int curveSplitNum,
            DrawContext dc, DrawPen pen)
        {
            VertexList pl = src;

            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[0];

            int i = 0;

            ScanState state = ScanState.START;

            for (; i < cnt; i++)
            {
                switch (state)
                {
                    case ScanState.START:
                        p0 = src[i];
                        state = ScanState.MAIN;
                        break;

                    case ScanState.MAIN:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_1;
                        }
                        else
                        {
                            dc.Drawing.DrawLine(pen, p0.vector, pl[i].vector);
                            p0 = pl[i];
                        }
                        break;

                    case ScanState.HANDLE_1:
                        if (pl[i].IsHandle)
                        {
                            state = ScanState.HANDLE_2;
                        }
                        else
                        {
                            p0 = DrawBezier3(pl[i - 2], pl[i - 1], pl[i], curveSplitNum, dc, pen);
                            state = ScanState.MAIN;
                        }
                        break;

                    case ScanState.HANDLE_2:
                        p0 = DrawBezier4(pl[i - 3], pl[i - 2], pl[i - 1], pl[i], curveSplitNum, dc, pen);
                        state = ScanState.MAIN;
                        break;
                }
            }

            switch (state)
            {
                case ScanState.MAIN:
                    if (isloop)
                    {
                        dc.Drawing.DrawLine(pen, p0.vector, pl[0].vector);
                    }
                    break;

                case ScanState.HANDLE_1:
                    p0 = DrawBezier3(pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, dc, pen);
                    break;
                case ScanState.HANDLE_2:
                    p0 = DrawBezier4(pl[cnt - 3], pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, dc, pen);
                    break;
            }

            return p0;
        }

        public static CadVertex DrawBezier3(
            CadVertex p0, CadVertex p1, CadVertex p2, int s, DrawContext dc, DrawPen pen)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 3;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            while (t <= 1.0)
            {
                t1 = default;
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);

                dc.Drawing.DrawLine(pen, t0.vector, t1.vector);

                t0 = t1;

                t += d;
            }

            return t1;
        }

        public static CadVertex DrawBezier4(
            CadVertex p0, CadVertex p1, CadVertex p2, CadVertex p3, int s, DrawContext dc, DrawPen pen)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 4;

            CadVertex t0 = p0;
            CadVertex t1 = p0;

            while (t <= 1.0)
            {
                t1 = default;
                t1 += p0 * BezierFuncs.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BezierFuncs.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BezierFuncs.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * BezierFuncs.BernsteinBasisF(n - 1, 3, t);

                dc.Drawing.DrawLine(pen, t0.vector, t1.vector);

                t0 = t1;

                t += d;
            }

            return t1;
        }
        #endregion
    }
}
