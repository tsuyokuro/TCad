using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace DebugClient
{
    public class DebugClient
    {
        NetworkStream? NetStream;
        StreamReader? Reader;
        TcpClient? Client;

        public DebugClient()
        {
        }

        public void Start(string ipAdder, int port)
        {
            while (true)
            {
                try
                {
                    ConnectServer(ipAdder, port);
                    Reading();
                    if (Client != null)
                    {
                        Client.Dispose();
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void Reading()
        {
            while (true)
            {
                if (Reader == null)
                {
                    break;
                }

                string? s;

                try
                {
                    s = Reader.ReadLine();
                }
                catch
                {
                    Console.WriteLine("Disconnected.");
                    break;
                }

                Console.WriteLine(s);
            }
        }

        public void ConnectServer(string ipAdder, int port)
        {
            Client = new TcpClient();

            bool connected = false;
            Console.WriteLine("Connecting...");

            while (!connected)
            {
                try
                {
                    Client.Connect(ipAdder, port);

                    if (Client.Connected)
                    {
                        Console.WriteLine("Connected!!");
                        connected = true;
                        break;
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }

            NetStream = Client.GetStream();
            Reader = new StreamReader(NetStream, Encoding.UTF8);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string ipAdder = "127.0.0.1";
            int port = 2300;

            DebugClient client = new DebugClient();
            client.Start(ipAdder, port);
        }
    }
}
