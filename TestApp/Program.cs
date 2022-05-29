using MessagePack;
using System;
using Plotter;
using CadDataTypes;
using Plotter.Serializer.v1002;

namespace TestApp
{
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
        }

        public void OnAfterDeserialize()
        {
            L = Math.Sqrt(X * X + Y * Y);
        }
    }


    [MessagePackObject]
    public class MpTest1
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;
    }

    [MessagePackObject]
    public class MpTest2 : IMessagePackSerializationCallbackReceiver
    {
        [Key("X")]
        public double X;

        [Key("Y")]
        public double Y;

        [Key("Version")]
        public byte[] Version = { 1, 0, 0, 2 };


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
            MpTest1 mpt = new MpTest1();

            mpt.X = 10;
            mpt.Y = 5;


            byte[] data = MessagePackSerializer.Serialize(mpt);


            MpTest2 re = MessagePackSerializer.Deserialize<MpTest2>(data);



            Console.WriteLine("end");
        }
    }
}
