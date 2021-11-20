using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace DebugServer
{
    public class ClientTask
    {
        NetworkStream NetStream;
        TaskContext Context;

        public void Start(TaskContext context, TcpClient client)
        {
            Context = context;
            NetStream = client.GetStream();

            Task.Run(MainLoop);
        }

        private void MainLoop()
        {
            Context.WriteLine($"Start client");

            StreamReader reader = new StreamReader(NetStream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(NetStream, Encoding.UTF8);

            while (true)
            {
                string s = reader.ReadLine();

                if (s == null)
                {
                    break;
                }

                Context.WriteLine(s);
            }
        }
    }

    public class TaskContext
    {
        Object WriteLock = new object();

        public void WriteLine(string s)
        {
            lock (WriteLock)
            {
                Console.WriteLine(s);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //string ipString = "127.0.0.1";
            //System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);

            string host = "localhost";
            System.Net.IPAddress ipAdd =
                System.Net.Dns.GetHostEntry(host).AddressList[0];

            //Listenするポート番号
            int port = 2001;

            //TcpListenerオブジェクトを作成する
            System.Net.Sockets.TcpListener listener =
                new System.Net.Sockets.TcpListener(ipAdd, port);

            //Listenを開始する
            listener.Start();

            Console.WriteLine($"Debug server started. port:{port}");

            TaskContext tc = new TaskContext();

            while (true)
            {
                Console.WriteLine($"Waiting client");
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine($"Client is connected");

                ClientTask ct = new ClientTask();
                ct.Start(tc, client);
            }
        }
    }
}
