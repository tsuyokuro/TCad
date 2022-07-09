using MessagePack;
using System;
using Plotter;
using CadDataTypes;
using Plotter.Serializer.v1002;
using Plotter.Serializer;
using TCad;
using System.Threading.Tasks;
using System.Threading;

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
            Win32Window wnd = new Win32Window();
            wnd.Create("Test");
            wnd.ShowWindow();
            wnd.StartMessageLoop();


            //Task task1 = Task.Run(() =>
            //{
            //    Win32Window wnd = new Win32Window();
            //    wnd.Create("Test");
            //    wnd.ShowWindow();
            //    wnd.StartMessageLoop();
            //});


            //Win32Window wnd2 = new Win32Window();

            //Task task2 = Task.Run(() =>
            //{
            //    Win32Window wnd2 = new Win32Window();
            //    wnd2.Create("Test2");
            //    wnd2.ShowWindow();
            //    wnd2.StartMessageLoop();
            //});

            Console.WriteLine("press enter");
            Console.ReadLine();
            Console.WriteLine("end");
        }
    }
}
