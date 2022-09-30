using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Plotter;

public class GDIToolManager : IDisposable
{
    public readonly struct PenKey : IEquatable<PenKey>
    {
        private readonly Color4 Color4;
        private readonly float W;

        public PenKey(DrawPen dp)
        {
            Color4 = dp.Color4();
            W = dp.Width;
        }

        public bool Equals(PenKey other)
        {
            return Color4 == other.Color4 && W == other.W;
        }

        public override bool Equals(object obj)
        {
            return obj is PenKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Color4.A,
                Color4.R,
                Color4.G,
                Color4.B,
                W
                );
        }
    }

    public readonly struct BrushKey : IEquatable<BrushKey>
    {
        private readonly Color4 Color4;

        public BrushKey(DrawBrush db)
        {
            Color4 = db.Color4();
        }

        public bool Equals(BrushKey other)
        {
            return Color4 == other.Color4;
        }

        public override bool Equals(object obj)
        {
            return obj is BrushKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Color4.A,
                Color4.R,
                Color4.G,
                Color4.B
                );
        }
    }

    private static GDIToolManager sInstance;

    public static GDIToolManager Instance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            if (sInstance == null)
            {
                sInstance = new GDIToolManager();
            }

            return sInstance;
        }
    }

    private Dictionary<PenKey, Pen> PenMap = new Dictionary<PenKey, Pen>();
    private Dictionary<BrushKey, SolidBrush> BrushMap = new Dictionary<BrushKey, SolidBrush>();

    private GDIToolManager(){}

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Pen Pen(in DrawPen dp)
    {
        PenKey key = new PenKey(dp);

        if (!PenMap.ContainsKey(key)) {
            Pen pen = new Pen(ColorUtil.ToGDIColor(dp.Color4()), dp.Width);
            PenMap.Add(key, pen);
        }

        return PenMap[key];
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public SolidBrush Brush(in DrawBrush db)
    {
        BrushKey key = new BrushKey(db);

        if (!BrushMap.ContainsKey(key))
        {
            SolidBrush brush = new SolidBrush(ColorUtil.ToGDIColor(db.Color4()));
            BrushMap.Add(key, brush);
        }

        return BrushMap[key];
    }

    public void Clear()
    {
        foreach (Pen p in PenMap.Values)
        {
            p.Dispose();
        }
        PenMap.Clear(); 

        foreach (Brush b in BrushMap.Values)
        {
            b.Dispose();
        }
        BrushMap.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
