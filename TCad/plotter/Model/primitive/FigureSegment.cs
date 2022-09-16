using CadDataTypes;

namespace Plotter
{
    //public struct PointPair
    //{
    //    public CadVertex P0;
    //    public CadVertex P1;

    //    public PointPair(CadVertex p0, CadVertex p1)
    //    {
    //        P0 = p0;
    //        P1 = p1;
    //    }
    //}

    public struct FigureSegment
    {
        public CadFigure Figure;
        public int SegIndex;
        public int Index0;
        public int Index1;

        public static FigureSegment InvalidValue = new FigureSegment(null, -1, -1, -1);

        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public CadVertex Point0
        {
            get
            {
                return Figure.GetPointAt(Index0);
            }

        }

        public CadVertex Point1
        {
            get
            {
                return Figure.GetPointAt(Index1);
            }
        }

        public CadVertex StoredPoint0
        {
            get
            {
                return Figure.GetStorePointAt(Index0);
            }

        }

        public CadVertex StoredPoint1
        {
            get
            {
                return Figure.GetStorePointAt(Index1);
            }
        }

        public CadSegment Segment
        {
            get
            {
                return Figure.GetSegmentAt(SegIndex);
            }

        }

        public FigureSegment(CadFigure fig, int segIndex, int a, int b)
        {
            Figure = fig;
            SegIndex = segIndex;
            Index0 = a;
            Index1 = b;
        }

        public void dump(string name = "FigureSegment")
        {
            DOut.pl(name + "{");
            DOut.Indent++;
            DOut.pl("FigureID:" + Figure.ID.ToString());
            DOut.pl("SegIndex:" + SegIndex.ToString());
            DOut.pl("Index0:" + Index0.ToString());
            DOut.pl("Index1:" + Index1.ToString());
            DOut.Indent--;
            DOut.pl("}");

        }
    }
}