using MessagePack;
using System;
using Plotter;
using CadDataTypes;
using Plotter.Serializer.v1002;

namespace TestApp
{
    [MessagePackObject]
    public class MpTest : IMessagePackSerializationCallbackReceiver
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;

        [IgnoreMember]
        public double L = 0;

        public void OnBeforeSerialize()
        {
            // NOP
        }

        public void OnAfterDeserialize()
        {
            L = Math.Sqrt(X * X + Y * Y);
        }
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            MpTest mpt = new MpTest();

            mpt.X = 10;
            mpt.Y = 5;


            byte[] data = MessagePackSerializer.Serialize(mpt);


            MpTest re = MessagePackSerializer.Deserialize<MpTest>(data);


            Console.WriteLine("re.L:" + re.L);

            CadFigurePolyLines polyLines = new CadFigurePolyLines();

            polyLines.AddPoint(new CadVertex(1, 1, 1));

            MpFigure_v1002 mpFig = MpFigure_v1002.Create(polyLines);

            data = MessagePackSerializer.Serialize(mpFig);

            MpFigure_v1002 mpFigR = MessagePackSerializer.Deserialize<MpFigure_v1002>(data);

            CadFigurePolyLines polyLinesR = new CadFigurePolyLines();

            mpFigR.RestoreTo(polyLinesR);

            Console.WriteLine("end");
        }
    }
}
