using System.Text;
using System.Net.Sockets;

namespace DebugClient;

public class DebugClient
{
    public string DefaultAddr = "127.0.0.1";
    public int DefaultPort = 2300;

    private NetworkStream? NetStream;
    private StreamReader? Reader;
    private TcpClient? Client;

    public DebugClient()
    {
    }

    public void Start()
    {
        Start(DefaultAddr, DefaultPort);
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

                if (s == null)
                {
                    Console.WriteLine("");
                    Console.WriteLine("\r\nDisonnected.\r\n");
                    Console.WriteLine("");
                    return;
                }
            }
            catch
            {
                Console.WriteLine("");
                Console.WriteLine("Disconnected.");
                Console.WriteLine("");
                break;
            }

            Console.WriteLine(s);
        }
    }

    public void ConnectServer(string ipAdder, int port)
    {
        Client = new TcpClient();

        bool connected = false;
        Console.Write("Waiting DebugServer...");

        while (!connected)
        {
            Task t = Client.ConnectAsync(ipAdder, port);
            Task.WaitAny(t);
                
            if (Client.Connected)
            {
                Console.WriteLine("Connected!!");
                connected = true;
                break;
            }
        }

        NetStream = Client.GetStream();
        Reader = new StreamReader(NetStream, Encoding.UTF8);
    }
}
